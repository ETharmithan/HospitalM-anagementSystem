import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, tap, timeout } from 'rxjs/operators';

export interface Notification {
  notificationId: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  actionUrl?: string;
  timeAgo: string;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api/notification';

  // Reactive state
  notifications = signal<Notification[]>([]);
  unreadCount = signal<number>(0);

  // Get all notifications
  getNotifications(userType: string = 'Patient'): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.baseUrl}?userType=${userType}`).pipe(
      timeout(10000),
      tap(notifications => this.notifications.set(notifications)),
      catchError(error => {
        console.error('Error fetching notifications:', error);
        return throwError(() => new Error('Failed to fetch notifications.'));
      })
    );
  }

  // Get unread notifications
  getUnreadNotifications(userType: string = 'Patient'): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.baseUrl}/unread?userType=${userType}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching unread notifications:', error);
        return throwError(() => new Error('Failed to fetch unread notifications.'));
      })
    );
  }

  // Get unread count
  getUnreadCount(userType: string = 'Patient'): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/unread-count?userType=${userType}`).pipe(
      timeout(10000),
      tap(result => this.unreadCount.set(result.count)),
      catchError(error => {
        console.error('Error fetching unread count:', error);
        return throwError(() => new Error('Failed to fetch unread count.'));
      })
    );
  }

  // Mark notification as read
  markAsRead(notificationId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/${notificationId}/read`, {}).pipe(
      timeout(10000),
      tap(() => {
        // Update local state
        const current = this.notifications();
        const updated = current.map(n => 
          n.notificationId === notificationId ? { ...n, isRead: true } : n
        );
        this.notifications.set(updated);
        this.unreadCount.update(count => Math.max(0, count - 1));
      }),
      catchError(error => {
        console.error('Error marking notification as read:', error);
        return throwError(() => new Error('Failed to mark notification as read.'));
      })
    );
  }

  // Mark all as read
  markAllAsRead(userType: string = 'Patient'): Observable<any> {
    return this.http.put(`${this.baseUrl}/read-all?userType=${userType}`, {}).pipe(
      timeout(10000),
      tap(() => {
        // Update local state
        const current = this.notifications();
        const updated = current.map(n => ({ ...n, isRead: true }));
        this.notifications.set(updated);
        this.unreadCount.set(0);
      }),
      catchError(error => {
        console.error('Error marking all as read:', error);
        return throwError(() => new Error('Failed to mark all notifications as read.'));
      })
    );
  }

  // Delete notification
  deleteNotification(notificationId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${notificationId}`).pipe(
      timeout(10000),
      tap(() => {
        // Update local state
        const current = this.notifications();
        const notification = current.find(n => n.notificationId === notificationId);
        const updated = current.filter(n => n.notificationId !== notificationId);
        this.notifications.set(updated);
        if (notification && !notification.isRead) {
          this.unreadCount.update(count => Math.max(0, count - 1));
        }
      }),
      catchError(error => {
        console.error('Error deleting notification:', error);
        return throwError(() => new Error('Failed to delete notification.'));
      })
    );
  }

  // Delete all notifications
  deleteAllNotifications(userType: string = 'Patient'): Observable<any> {
    return this.http.delete(`${this.baseUrl}/all?userType=${userType}`).pipe(
      timeout(10000),
      tap(() => {
        this.notifications.set([]);
        this.unreadCount.set(0);
      }),
      catchError(error => {
        console.error('Error deleting all notifications:', error);
        return throwError(() => new Error('Failed to delete all notifications.'));
      })
    );
  }

  // Get notification icon based on type
  getNotificationIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'booking': return '📅';
      case 'prescription': return '💊';
      case 'payment': return '💳';
      case 'success': return '✅';
      case 'warning': return '⚠️';
      case 'error': return '❌';
      default: return '🔔';
    }
  }

  // Get notification color class based on type
  getNotificationClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'booking': return 'notification-booking';
      case 'prescription': return 'notification-prescription';
      case 'payment': return 'notification-payment';
      case 'success': return 'notification-success';
      case 'warning': return 'notification-warning';
      case 'error': return 'notification-error';
      default: return 'notification-info';
    }
  }
}
