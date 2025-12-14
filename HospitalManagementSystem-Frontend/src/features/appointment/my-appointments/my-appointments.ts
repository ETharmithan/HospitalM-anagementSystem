import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AppointmentService } from '../../../core/services/appointment-service';
import { DoctorService } from '../../../core/services/doctor-service';
import { PatientService } from '../../../core/services/patient-service';
import { ToastService } from '../../../core/services/toast-service';
import { AccountService } from '../../../core/services/account-service';
import { Appointment, Doctor } from '../../../types/doctor';

@Component({
  selector: 'app-my-appointments',
  imports: [CommonModule, RouterModule],
  templateUrl: './my-appointments.html',
  styleUrl: './my-appointments.css',
})
export class MyAppointments implements OnInit {
  router = inject(Router);
  private appointmentService = inject(AppointmentService);
  private doctorService = inject(DoctorService);
  private patientService = inject(PatientService);
  private toastService = inject(ToastService);
  private accountService = inject(AccountService);

  appointments = signal<Appointment[]>([]);
  isLoading = signal<boolean>(false);
  patientId = signal<string | null>(null);
  doctorsMap = signal<Map<string, Doctor>>(new Map());

  ngOnInit() {
    this.loadPatientId();
  }

  loadPatientId() {
    const currentUser = this.accountService.currentUser();
    if (currentUser?.id) {
      this.patientService.getPatientByUserId(currentUser.id).subscribe({
        next: (patient) => {
          if (patient.patientId) {
            this.patientId.set(patient.patientId);
            this.loadAppointments(patient.patientId);
          }
        },
        error: () => {
          this.toastService.warning('Please complete your patient registration first');
        },
      });
    } else {
      this.toastService.error('Please login to view your appointments');
    }
  }

  loadAppointments(patientId: string) {
    this.isLoading.set(true);
    this.appointmentService.getAppointmentsByPatientId(patientId).subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.loadDoctorsForAppointments(appointments);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.toastService.error('Failed to load appointments');
        this.isLoading.set(false);
        console.error(error);
      },
    });
  }

  loadDoctorsForAppointments(appointments: Appointment[]) {
    const doctorIds = [...new Set(appointments.map((apt) => apt.doctorId))];
    const doctorsMap = new Map<string, Doctor>();

    doctorIds.forEach((doctorId) => {
      this.doctorService.getDoctorById(doctorId).subscribe({
        next: (doctor) => {
          doctorsMap.set(doctorId, doctor);
          this.doctorsMap.set(new Map(doctorsMap));
        },
        error: (error) => {
          console.error('Failed to load doctor:', error);
        },
      });
    });
  }

  getDoctor(doctorId: string): Doctor | undefined {
    return this.doctorsMap().get(doctorId);
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'Invalid date';
    
    let date: Date;
    
    // Try to parse various date formats
    if (dateString.includes('T')) {
      // ISO format: 2024-01-15T00:00:00
      date = new Date(dateString);
    } else if (dateString.includes('-')) {
      // Date format: 2024-01-15
      const parts = dateString.split('-');
      if (parts.length === 3) {
        const [year, month, day] = parts.map(Number);
        date = new Date(year, month - 1, day);
      } else {
        date = new Date(dateString);
      }
    } else if (dateString.includes('/')) {
      // Date format: 01/15/2024
      date = new Date(dateString);
    } else {
      // Try as-is
      date = new Date(dateString);
    }
    
    // Check if date is valid
    if (isNaN(date.getTime())) {
      return 'Invalid date';
    }
    
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  formatTime(timeString: string): string {
    if (!timeString) return 'Invalid time';
    
    try {
      // Handle various time formats
      let hours: number;
      let minutes: string;
      
      if (timeString.includes(':')) {
        const [h, m] = timeString.split(':');
        hours = parseInt(h);
        minutes = m.padStart(2, '0');
      } else if (timeString.includes('.')) {
        const [h, m] = timeString.split('.');
        hours = parseInt(h);
        minutes = m.padStart(2, '0');
      } else {
        // Try to parse as number (e.g., 14.5 = 14:30)
        const timeFloat = parseFloat(timeString);
        hours = Math.floor(timeFloat);
        minutes = Math.round((timeFloat - hours) * 60).toString().padStart(2, '0');
      }
      
      if (isNaN(hours) || isNaN(parseInt(minutes))) {
        return 'Invalid time';
      }
      
      const ampm = hours >= 12 ? 'PM' : 'AM';
      const displayHour = hours % 12 || 12;
      return `${displayHour}:${minutes} ${ampm}`;
    } catch (error) {
      return 'Invalid time';
    }
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'scheduled':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
      case 'completed':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      case 'cancelled':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      case 'cancellationrequested':
        return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200';
      case 'pending':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  }

  cancelAppointment(appointmentId: string, appointmentDate: string, appointmentTime: string) {
    // Check if appointment is within 60 minutes
    const appointmentDateTime = new Date(`${appointmentDate}T${appointmentTime}`);
    const timeUntilAppointment = appointmentDateTime.getTime() - new Date().getTime();
    const minutesUntilAppointment = timeUntilAppointment / (1000 * 60);
    
    if (minutesUntilAppointment < 60) {
      this.toastService.error('Cancellations must be requested at least 60 minutes before the appointment time.');
      return;
    }

    const reason = prompt('Please provide a reason for cancellation:');
    if (reason === null) return; // User cancelled

    this.appointmentService.requestCancellation(appointmentId, reason).subscribe({
      next: () => {
        this.toastService.success('Cancellation request submitted successfully. Your doctor will review and approve the request.');
        if (this.patientId()) {
          this.loadAppointments(this.patientId()!);
        }
      },
      error: (error) => {
        this.toastService.error(error.error?.message || 'Failed to submit cancellation request');
        console.error(error);
      },
    });
  }

  isUpcoming(appointmentDate: string, appointmentTime: string): boolean {
    const appointmentDateTime = new Date(`${appointmentDate}T${appointmentTime}`);
    return appointmentDateTime > new Date();
  }
}

