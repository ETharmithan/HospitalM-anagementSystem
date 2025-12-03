import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
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
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  formatTime(timeString: string): string {
    // Assuming time is in HH:mm format
    const [hours, minutes] = timeString.split(':');
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minutes} ${ampm}`;
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'scheduled':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
      case 'completed':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      case 'cancelled':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      case 'pending':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  }

  cancelAppointment(appointmentId: string) {
    if (confirm('Are you sure you want to cancel this appointment?')) {
      this.appointmentService.deleteAppointment(appointmentId).subscribe({
        next: () => {
          this.toastService.success('Appointment cancelled successfully');
          if (this.patientId()) {
            this.loadAppointments(this.patientId()!);
          }
        },
        error: (error) => {
          this.toastService.error('Failed to cancel appointment');
          console.error(error);
        },
      });
    }
  }

  isUpcoming(appointmentDate: string, appointmentTime: string): boolean {
    const appointmentDateTime = new Date(`${appointmentDate}T${appointmentTime}`);
    return appointmentDateTime > new Date();
  }
}

