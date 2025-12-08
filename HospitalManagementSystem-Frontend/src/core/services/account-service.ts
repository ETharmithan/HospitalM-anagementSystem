import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private authService = inject(AuthService);

  // Legacy support: Delegate to AuthService
  get currentUser() {
    return this.authService.currentUser;
  }

  logout() {
    this.authService.logout();
  }

  // Methods that might still be needed if other components use them directly
  // But based on analysis, login/register/verifyEmail are moved.
  
  // If there are other methods in AccountService (e.g. updateProfile), keep them here.
  // Currently there were no other methods in the file I read previously.
}
