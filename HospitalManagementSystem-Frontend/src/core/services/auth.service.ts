import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';

export interface User {
  userId: string;
  patientId: string;
  email: string;
  name: string;
  role: string;
  imageUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private isLoggedInSignal = signal<boolean>(false);
  public isLoggedIn = this.isLoggedInSignal.asReadonly();

  constructor(private router: Router) {
    // Check for existing user session on init
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      try {
        const user = JSON.parse(storedUser);
        this.currentUserSubject.next(user);
        this.isLoggedInSignal.set(true);
      } catch (e) {
        localStorage.removeItem('currentUser');
      }
    }
  }

  login(user: User): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
    this.isLoggedInSignal.set(true);
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.isLoggedInSignal.set(false);
    this.router.navigate(['/']);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get isLoggedInValue(): boolean {
    return this.isLoggedInSignal();
  }

  // For patient registration auto-login
  autoLoginAfterRegistration(userData: any): void {
    const user: User = {
      userId: userData.userId,
      patientId: userData.patientId,
      email: userData.emailAddress,
      name: `${userData.firstName} ${userData.lastName}`,
      role: 'Patient',
      imageUrl: userData.imageUrl
    };
    this.login(user);
  }
}
