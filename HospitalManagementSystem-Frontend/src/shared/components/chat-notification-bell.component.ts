import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ChatService } from '../../core/services/chat-service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat-notification-bell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notification-bell" (click)="goToChat()" title="Messages">
      <svg 
        xmlns="http://www.w3.org/2000/svg" 
        width="24" 
        height="24" 
        viewBox="0 0 24 24" 
        fill="none" 
        stroke="currentColor" 
        stroke-width="2" 
        stroke-linecap="round" 
        stroke-linejoin="round">
        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
        <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
      </svg>
      <span *ngIf="unreadCount() > 0" class="badge">{{ unreadCount() }}</span>
    </div>
  `,
  styles: [`
    .notification-bell {
      position: relative;
      cursor: pointer;
      padding: 8px;
      border-radius: 8px;
      transition: all 0.2s;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }

    .notification-bell:hover {
      background: rgba(16, 185, 129, 0.1);
    }

    .notification-bell svg {
      color: #6b7280;
      transition: color 0.2s;
    }

    .notification-bell:hover svg {
      color: #10b981;
    }

    .badge {
      position: absolute;
      top: 4px;
      right: 4px;
      background: #ef4444;
      color: white;
      border-radius: 10px;
      padding: 2px 6px;
      font-size: 11px;
      font-weight: 600;
      min-width: 18px;
      text-align: center;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0%, 100% {
        transform: scale(1);
      }
      50% {
        transform: scale(1.1);
      }
    }
  `]
})
export class ChatNotificationBellComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);
  private router = inject(Router);
  
  unreadCount = signal<number>(0);
  private subscription?: Subscription;

  async ngOnInit(): Promise<void> {
    // Connect to SignalR if not already connected
    if (!this.chatService.isConnected()) {
      await this.chatService.startConnection();
    }
    
    // Load initial unread count
    this.chatService.loadUnreadCount();
    
    // Subscribe to unread count changes
    this.unreadCount.set(this.chatService.unreadCount());
    
    // Listen for new messages to update count
    this.subscription = this.chatService.onMessageReceived.subscribe(() => {
      console.log('[Bell] New message received, updating count');
      this.chatService.loadUnreadCount();
      setTimeout(() => {
        this.unreadCount.set(this.chatService.unreadCount());
        console.log('[Bell] Updated count:', this.unreadCount());
      }, 500);
    });
    
    // Poll for updates every 30 seconds as backup
    setInterval(() => {
      this.chatService.loadUnreadCount();
      this.unreadCount.set(this.chatService.unreadCount());
    }, 30000);
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  goToChat(): void {
    this.router.navigate(['/chat']);
  }
}
