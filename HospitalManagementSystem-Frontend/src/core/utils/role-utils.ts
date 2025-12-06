export function getRoleDashboardRoute(role: string | undefined | null): string {
  switch (role?.toLowerCase()) {
    case 'superadmin':
      return '/superadmin/dashboard';
    case 'admin':
      return '/admin/dashboard';
    case 'doctor':
      return '/doctor/dashboard';
    case 'patient':
      return '/patient/dashboard';
    default:
      return '/home';
  }
}

export function hasRole(userRole: string | undefined | null, allowedRoles: string[]): boolean {
  if (!userRole) return false;
  return allowedRoles.map(r => r.toLowerCase()).includes(userRole.toLowerCase());
}

export const ROLES = {
  SUPER_ADMIN: 'SuperAdmin',
  ADMIN: 'Admin',
  DOCTOR: 'Doctor',
  PATIENT: 'Patient'
} as const;
