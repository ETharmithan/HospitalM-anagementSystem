import { HttpClient } from '@angular/common/http';
import { Component, inject, signal, computed } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { Nav } from '../layout/nav/nav';

import { PatientRegister } from '../patient-register/patient-register';
import { Home } from '../home/home';
import { QuickCard } from '../layout/cards/quick-card/quick-card';
import { Sidebar } from '../layout/sidebar/sidebar';
import { Topnavbar } from '../layout/topnavbar/topnavbar';
import { User } from '../types/user';
import { Dashboard } from '../layout/patient/dashboard/dashboard';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [ Nav, PatientRegister, Home, QuickCard, Sidebar, Topnavbar, Dashboard, RouterOutlet ],

  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private http = inject(HttpClient);
  private router = inject(Router);
  protected readonly title = signal('HospitalManagementSystem');
  protected role = signal('Patient');
  protected currentRoute = signal('');
  

  protected user  = signal<User | null>(null );

  // Pages where nav should be shown (public/home pages only)
  private navRoutes = ['/', '/home', '/doctors', '/hospitals', '/login', '/register', '/patient-register', '/about', '/services', '/contact'];
  
  showNav = computed(() => {
    const route = this.currentRoute();
    return this.navRoutes.some(r => route === r || route.startsWith('/doctor/') && !route.includes('dashboard'));
  });

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.currentRoute.set(event.urlAfterRedirects);
    });
  }
}
