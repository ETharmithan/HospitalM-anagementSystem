import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { AuthService } from '../../../core/services/auth-service';
import { ToastService } from '../../../core/services/toast-service';
import { getRoleDashboardRoute } from '../../../core/utils/role-utils';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.css',
})
export class VerifyEmail implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private accountService = inject(AccountService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  otpForm!: FormGroup;
  email = signal<string>('');
  isLoading = signal<boolean>(false);
  isResending = signal<boolean>(false);
  countdown = signal<number>(0);
  private countdownInterval: any;

  ngOnInit() {
    // Get email from query params or route state
    const emailParam = this.route.snapshot.queryParamMap.get('email');
    if (emailParam) {
      this.email.set(emailParam);
    } else {
      // Try to get from navigation state
      const state = history.state;
      if (state?.email) {
        this.email.set(state.email);
      }
    }

    if (!this.email()) {
      this.toastService.error('Email not found. Please register again.');
      this.router.navigate(['/register']);
      return;
    }

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    });

    // Start countdown for resend button
    this.startCountdown();
  }

  startCountdown() {
    this.countdown.set(60);
    this.countdownInterval = setInterval(() => {
      const current = this.countdown();
      if (current > 0) {
        this.countdown.set(current - 1);
      } else {
        clearInterval(this.countdownInterval);
      }
    }, 1000);
  }

  onSubmit() {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const otp = this.otpForm.value.otp;

    this.authService.verifyEmail(this.email(), otp).subscribe({
      next: (response: any) => {
        this.isLoading.set(false);
        this.toastService.success('Email verified successfully! Logging you in...');
        
        // If user data is returned, log them in and navigate to dashboard
        if (response.user && response.user.token) {
          // AuthService.verifyEmail already calls setUser, so we just navigate
          
          // Navigate to role-based dashboard
          const landingRoute = getRoleDashboardRoute(response.user.role);
          this.router.navigate([landingRoute]);
        } else {
          // No token returned, redirect to login
          this.toastService.info('Please login with your credentials.');
          this.router.navigate(['/login']);
        }
      },
      error: (error: any) => {
        this.isLoading.set(false);
        this.toastService.error(error.error?.message || error.error || 'Verification failed. Please try again.');
      },
    });
  }

  resendOtp() {
    if (this.countdown() > 0) return;

    this.isResending.set(true);
    this.authService.resendOtp(this.email()).subscribe({
      next: (response: any) => {
        this.isResending.set(false);
        this.toastService.success(response.message || 'OTP sent successfully!');
        this.startCountdown();
        this.otpForm.reset();
      },
      error: (error) => {
        this.isResending.set(false);
        this.toastService.error(error.error?.message || error.error || 'Failed to resend OTP.');
      },
    });
  }

  get otp() {
    return this.otpForm.get('otp');
  }

  ngOnDestroy() {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }
}
