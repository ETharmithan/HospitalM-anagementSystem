import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AccountService } from './account-service';

// ==================== INTERFACES ====================

export interface ChatSession {
  sessionId: string;
  patientId: string;
  patientName: string;
  doctorId?: string;
  doctorName?: string;
  adminId?: string;
  adminName?: string;
  sessionType: string;
  status: string;
  startedAt: string;
  endedAt?: string;
  lastMessageAt: string;
  lastMessage?: ChatMessage;
  unreadCount: number;
}

export interface ChatMessage {
  messageId: string;
  sessionId: string;
  senderId: string;
  senderType: string;
  senderName: string;
  content: string;
  messageType: string;
  attachmentUrl?: string;
  attachmentName?: string;
  isRead: boolean;
  readAt?: string;
  sentAt: string;
}

export interface ChatRequest {
  requestId: string;
  patientId: string;
  patientName: string;
  doctorId?: string;
  doctorName?: string;
  requestType: string;
  status: string;
  message?: string;
  declineReason?: string;
  requestedAt: string;
  respondedAt?: string;
  expiresAt: string;
  sessionId?: string;
}

export interface ChatRequestResponse {
  requestId: string;
  patientId: string;
  sessionId: string;
  status: string;
}

export interface DoctorChatAvailability {
  doctorId: string;
  doctorName: string;
  specialization?: string;
  profileImage?: string;
  isAvailableForChat: boolean;
  isAvailableForVideo: boolean;
  status: string;
  statusMessage?: string;
  lastOnlineAt: string;
  currentActiveChatCount: number;
  maxConcurrentChats: number;
}

export interface VideoCallInfo {
  sessionId: string;
  callerId: string;
  callerName: string;
}

export interface WebRTCSignal {
  sessionId: string;
  fromUserId: string;
  offer?: RTCSessionDescriptionInit;
  answer?: RTCSessionDescriptionInit;
  candidate?: RTCIceCandidateInit;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  
  private baseUrl = 'http://localhost:5245/api/chat';
  private hubUrl = 'http://localhost:5245/hubs/chat';
  
  private hubConnection: signalR.HubConnection | null = null;
  
  // Signals for reactive state
  sessions = signal<ChatSession[]>([]);
  currentSession = signal<ChatSession | null>(null);
  messages = signal<ChatMessage[]>([]);
  pendingRequests = signal<ChatRequest[]>([]);
  availableDoctors = signal<DoctorChatAvailability[]>([]);
  unreadCount = signal<number>(0);
  isConnected = signal<boolean>(false);
  
  // Chat availability for doctors
  myChatAvailability = signal<{ isAvailableForChat: boolean; isAvailableForVideo: boolean; statusMessage?: string }>({
    isAvailableForChat: false,
    isAvailableForVideo: false
  });

  // Event subjects for real-time updates
  private messageReceived$ = new Subject<ChatMessage>();
  private chatRequestReceived$ = new Subject<ChatRequest>();
  private chatRequestAccepted$ = new Subject<ChatRequestResponse>();
  private chatRequestDeclined$ = new Subject<ChatRequest>();
  private typingUser$ = new Subject<{ userId: string; userName: string }>();
  
  // Video call events
  private incomingCall$ = new Subject<VideoCallInfo>();
  private callAccepted$ = new Subject<{ sessionId: string; acceptedBy: string }>();
  private callDeclined$ = new Subject<{ sessionId: string; declinedBy: string }>();
  private callEnded$ = new Subject<string>();
  
  // WebRTC signaling events
  private offerReceived$ = new Subject<WebRTCSignal>();
  private answerReceived$ = new Subject<WebRTCSignal>();
  private iceCandidateReceived$ = new Subject<WebRTCSignal>();

  // Public observables
  onMessageReceived = this.messageReceived$.asObservable();
  onChatRequestReceived = this.chatRequestReceived$.asObservable();
  onChatRequestAccepted = this.chatRequestAccepted$.asObservable();
  onChatRequestDeclined = this.chatRequestDeclined$.asObservable();
  onUserTyping = this.typingUser$.asObservable();
  onIncomingCall = this.incomingCall$.asObservable();
  onCallAccepted = this.callAccepted$.asObservable();
  onCallDeclined = this.callDeclined$.asObservable();
  onCallEnded = this.callEnded$.asObservable();
  onOfferReceived = this.offerReceived$.asObservable();
  onAnswerReceived = this.answerReceived$.asObservable();
  onIceCandidateReceived = this.iceCandidateReceived$.asObservable();

  // ==================== SIGNALR CONNECTION ====================

  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = this.accountService.currentUser()?.token;
    if (!token) {
      console.error('No token available for SignalR connection');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerHubHandlers();

    try {
      await this.hubConnection.start();
      console.log('SignalR Connected');
      this.isConnected.set(true);
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      this.isConnected.set(false);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.isConnected.set(false);
    }
  }

  private registerHubHandlers(): void {
    if (!this.hubConnection) return;

    // Message handlers
    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      this.messages.update(msgs => [...msgs, message]);
      this.messageReceived$.next(message);
    });

    this.hubConnection.on('MessagesRead', (sessionId: string, userId: string) => {
      this.messages.update(msgs => 
        msgs.map(m => m.sessionId === sessionId && m.senderId !== userId 
          ? { ...m, isRead: true, readAt: new Date().toISOString() } 
          : m
        )
      );
    });

    // Typing indicators
    this.hubConnection.on('UserTyping', (userId: string, userName: string) => {
      this.typingUser$.next({ userId, userName });
    });

    this.hubConnection.on('UserStoppedTyping', (userId: string) => {
      this.typingUser$.next({ userId, userName: '' });
    });

    // Chat request handlers
    this.hubConnection.on('ChatRequestReceived', (request: ChatRequest) => {
      this.pendingRequests.update(reqs => [request, ...reqs]);
      this.chatRequestReceived$.next(request);
    });

    this.hubConnection.on('ChatRequestAccepted', (response: ChatRequestResponse) => {
      this.chatRequestAccepted$.next(response);
      this.loadSessions(); // Refresh sessions
    });

    this.hubConnection.on('ChatRequestDeclined', (request: ChatRequest) => {
      this.chatRequestDeclined$.next(request);
    });

    this.hubConnection.on('ChatRequestCancelled', (request: ChatRequest) => {
      this.pendingRequests.update(reqs => reqs.filter(r => r.requestId !== request.requestId));
    });

    // Doctor availability
    this.hubConnection.on('DoctorAvailabilityChanged', (availability: any) => {
      this.availableDoctors.update(docs => {
        const index = docs.findIndex(d => d.doctorId === availability.DoctorId);
        if (index >= 0) {
          docs[index] = { ...docs[index], ...availability };
          return [...docs];
        }
        return docs;
      });
    });

    // Video call handlers
    this.hubConnection.on('IncomingVideoCall', (callInfo: VideoCallInfo) => {
      this.incomingCall$.next(callInfo);
    });

    this.hubConnection.on('VideoCallAccepted', (data: { sessionId: string; acceptedBy: string }) => {
      this.callAccepted$.next(data);
    });

    this.hubConnection.on('VideoCallDeclined', (data: { sessionId: string; declinedBy: string }) => {
      this.callDeclined$.next(data);
    });

    this.hubConnection.on('VideoCallEnded', (sessionId: string) => {
      this.callEnded$.next(sessionId);
    });

    // WebRTC signaling
    this.hubConnection.on('ReceiveOffer', (signal: WebRTCSignal) => {
      this.offerReceived$.next(signal);
    });

    this.hubConnection.on('ReceiveAnswer', (signal: WebRTCSignal) => {
      this.answerReceived$.next(signal);
    });

    this.hubConnection.on('ReceiveIceCandidate', (signal: WebRTCSignal) => {
      this.iceCandidateReceived$.next(signal);
    });

    // User online/offline
    this.hubConnection.on('UserOnline', (userId: string, userRole: string) => {
      console.log(`User ${userId} (${userRole}) is online`);
    });

    this.hubConnection.on('UserOffline', (userId: string, userRole: string) => {
      console.log(`User ${userId} (${userRole}) is offline`);
    });
  }

  // ==================== HUB METHODS ====================

  async sendMessage(sessionId: string, content: string, messageType: string = 'Text'): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendMessage', sessionId, content, messageType);
    }
  }

  async joinSession(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinSession', sessionId);
    }
  }

  async leaveSession(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveSession', sessionId);
    }
  }

  async markMessagesAsRead(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('MarkMessagesAsRead', sessionId);
    }
  }

  async startTyping(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('StartTyping', sessionId);
    }
  }

  async stopTyping(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('StopTyping', sessionId);
    }
  }

  // Chat requests via SignalR
  async requestChat(doctorId: string, requestType: string, message?: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('RequestChat', doctorId, requestType, message);
    }
  }

  async acceptChatRequest(requestId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('AcceptChatRequest', requestId);
    }
  }

  async declineChatRequest(requestId: string, reason?: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('DeclineChatRequest', requestId, reason);
    }
  }

  async cancelChatRequest(requestId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('CancelChatRequest', requestId);
    }
  }

  // Doctor availability via SignalR
  async setChatAvailability(isAvailableForChat: boolean, isAvailableForVideo: boolean, statusMessage?: string): Promise<void> {
    const user = this.accountService.currentUser();
    if (user?.role !== 'Doctor') {
      console.warn('Only doctors can set chat availability');
      return;
    }

    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SetChatAvailability', isAvailableForChat, isAvailableForVideo, statusMessage);
      this.myChatAvailability.set({ isAvailableForChat, isAvailableForVideo, statusMessage });
    }
  }

  // Video call methods
  async initiateVideoCall(sessionId: string, targetUserId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('InitiateVideoCall', sessionId, targetUserId);
    }
  }

  async acceptVideoCall(sessionId: string, callerId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('AcceptVideoCall', sessionId, callerId);
    }
  }

  async declineVideoCall(sessionId: string, callerId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('DeclineVideoCall', sessionId, callerId);
    }
  }

  async endVideoCall(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('EndVideoCall', sessionId);
    }
  }

  // WebRTC signaling
  async sendOffer(sessionId: string, targetUserId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendOffer', sessionId, targetUserId, offer);
    }
  }

  async sendAnswer(sessionId: string, targetUserId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendAnswer', sessionId, targetUserId, answer);
    }
  }

  async sendIceCandidate(sessionId: string, targetUserId: string, candidate: RTCIceCandidateInit): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendIceCandidate', sessionId, targetUserId, candidate);
    }
  }

  // ==================== HTTP API METHODS ====================

  loadSessions(): void {
    this.http.get<ChatSession[]>(`${this.baseUrl}/sessions`).subscribe({
      next: (sessions) => this.sessions.set(sessions),
      error: (err) => console.error('Failed to load sessions:', err)
    });
  }

  getSession(sessionId: string): Observable<ChatSession> {
    return this.http.get<ChatSession>(`${this.baseUrl}/sessions/${sessionId}`);
  }

  endSession(sessionId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/sessions/${sessionId}/end`, {});
  }

  getMessages(sessionId: string, skip: number = 0, take: number = 50): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.baseUrl}/sessions/${sessionId}/messages?skip=${skip}&take=${take}`);
  }

  loadMessages(sessionId: string): void {
    this.getMessages(sessionId).subscribe({
      next: (messages) => this.messages.set(messages),
      error: (err) => console.error('Failed to load messages:', err)
    });
  }

  getUnreadCount(): Observable<{ unreadCount: number }> {
    return this.http.get<{ unreadCount: number }>(`${this.baseUrl}/unread-count`);
  }

  loadUnreadCount(): void {
    this.getUnreadCount().subscribe({
      next: (result) => this.unreadCount.set(result.unreadCount),
      error: (err) => console.error('Failed to load unread count:', err)
    });
  }

  // Chat requests via HTTP
  createChatRequestHttp(doctorId: string, requestType: string, message?: string): Observable<ChatRequest> {
    return this.http.post<ChatRequest>(`${this.baseUrl}/requests`, { doctorId, requestType, message });
  }

  getPendingRequests(): Observable<ChatRequest[]> {
    return this.http.get<ChatRequest[]>(`${this.baseUrl}/requests/pending`);
  }

  loadPendingRequests(): void {
    this.getPendingRequests().subscribe({
      next: (requests) => this.pendingRequests.set(requests),
      error: (err) => console.error('Failed to load pending requests:', err)
    });
  }

  getMyRequests(): Observable<ChatRequest[]> {
    return this.http.get<ChatRequest[]>(`${this.baseUrl}/requests/my`);
  }

  acceptChatRequestHttp(requestId: string): Observable<ChatRequestResponse> {
    return this.http.post<ChatRequestResponse>(`${this.baseUrl}/requests/${requestId}/accept`, {});
  }

  declineChatRequestHttp(requestId: string, reason?: string): Observable<ChatRequest> {
    return this.http.post<ChatRequest>(`${this.baseUrl}/requests/${requestId}/decline`, { reason });
  }

  cancelChatRequestHttp(requestId: string): Observable<ChatRequest> {
    return this.http.post<ChatRequest>(`${this.baseUrl}/requests/${requestId}/cancel`, {});
  }

  // Doctor availability via HTTP
  getAvailableDoctors(): Observable<DoctorChatAvailability[]> {
    return this.http.get<DoctorChatAvailability[]>(`${this.baseUrl}/available-doctors`);
  }

  getAllDoctors(): Observable<DoctorChatAvailability[]> {
    return this.http.get<DoctorChatAvailability[]>(`${this.baseUrl}/doctors`);
  }

  loadAvailableDoctors(): void {
    this.getAllDoctors().subscribe({
      next: (doctors) => this.availableDoctors.set(doctors),
      error: (err) => console.error('Failed to load doctors:', err)
    });
  }

  createDirectSession(doctorId: string): Observable<ChatSession> {
    return this.http.post<ChatSession>(`${this.baseUrl}/sessions/direct`, { doctorId });
  }

  getDoctorAvailability(doctorId: string): Observable<DoctorChatAvailability> {
    return this.http.get<DoctorChatAvailability>(`${this.baseUrl}/availability/${doctorId}`);
  }

  setMyAvailabilityHttp(isAvailableForChat: boolean, isAvailableForVideo: boolean, statusMessage?: string): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(`${this.baseUrl}/availability`, {
      isAvailableForChat,
      isAvailableForVideo,
      statusMessage
    });
  }
}
