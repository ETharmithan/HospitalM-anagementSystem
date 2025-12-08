import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '../core/services/toast-service';
import { AccountService } from '../core/services/account-service';
import { AuthService } from '../core/services/auth-service';
import { getRoleDashboardRoute } from '../core/utils/role-utils';
import { catchError, of, finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  loginForm!: FormGroup;
  isLoading = false;
  showPassword = false;
  rememberMe = false;

  ngOnInit(): void {
    this.initializeForm();
    
    // Check if user is already logged in
    if (this.authService.isLoggedIn()) {
      const user = this.accountService.currentUser();
      if (user) {
        const dashboardRoute = getRoleDashboardRoute(user.role);
        this.router.navigate([dashboardRoute]);
      }
    }
  }

  initializeForm(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }

  get rememberMeControl() {
    return this.loginForm.get('rememberMe');
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.toastService.error('Please fill all required fields correctly');
      return;
    }

    // Prevent multiple simultaneous login attempts
    if (this.isLoading) {
      return;
    }

    this.isLoading = true;
    const formValue = this.loginForm.value;

    const loginData = {
      email: formValue.email,
      password: formValue.password
    };

    this.authService.login(loginData).pipe(
      catchError((error: any) => {
        let errorMessage = 'Login failed. Please try again.';
        
        // Handle timeout error
        if (error.name === 'TimeoutError') {
          errorMessage = 'Connection timeout. Please check your network and try again.';
        } else if (error?.status === 401) {
          errorMessage = 'Invalid email or password.';
        } else if (error?.status === 500) {
          errorMessage = 'Server error. Please try again later.';
        } else if (error?.status === 0) {
          errorMessage = 'Network error. Please check your connection.';
        } else if (error?.error?.message) {
          errorMessage = error.error.message;
        }
        
        this.toastService.error(errorMessage);
        return of(null);
      }),
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (response: any) => {
        // If response is null (from error handler), just return
        if (!response) {
          return;
        }
        
        // Check if email verification is required
        if (response.requiresVerification) {
          this.toastService.warning(response.message || 'Please verify your email before logging in.');
          this.router.navigate(['/verify-email'], { 
            queryParams: { email: response.email },
            state: { email: response.email }
          });
          return;
        }
        
        // Successful login
        if (response.user && response.token) {
           // AuthService.login already calls setUser, so we just navigate
           this.toastService.success('Login successful! Welcome back.');
           
           // Navigate to role-based landing page
           const landingRoute = getRoleDashboardRoute(response.user.role);
           this.router.navigate([landingRoute]);
        }
      }
    });
  }

  onForgotPassword(): void {
    // TODO: Implement forgot password functionality
    this.toastService.info('Forgot password feature coming soon!');
  }

  onRegister(): void {
    this.router.navigate(['/register']);
  }

  // Social login methods (placeholders for future implementation)
  onGoogleLogin(): void {
    this.toastService.info('Google login coming soon!');
  }

  onFacebookLogin(): void {
    this.toastService.info('Facebook login coming soon!');
  }

  // Helper method to check form validity for real-time validation
  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return field ? (field.invalid && (field.touched || field.dirty)) : false;
  }

  // Helper method to get field error message
  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (!field || !field.errors) return '';
    
    const errors = field.errors;
    if (errors['required']) return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} is required`;
    if (errors['email']) return 'Please enter a valid email address';
    if (errors['minlength']) return `Password must be at least ${errors['minlength'].requiredLength} characters`;
    
    return 'Invalid field';
  }
}
