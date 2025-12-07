import { Routes } from '@angular/router';
import { authGuard, superAdminGuard, adminGuard, doctorGuard, patientGuard } from '../core/guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', loadComponent: () => import('../home/home').then(c => c.Home) },
  { path: 'login', loadComponent: () => import('../login/login').then(c => c.Login) },
  { path: 'register', loadComponent: () => import('../patient-register/patient-register').then(c => c.PatientRegister) },
  { path: 'patient-register', loadComponent: () => import('../patient-register/patient-register').then(c => c.PatientRegister) },
  { path: 'verify-email', loadComponent: () => import('../features/auth/verify-email/verify-email').then(c => c.VerifyEmail) },
  
  // SuperAdmin routes
  { 
    path: 'superadmin/dashboard', 
    loadComponent: () => import('../features/superadmin/superadmin-dashboard').then(c => c.SuperAdminDashboard),
    canActivate: [superAdminGuard]
  },
  
  // Admin (Hospital Owner) routes
  { 
    path: 'admin/dashboard', 
    loadComponent: () => import('../features/admin/admin-dashboard').then(c => c.AdminDashboard),
    canActivate: [adminGuard]
  },
  
  // Doctor routes
  { 
    path: 'doctor/dashboard', 
    loadComponent: () => import('../features/doctor/doctor-dashboard/doctor-dashboard').then(c => c.DoctorDashboard),
    canActivate: [doctorGuard]
  },
  
  // Patient routes
  { 
    path: 'patient/dashboard', 
    loadComponent: () => import('../features/patient/patient-dashboard/patient-dashboard').then(c => c.PatientDashboard),
    canActivate: [patientGuard]
  },
  { 
    path: 'my-appointments', 
    loadComponent: () => import('../features/appointment/my-appointments/my-appointments').then(c => c.MyAppointments),
    canActivate: [authGuard]
  },
  
  // Public doctor listing and booking
  { path: 'doctors', loadComponent: () => import('../features/doctor/doctor-list/doctor-list').then(c => c.DoctorList) },
  { path: 'doctor/:doctorId', loadComponent: () => import('../features/doctor/doctor-list/doctor-list').then(c => c.DoctorList) },
  { 
    path: 'book-appointment/:doctorId', 
    loadComponent: () => import('../features/appointment/book-appointment/book-appointment').then(c => c.BookAppointment),
    canActivate: [authGuard]
  },
  
  // Hospitals listing (public)
  { path: 'hospitals', loadComponent: () => import('../features/hospital/hospital-list/hospital-list').then(c => c.HospitalList) },
  
  { path: '**', redirectTo: 'home' } // Catch-all route
];
