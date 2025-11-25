import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Nav } from '../layout/nav/nav';
import { PatientRegister } from '../patient-register/patient-register';
import { Home } from '../home/home';
import { QuickCard } from '../layout/cards/quick-card/quick-card';
import { Sidebar } from '../layout/sidebar/sidebar';
import { Topnavbar } from '../layout/topnavbar/topnavbar';
import { User } from '../types/user';
import { Dashboard } from '../layout/patient/dashboard/dashboard';

@Component({
  selector: 'app-root',
  imports: [ Nav, PatientRegister, Home, QuickCard, Sidebar, Topnavbar, Dashboard],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private http = inject(HttpClient);
  protected readonly title = signal('HospitalManagementSystem');
  protected role = signal('Patient');
  

  protected user  = signal<User | null>(null );






}
