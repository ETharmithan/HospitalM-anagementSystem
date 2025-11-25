import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '../core/services/toast-service';
import { AccountService } from '../core/services/account-service';
import { AuthService } from '../core/services/auth.service';

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

  loginForm!: FormGroup;
  isLoading = false;
  showPassword = false;
  rememberMe = false;

  ngOnInit(): void {
    this.initializeForm();
    
    // Check if user is already logged in
    if (this.authService.isLoggedInValue) {
      this.router.navigate(['/']);
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

    this.isLoading = true;
    const formValue = this.loginForm.value;

    const loginData = {
      email: formValue.email,
      password: formValue.password
    };

    this.accountService.login(loginData).subscribe({
      next: (userResponse: any) => {
        this.isLoading = false;
        
        // Convert to AuthService User format
        const user = {
          userId: userResponse.id || userResponse.userId,
          patientId: userResponse.patientId || '',
          email: userResponse.email,
          name: userResponse.displayName || userResponse.name || 'User',
          role: userResponse.role || 'User',
          imageUrl: userResponse.imageUrl || ''
        };

        // Login using AuthService
        this.authService.login(user);
        
        // Show success message
        this.toastService.success('Login successful! Welcome back.');
        
        // Navigate to home page
        this.router.navigate(['/']);
      },
      error: (error: any) => {
        this.isLoading = false;
        let errorMessage = 'Login failed. Please try again.';
        
        // Handle specific error cases
        if (error?.status === 401) {
          errorMessage = 'Invalid email or password. Please check your credentials and try again.';
        } else if (error?.status === 400) {
          if (error?.error?.message) {
            errorMessage = error.error.message;
            // Make specific errors more user-friendly
            if (errorMessage.toLowerCase().includes('invalid email')) {
              errorMessage = 'Invalid email address. Please enter a valid email.';
            } else if (errorMessage.toLowerCase().includes('invalid password')) {
              errorMessage = 'Invalid password. Please check your password and try again.';
            } else if (errorMessage.toLowerCase().includes('email not found')) {
              errorMessage = 'Email not found. Please check your email or register for a new account.';
            }
          } else if (error?.error?.title) {
            errorMessage = error.error.title;
          } else if (error?.error?.errors) {
            // Handle validation errors
            const errorMessages = Object.values(error.error.errors).flat();
            errorMessage = errorMessages.join(', ') || 'Invalid login credentials';
          }
        } else if (error?.status === 500) {
          errorMessage = 'Server error. Please try again later.';
        } else if (error?.status === 0) {
          errorMessage = 'Network error. Please check your internet connection.';
        } else if (error?.error?.message) {
          errorMessage = error.error.message;
        }
        
        this.toastService.error(errorMessage);
        console.error('Login error:', error);
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
