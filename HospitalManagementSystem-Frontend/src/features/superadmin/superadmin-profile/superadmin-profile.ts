import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminDashboardService, MyProfileResponse, UpdateMyProfileRequest } from '../../../core/services/admin-dashboard-service';
import { ToastService } from '../../../core/services/toast-service';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-superadmin-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, Nav],
  templateUrl: './superadmin-profile.html',
  styleUrl: './superadmin-profile.css',
})
export class SuperAdminProfile implements OnInit {
  private adminDashboardService = inject(AdminDashboardService);
  private toastService = inject(ToastService);

  isLoading = signal(true);
  isSaving = signal(false);
  profile = signal<MyProfileResponse | null>(null);

  form: UpdateMyProfileRequest = {
    username: '',
    email: '',
    password: '',
  };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.adminDashboardService.getMyProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.form = {
          username: data.username,
          email: data.email,
          password: '',
        };
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.error(err?.message || 'Failed to load profile');
        this.isLoading.set(false);
      },
    });
  }

  save(): void {
    if (!this.form.username || !this.form.email) {
      this.toastService.error('Username and Email are required');
      return;
    }

    if (this.form.password && this.form.password.length > 0 && this.form.password.length < 8) {
      this.toastService.error('Password must be at least 8 characters');
      return;
    }

    const payload: UpdateMyProfileRequest = {
      username: this.form.username,
      email: this.form.email,
    };

    if (this.form.password && this.form.password.trim().length > 0) {
      payload.password = this.form.password;
    }

    this.isSaving.set(true);
    this.adminDashboardService.updateMyProfile(payload).subscribe({
      next: () => {
        this.toastService.success('Profile updated');
        this.form.password = '';
        this.isSaving.set(false);
        this.load();
      },
      error: (err) => {
        this.toastService.error(err?.message || 'Failed to update profile');
        this.isSaving.set(false);
      },
    });
  }
}
