import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Nav } from '../layout/nav/nav';
import { PatientRegister } from '../patient-register/patient-register';

@Component({
  selector: 'app-root',
  imports: [ Nav, PatientRegister],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private http = inject(HttpClient);
  protected readonly title = signal('HospitalManagementSystem-Frontend');

}
