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
  { 
    path: 'superadmin/hospital-details/:id', 
    loadComponent: () => import('../features/superadmin/hospital-details/hospital-details').then(c => c.HospitalDetails),
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
    path: 'patient-profile', 
    loadComponent: () => import('../features/patient/patient-profile/patient-profile').then(c => c.PatientProfile),
    canActivate: [authGuard]
  },
  
  { 
    path: 'my-appointments', 
    loadComponent: () => import('../features/appointment/my-appointments/my-appointments').then(c => c.MyAppointments),
    canActivate: [authGuard]
  },
  
  // Public doctor listing and booking
  { path: 'doctors', loadComponent: () => import('../features/doctor/doctor-list/doctor-list').then(c => c.DoctorList) },
  { path: 'doctor/:doctorId', loadComponent: () => import('../features/doctor/doctor-list/doctor-list').then(c => c.DoctorList) },
  { path: 'doctor-detail/:id', loadComponent: () => import('../features/doctor/doctor-detail/doctor-detail').then(c => c.DoctorDetail) },
  { 
    path: 'book-appointment/:doctorId', 
    loadComponent: () => import('../features/appointment/book-appointment/book-appointment').then(c => c.BookAppointment),
    canActivate: [authGuard]
  },
  
  // Hospitals listing (public)
  { path: 'hospitals', loadComponent: () => import('../features/hospital/hospital-list/hospital-list').then(c => c.HospitalList) },
  { path: 'hospital-detail/:id', loadComponent: () => import('../features/hospital/hospital-detail/hospital-detail').then(c => c.HospitalDetail) },
  
  // Chat
  { 
    path: 'chat', 
    loadComponent: () => import('../features/chat/chat').then(c => c.ChatComponent),
    canActivate: [authGuard]
  },

  {
    path: 'e-prescription/create',
    loadComponent: () => import('../features/E-Prescription/create-e-prescription/create-e-prescription').then(c => c.CreateEPrescription),
    canActivate: [doctorGuard]
  },

  {
    path: 'e-prescription',
    loadComponent: () => import('../features/E-Prescription/show-e-prescription/show-e-prescription').then(c => c.ShowEPrescription),
    canActivate: [authGuard]
  },

  // Terms and Contact pages
  { path: 'terms', loadComponent: () => import('../features/shared/terms-conditions/terms-conditions').then(c => c.TermsConditions) },
  { path: 'contact', loadComponent: () => import('../features/shared/contact/contact').then(c => c.Contact) },

  { path: '**', redirectTo: 'home' } // Catch-all route
];
