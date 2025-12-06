import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../services/account-service';
import { ToastService } from '../services/toast-service';
import { hasRole } from '../utils/role-utils';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastService = inject(ToastService);
  const router = inject(Router);

  const user = accountService.currentUser();
  if (!user) {
    toastService.error('Please login to access this page');
    router.navigate(['/login']);
    return false;
  }
  return true;
};

// Role-based guard factory
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const accountService = inject(AccountService);
    const toastService = inject(ToastService);
    const router = inject(Router);

    const user = accountService.currentUser();
    if (!user) {
      toastService.error('Please login to access this page');
      router.navigate(['/login']);
      return false;
    }

    if (!hasRole(user.role, allowedRoles)) {
      toastService.error('You do not have permission to access this page');
      router.navigate(['/home']);
      return false;
    }

    return true;
  };
};

// Pre-defined role guards
export const superAdminGuard: CanActivateFn = roleGuard(['SuperAdmin']);
export const adminGuard: CanActivateFn = roleGuard(['Admin', 'SuperAdmin']);
export const doctorGuard: CanActivateFn = roleGuard(['Doctor']);
export const patientGuard: CanActivateFn = roleGuard(['Patient']);
export const staffGuard: CanActivateFn = roleGuard(['SuperAdmin', 'Admin', 'Doctor']);
