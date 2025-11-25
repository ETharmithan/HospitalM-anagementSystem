import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', loadComponent: () => import('../home/home').then(c => c.Home) },
  { path: 'login', loadComponent: () => import('../login/login').then(c => c.Login) },
  { path: 'register', loadComponent: () => import('../patient-register/patient-register').then(c => c.PatientRegister) },
  { path: 'patient-register', loadComponent: () => import('../patient-register/patient-register').then(c => c.PatientRegister) },
  { path: '**', redirectTo: 'home' } // Catch-all route
];
