import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap, Observable, map, timeout } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { LoginCreds, RegisterCreds, User } from '../../types/user';

export interface AuthResponse {
  user?: User;
  token?: string;
  message?: string;
  requiresVerification?: boolean;
  email?: string;
  emailSent?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // State
  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();
  
  // Signal for modern reactivity
  public currentUser = signal<User | null>(this.getUserFromStorage());
  public isLoggedIn = signal<boolean>(!!this.getUserFromStorage());

  private apiUrl = `${environment.apiUrl}/account`;

  constructor() {
    // Sync signal with subject for backward compatibility
    this.currentUser$.subscribe(user => {
      this.currentUser.set(user);
      this.isLoggedIn.set(!!user);
    });
  }

  // ------------------------
  // LOGIN
  // ------------------------
  login(credentials: LoginCreds): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      timeout(6000), // 6 second timeout for network errors
      map(response => {
        // Handle requiresVerification case
        if (response.requiresVerification) {
          return response as AuthResponse;
        }
        
        // Handle successful login - map backend User_Dto (capital properties) to frontend User (lowercase)
        if (response && (response.Token || response.token)) {
          const user: User = {
            id: response.Id || response.id,
            displayName: response.DisplayName || response.displayName,
            email: response.Email || response.email,
            token: response.Token || response.token,
            role: response.Role || response.role,
            imageUrl: response.ImageUrl || response.imageUrl
          };
          
          this.setUser(user);
          return { user, token: user.token } as AuthResponse;
        }
        
        return response as AuthResponse;
      })
    );
  }

  // ------------------------
  // REGISTER
  // ------------------------
  register(data: RegisterCreds): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data);
  }

  // ------------------------
  // VERIFY EMAIL
  // ------------------------
  verifyEmail(email: string, otp: string): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/verify-email`, { email, otp }).pipe(
      map(response => {
        if (response.user) {
          const backendUser = response.user;
          // Map backend User_Dto to frontend User
          if (backendUser.Token || backendUser.token) {
            const user: User = {
              id: backendUser.Id || backendUser.id,
              displayName: backendUser.DisplayName || backendUser.displayName,
              email: backendUser.Email || backendUser.email,
              token: backendUser.Token || backendUser.token,
              role: backendUser.Role || backendUser.role,
              imageUrl: backendUser.ImageUrl || backendUser.imageUrl
            };
            this.setUser(user);
            response.user = user; // Update response with mapped user
          }
        }
        return response;
      })
    );
  }

  // ------------------------
  // RESEND OTP
  // ------------------------
  resendOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resend-otp`, { email });
  }

  // ------------------------
  // PASSWORD MANAGEMENT
  // ------------------------
  changePassword(passwordData: { currentPassword: string; newPassword: string; confirmNewPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, passwordData);
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(resetData: { email: string; resetToken: string; newPassword: string; confirmNewPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, resetData);
  }

  // ------------------------
  // LOGOUT
  // ------------------------
  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('user'); // Clean up legacy key
    this.currentUserSubject.next(null);
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
    this.router.navigate(['/auth/login']);
  }

  // ------------------------
  // STATE MANAGEMENT
  // ------------------------
  
  setUser(user: User): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
    // Signal updates via subscription in constructor
  }

  getToken(): string | null {
    return this.currentUserSubject.value?.token ?? null;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private getUserFromStorage(): User | null {
    const u = localStorage.getItem('currentUser');
    return u ? JSON.parse(u) : null;
  }
}
