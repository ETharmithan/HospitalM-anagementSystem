import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../services/account-service';
import { getRoleDashboardRoute } from '../utils/role-utils';

/**
 * Guest Guard - Redirects authenticated users to their dashboard
 * Use this for pages that should only be accessible to non-authenticated users (e.g., home, login, register)
 */
export const guestGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  const user = accountService.currentUser();
  
  // If user is logged in, redirect to their role-based dashboard
  if (user) {
    const dashboardRoute = getRoleDashboardRoute(user.role);
    router.navigate([dashboardRoute]);
    return false;
  }
  
  // Allow access for guests
  return true;
};
