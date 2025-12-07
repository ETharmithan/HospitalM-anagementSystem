import { Component, OnInit, OnDestroy, inject, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatSession, ChatMessage, ChatRequest, DoctorChatAvailability, VideoCallInfo } from '../../core/services/chat-service';
import { AccountService } from '../../core/services/account-service';
import { ToastService } from '../../core/services/toast-service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css'
})
export class ChatComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);
  private accountService = inject(AccountService);
  private toastService = inject(ToastService);

  @ViewChild('messagesContainer') messagesContainer!: ElementRef;
  @ViewChild('localVideo') localVideo!: ElementRef<HTMLVideoElement>;
  @ViewChild('remoteVideo') remoteVideo!: ElementRef<HTMLVideoElement>;

  // State
  activeView = signal<'list' | 'chat' | 'doctors'>('list');
  currentSession = signal<ChatSession | null>(null);
  newMessage = signal<string>('');
  isTyping = signal<boolean>(false);
  typingUser = signal<string>('');
  
  // Video call state
  isInCall = signal<boolean>(false);
  incomingCall = signal<VideoCallInfo | null>(null);
  localStream: MediaStream | null = null;
  peerConnection: RTCPeerConnection | null = null;

  // Subscriptions
  private subscriptions: Subscription[] = [];

  // Computed
  sessions = computed(() => this.chatService.sessions());
  messages = computed(() => this.chatService.messages());
  pendingRequests = computed(() => this.chatService.pendingRequests());
  availableDoctors = computed(() => this.chatService.availableDoctors());
  allDoctors = computed(() => this.chatService.availableDoctors()); // Using same signal for all doctors
  unreadCount = computed(() => this.chatService.unreadCount());
  isConnected = computed(() => this.chatService.isConnected());
  myChatAvailability = computed(() => this.chatService.myChatAvailability());

  // User info
  get currentUserId(): string {
    return this.accountService.currentUser()?.id || '';
  }

  get currentUserRole(): string {
    return this.accountService.currentUser()?.role || 'Patient';
  }

  get isDoctor(): boolean {
    return this.currentUserRole === 'Doctor';
  }

  ngOnInit(): void {
    this.initializeChat();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.chatService.stopConnection();
    this.cleanupVideoCall();
  }

  async initializeChat(): Promise<void> {
    await this.chatService.startConnection();
    this.chatService.loadSessions();
    this.chatService.loadUnreadCount();
    
    if (this.isDoctor) {
      this.chatService.loadPendingRequests();
    } else {
      this.chatService.loadAvailableDoctors();
    }

    this.setupEventListeners();
  }

  private setupEventListeners(): void {
    // Message received
    this.subscriptions.push(
      this.chatService.onMessageReceived.subscribe(message => {
        if (this.currentSession()?.sessionId === message.sessionId) {
          this.scrollToBottom();
          this.chatService.markMessagesAsRead(message.sessionId);
        }
      })
    );

    // Typing indicator
    this.subscriptions.push(
      this.chatService.onUserTyping.subscribe(({ userId, userName }) => {
        if (userId !== this.currentUserId && userName) {
          this.typingUser.set(userName);
          setTimeout(() => this.typingUser.set(''), 3000);
        } else {
          this.typingUser.set('');
        }
      })
    );

    // Chat request accepted
    this.subscriptions.push(
      this.chatService.onChatRequestAccepted.subscribe(response => {
        this.toastService.success('Chat request accepted!');
        this.openSession(response.sessionId);
      })
    );

    // Chat request declined
    this.subscriptions.push(
      this.chatService.onChatRequestDeclined.subscribe(request => {
        this.toastService.error(`Chat request declined: ${request.declineReason || 'No reason provided'}`);
      })
    );

    // Incoming video call
    this.subscriptions.push(
      this.chatService.onIncomingCall.subscribe(callInfo => {
        this.incomingCall.set(callInfo);
      })
    );

    // Call accepted
    this.subscriptions.push(
      this.chatService.onCallAccepted.subscribe(async ({ sessionId }) => {
        await this.startWebRTC(true);
      })
    );

    // Call declined
    this.subscriptions.push(
      this.chatService.onCallDeclined.subscribe(() => {
        this.toastService.info('Call was declined');
        this.cleanupVideoCall();
      })
    );

    // Call ended
    this.subscriptions.push(
      this.chatService.onCallEnded.subscribe(() => {
        this.toastService.info('Call ended');
        this.cleanupVideoCall();
      })
    );

    // WebRTC signaling
    this.subscriptions.push(
      this.chatService.onOfferReceived.subscribe(async signal => {
        await this.handleOffer(signal);
      })
    );

    this.subscriptions.push(
      this.chatService.onAnswerReceived.subscribe(async signal => {
        await this.handleAnswer(signal);
      })
    );

    this.subscriptions.push(
      this.chatService.onIceCandidateReceived.subscribe(async signal => {
        await this.handleIceCandidate(signal);
      })
    );
  }

  // ==================== NAVIGATION ====================

  showSessionList(): void {
    this.activeView.set('list');
    this.currentSession.set(null);
  }

  showAvailableDoctors(): void {
    this.activeView.set('doctors');
    this.chatService.loadAvailableDoctors();
  }

  async openSession(sessionId: string): Promise<void> {
    const session = this.sessions().find(s => s.sessionId === sessionId);
    if (session) {
      this.currentSession.set(session);
      this.activeView.set('chat');
      this.chatService.loadMessages(sessionId);
      await this.chatService.joinSession(sessionId);
      await this.chatService.markMessagesAsRead(sessionId);
      setTimeout(() => this.scrollToBottom(), 100);
    } else {
      // Load session from API
      this.chatService.getSession(sessionId).subscribe({
        next: async (session) => {
          this.currentSession.set(session);
          this.activeView.set('chat');
          this.chatService.loadMessages(sessionId);
          await this.chatService.joinSession(sessionId);
          await this.chatService.markMessagesAsRead(sessionId);
          setTimeout(() => this.scrollToBottom(), 100);
        },
        error: () => this.toastService.error('Failed to open chat session')
      });
    }
  }

  // ==================== MESSAGING ====================

  async sendMessage(): Promise<void> {
    const content = this.newMessage().trim();
    const session = this.currentSession();
    
    if (!content || !session) return;

    await this.chatService.sendMessage(session.sessionId, content);
    this.newMessage.set('');
    this.scrollToBottom();
  }

  onMessageInput(): void {
    const session = this.currentSession();
    if (!session) return;

    if (!this.isTyping()) {
      this.isTyping.set(true);
      this.chatService.startTyping(session.sessionId);
    }

    // Debounce stop typing
    setTimeout(() => {
      this.isTyping.set(false);
      this.chatService.stopTyping(session.sessionId);
    }, 2000);
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }

  // ==================== CHAT REQUESTS ====================

  async requestChat(doctor: DoctorChatAvailability, type: 'Text' | 'Video'): Promise<void> {
    try {
      await this.chatService.requestChat(doctor.doctorId, type, `Hi Dr. ${doctor.doctorName}, I would like to chat with you.`);
      this.toastService.success('Chat request sent! Waiting for doctor to accept...');
    } catch (error) {
      this.toastService.error('Failed to send chat request');
    }
  }

  async startDirectChat(doctor: any): Promise<void> {
    try {
      // Check if session already exists
      const existingSession = this.sessions().find(
        s => s.doctorId === doctor.doctorId
      );

      if (existingSession) {
        this.openSession(existingSession.sessionId);
        return;
      }

      // Create new direct session
      this.chatService.createDirectSession(doctor.doctorId).subscribe({
        next: (session) => {
          this.chatService.loadSessions();
          this.toastService.success(`Chat started with ${doctor.doctorName}`);
          setTimeout(() => this.openSession(session.sessionId), 500);
        },
        error: (err) => {
          console.error('Failed to create session:', err);
          this.toastService.error('Failed to start chat. Please try again.');
        }
      });
    } catch (error) {
      this.toastService.error('Failed to start chat');
    }
  }

  async acceptRequest(request: ChatRequest): Promise<void> {
    try {
      await this.chatService.acceptChatRequest(request.requestId);
      this.toastService.success('Chat request accepted');
    } catch (error) {
      this.toastService.error('Failed to accept request');
    }
  }

  async declineRequest(request: ChatRequest): Promise<void> {
    const reason = prompt('Reason for declining (optional):');
    try {
      await this.chatService.declineChatRequest(request.requestId, reason || undefined);
      this.toastService.info('Chat request declined');
    } catch (error) {
      this.toastService.error('Failed to decline request');
    }
  }

  // ==================== DOCTOR AVAILABILITY ====================

  async toggleChatAvailability(): Promise<void> {
    const current = this.myChatAvailability();
    await this.chatService.setChatAvailability(
      !current.isAvailableForChat,
      current.isAvailableForVideo,
      current.statusMessage
    );
  }

  async toggleVideoAvailability(): Promise<void> {
    const current = this.myChatAvailability();
    await this.chatService.setChatAvailability(
      current.isAvailableForChat,
      !current.isAvailableForVideo,
      current.statusMessage
    );
  }

  // ==================== VIDEO CALLING ====================

  async startVideoCall(): Promise<void> {
    const session = this.currentSession();
    if (!session) return;

    const targetUserId = this.isDoctor ? session.patientId : session.doctorId;
    if (!targetUserId) return;

    await this.chatService.initiateVideoCall(session.sessionId, targetUserId);
    this.toastService.info('Calling...');
  }

  async acceptIncomingCall(): Promise<void> {
    const call = this.incomingCall();
    if (!call) return;

    await this.chatService.acceptVideoCall(call.sessionId, call.callerId);
    this.incomingCall.set(null);
    await this.startWebRTC(false);
  }

  async declineIncomingCall(): Promise<void> {
    const call = this.incomingCall();
    if (!call) return;

    await this.chatService.declineVideoCall(call.sessionId, call.callerId);
    this.incomingCall.set(null);
  }

  async endCall(): Promise<void> {
    const session = this.currentSession();
    if (session) {
      await this.chatService.endVideoCall(session.sessionId);
    }
    this.cleanupVideoCall();
  }

  // ==================== WEBRTC ====================

  private async startWebRTC(isInitiator: boolean): Promise<void> {
    try {
      this.localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
      
      if (this.localVideo) {
        this.localVideo.nativeElement.srcObject = this.localStream;
      }

      this.peerConnection = new RTCPeerConnection({
        iceServers: [
          { urls: 'stun:stun.l.google.com:19302' },
          { urls: 'stun:stun1.l.google.com:19302' }
        ]
      });

      // Add local tracks
      this.localStream.getTracks().forEach(track => {
        this.peerConnection!.addTrack(track, this.localStream!);
      });

      // Handle remote tracks
      this.peerConnection.ontrack = (event) => {
        if (this.remoteVideo) {
          this.remoteVideo.nativeElement.srcObject = event.streams[0];
        }
      };

      // Handle ICE candidates
      this.peerConnection.onicecandidate = async (event) => {
        if (event.candidate) {
          const session = this.currentSession();
          const targetUserId = this.isDoctor ? session?.patientId : session?.doctorId;
          if (session && targetUserId) {
            await this.chatService.sendIceCandidate(session.sessionId, targetUserId, event.candidate.toJSON());
          }
        }
      };

      this.isInCall.set(true);

      // If initiator, create and send offer
      if (isInitiator) {
        const offer = await this.peerConnection.createOffer();
        await this.peerConnection.setLocalDescription(offer);
        
        const session = this.currentSession();
        const targetUserId = this.isDoctor ? session?.patientId : session?.doctorId;
        if (session && targetUserId) {
          await this.chatService.sendOffer(session.sessionId, targetUserId, offer);
        }
      }
    } catch (error) {
      console.error('Failed to start WebRTC:', error);
      this.toastService.error('Failed to access camera/microphone');
      this.cleanupVideoCall();
    }
  }

  private async handleOffer(signal: any): Promise<void> {
    if (!this.peerConnection) {
      await this.startWebRTC(false);
    }

    if (this.peerConnection && signal.Offer) {
      await this.peerConnection.setRemoteDescription(new RTCSessionDescription(signal.Offer));
      const answer = await this.peerConnection.createAnswer();
      await this.peerConnection.setLocalDescription(answer);

      const session = this.currentSession();
      if (session) {
        await this.chatService.sendAnswer(session.sessionId, signal.FromUserId, answer);
      }
    }
  }

  private async handleAnswer(signal: any): Promise<void> {
    if (this.peerConnection && signal.Answer) {
      await this.peerConnection.setRemoteDescription(new RTCSessionDescription(signal.Answer));
    }
  }

  private async handleIceCandidate(signal: any): Promise<void> {
    if (this.peerConnection && signal.Candidate) {
      await this.peerConnection.addIceCandidate(new RTCIceCandidate(signal.Candidate));
    }
  }

  private cleanupVideoCall(): void {
    this.isInCall.set(false);
    this.incomingCall.set(null);

    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = null;
    }

    if (this.peerConnection) {
      this.peerConnection.close();
      this.peerConnection = null;
    }

    if (this.localVideo) {
      this.localVideo.nativeElement.srcObject = null;
    }

    if (this.remoteVideo) {
      this.remoteVideo.nativeElement.srcObject = null;
    }
  }

  // ==================== HELPERS ====================

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    const today = new Date();
    
    if (date.toDateString() === today.toDateString()) {
      return 'Today';
    }
    
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    if (date.toDateString() === yesterday.toDateString()) {
      return 'Yesterday';
    }
    
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  isMyMessage(message: ChatMessage): boolean {
    return message.senderId === this.currentUserId;
  }

  getOtherParticipantName(session: ChatSession): string {
    if (this.isDoctor) {
      return session.patientName;
    }
    return session.doctorName || session.adminName || 'Unknown';
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'online': return '#22c55e';
      case 'busy': return '#f59e0b';
      case 'away': return '#6b7280';
      default: return '#ef4444';
    }
  }
}
