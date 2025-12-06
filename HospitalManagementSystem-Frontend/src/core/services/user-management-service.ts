import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface UserInfo {
  userId: string;
  email: string;
  username: string;
  role: string;
  imageUrl?: string;
  isEmailVerified: boolean;
}

export interface CreateUserRequest {
  email: string;
  displayName: string;
  password: string;
  role: string;
  imageUrl?: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api';

  // Get all users (for SuperAdmin)
  getAllUsers(): Observable<UserInfo[]> {
    return this.http.get<UserInfo[]>(`${this.baseUrl}/users`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching users:', error);
        return throwError(() => new Error('Failed to fetch users.'));
      })
    );
  }

  // Get users by role
  getUsersByRole(role: string): Observable<UserInfo[]> {
    return this.http.get<UserInfo[]>(`${this.baseUrl}/users/role/${role}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching users by role:', error);
        return throwError(() => new Error('Failed to fetch users.'));
      })
    );
  }

  // Create a new user (Admin, Doctor, etc.)
  createUser(user: CreateUserRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/account/register`, user).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating user:', error);
        const message = error.error?.message || error.error || 'Failed to create user.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Delete user
  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${userId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting user:', error);
        return throwError(() => new Error('Failed to delete user.'));
      })
    );
  }

  // Update user role
  updateUserRole(userId: string, role: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/users/${userId}/role`, { role }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating user role:', error);
        return throwError(() => new Error('Failed to update user role.'));
      })
    );
  }
}
