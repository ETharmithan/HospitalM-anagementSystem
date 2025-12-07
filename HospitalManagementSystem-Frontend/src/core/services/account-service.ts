import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { RegisterCreds, User } from '../../types/user';
import { Observable, tap } from 'rxjs';

export interface RegisterResponse {
  user: User;
  emailSent: boolean;
  requiresVerification: boolean;
  message: string;
}

export interface VerifyEmailResponse {
  message: string;
  user?: User;
}

export interface LoginResponse {
  requiresVerification?: boolean;
  email?: string;
  message?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  currentUser = signal<User | null>(null);

  baseUrl = 'http://localhost:5245/api/';

  constructor() {
    // Initialize from localStorage on startup
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        const user = JSON.parse(storedUser);
        this.currentUser.set(user);
      } catch (e) {
        localStorage.removeItem('user');
      }
    }
  }

  // REGISTER METHOD - now returns registration response with verification info
  register(creds: RegisterCreds): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(this.baseUrl + 'account/register', creds);
  }

  // LOGIN METHOD - handles both verified and unverified users
  login(creds: any): Observable<User | LoginResponse> {
    return this.http.post<User | LoginResponse>(this.baseUrl + 'account/login', creds).pipe(
      tap((response: any) => {
        // Only set user if it's a valid user object (has token)
        if (response && response.token && !response.requiresVerification) {
          this.setCurrentUser(response as User);
        }
      })
    );
  }

  // VERIFY EMAIL with OTP
  verifyEmail(email: string, otp: string): Observable<VerifyEmailResponse> {
    return this.http.post<VerifyEmailResponse>(this.baseUrl + 'account/verify-email', { email, otp });
  }

  // RESEND OTP
  resendOtp(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.baseUrl + 'account/resend-otp', { email });
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
