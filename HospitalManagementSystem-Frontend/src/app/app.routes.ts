import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', loadComponent: () => import('../home/home').then(c => c.Home) },
  { path: 'login', loadComponent: () => import('../login/login').then(c => c.Login) },
  { path: 'register', loadComponent: () => import('../patient-register/patient-register').then(c => c.PatientRegister) },
  { path: 'patient-register', loadComponent: () => import('../patient-register/patient-register').then(c => c.PatientRegister) },
  // Doctor and Appointment routes
  { path: 'doctors', loadComponent: () => import('../features/doctor/doctor-list/doctor-list').then(c => c.DoctorList) },
  { path: 'doctor/:doctorId', loadComponent: () => import('../features/doctor/doctor-list/doctor-list').then(c => c.DoctorList) },
  { path: 'book-appointment/:doctorId', loadComponent: () => import('../features/appointment/book-appointment/book-appointment').then(c => c.BookAppointment) },
  { path: 'my-appointments', loadComponent: () => import('../features/appointment/my-appointments/my-appointments').then(c => c.MyAppointments) },
  { path: '**', redirectTo: 'home' } // Catch-all route
];
