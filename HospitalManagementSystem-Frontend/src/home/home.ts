import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
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

  currentUser = signal<User | null>(null);
  isLoggedIn = signal(false);
  doctorCount = signal<number>(0);
  isLoading = signal(false);

  ngOnInit(): void {
    const user = this.accountService.currentUser();
    this.currentUser.set(user);
    this.isLoggedIn.set(user !== null);
    this.loadDoctorCount();
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
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
    this.router.navigate(['/home']);
  }

  onDashboard(): void {
    const role = this.currentUser()?.role?.toLowerCase();
    if (role === 'superadmin') {
      this.router.navigate(['/superadmin/dashboard']);
      return;
    }
    if (role === 'admin') {
      this.router.navigate(['/admin/dashboard']);
      return;
    }

    if (role === 'patient') {
      this.router.navigate(['/my-appointments']);
    } else {
      this.router.navigate(['/doctors']);
    }
  }

  onBookAppointment(): void {
    this.router.navigate(['/doctors']);
  }

  onFindDoctors(): void {
    this.router.navigate(['/doctors']);
  }
}
