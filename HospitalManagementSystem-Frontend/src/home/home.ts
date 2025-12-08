import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AccountService } from '../core/services/account-service';
import { DoctorService } from '../core/services/doctor-service';
import { User } from '../types/user';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private accountService = inject(AccountService);
  private doctorService = inject(DoctorService);
  router = inject(Router);

  // Use computed to reactively track user state
  currentUser = computed(() => this.accountService.currentUser());
  isLoggedIn = computed(() => this.accountService.currentUser() !== null);
  
  doctorCount = signal<number>(0);
  isLoading = signal(false);

  ngOnInit(): void {
    // Redirect logged-in users to their dashboard
    const user = this.currentUser();
    if (user) {
      this.redirectToDashboard(user);
      return;
    }
    
    this.loadDoctorCount();
  }

  private redirectToDashboard(user: User): void {
    const role = user.role?.toLowerCase();
    switch (role) {
      case 'superadmin':
        this.router.navigate(['/superadmin/dashboard']);
        break;
      case 'admin':
        this.router.navigate(['/admin/dashboard']);
        break;
      case 'doctor':
        this.router.navigate(['/doctor/dashboard']);
        break;
      case 'patient':
        this.router.navigate(['/patient/dashboard']);
        break;
      default:
        // If role is unknown, stay on homepage
        break;
    }
  }

  loadDoctorCount() {
    this.isLoading.set(true);
    this.doctorService.getAllDoctors().subscribe({
      next: (doctors) => {
        this.doctorCount.set(doctors.length);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  onLogin(): void {
    this.router.navigate(['/login']);
  }

  onRegister(): void {
    this.router.navigate(['/register']);
  }

  onLogout(): void {
    this.accountService.logout();
    // currentUser and isLoggedIn are computed, they will update automatically
    this.router.navigate(['/home']);
  }

  onDashboard(): void {
    const role = this.currentUser()?.role?.toLowerCase();
    switch (role) {
      case 'superadmin':
        this.router.navigate(['/superadmin/dashboard']);
        break;
      case 'admin':
        this.router.navigate(['/admin/dashboard']);
        break;
      case 'doctor':
        this.router.navigate(['/doctor/dashboard']);
        break;
      case 'patient':
        this.router.navigate(['/patient/dashboard']);
        break;
      default:
        this.router.navigate(['/']);
    }
  }

  onBookAppointment(): void {
    this.router.navigate(['/doctors']);
  }

  onFindDoctors(): void {
    this.router.navigate(['/doctors']);
  }
}
