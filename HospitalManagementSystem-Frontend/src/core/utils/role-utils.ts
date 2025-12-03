export function getRoleDashboardRoute(role: string | undefined | null): string {
  switch (role?.toLowerCase()) {
    case 'superadmin':
      return '/superadmin/dashboard';
    case 'admin':
      return '/admin/dashboard';
    default:
      return '/home';
  }
}
