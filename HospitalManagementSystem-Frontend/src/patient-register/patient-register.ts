import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-patient-register',
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './patient-register.html',
  styleUrl: './patient-register.css',
})
export class PatientRegister implements OnInit {
  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

}
