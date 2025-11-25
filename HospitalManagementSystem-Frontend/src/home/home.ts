import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  currentUser: any = null;
  isLoggedIn = false;

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.isLoggedIn = this.authService.isLoggedInValue;
  }

  onLogin(): void {
    this.router.navigate(['/login']);
  }

  onRegister(): void {
    this.router.navigate(['/register']);
  }

  onLogout(): void {
    this.authService.logout();
  }

  onDashboard(): void {
    // TODO: Navigate to dashboard based on user role
    if (this.currentUser?.role === 'Patient') {
      this.router.navigate(['/patient-dashboard']);
    } else {
      this.router.navigate(['/dashboard']);
    }
  }
}
