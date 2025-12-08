import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AppointmentService } from '../../../core/services/appointment-service';
import { DoctorService } from '../../../core/services/doctor-service';
import { PatientService } from '../../../core/services/patient-service';
import { AccountService } from '../../../core/services/account-service';
import { ToastService } from '../../../core/services/toast-service';
import { Appointment, Doctor } from '../../../types/doctor';
import { ChatNotificationBellComponent } from '../../../shared/components/chat-notification-bell.component';

interface PatientStats {
  totalAppointments: number;
  upcomingAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
}

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ChatNotificationBellComponent],
  templateUrl: './patient-dashboard.html',
  styleUrl: './patient-dashboard.css',
})
export class PatientDashboard implements OnInit {
  private appointmentService = inject(AppointmentService);
  private doctorService = inject(DoctorService);
  private patientService = inject(PatientService);
  accountService = inject(AccountService); // public for template access
  private toastService = inject(ToastService);
  private router = inject(Router);

  // Getter for user display name
  get userName(): string {
    return this.accountService.currentUser()?.displayName || 'Patient';
  }

  appointments = signal<Appointment[]>([]);
  upcomingAppointments = signal<Appointment[]>([]);
  pastAppointments = signal<Appointment[]>([]);
  doctors = signal<Doctor[]>([]);
  patientId = signal<string | null>(null);
  isLoading = signal(true);
  isLoadingDoctors = signal(false);
  activeTab = signal<'upcoming' | 'past' | 'doctors' | 'hospitals'>('upcoming');

  stats = signal<PatientStats>({
    totalAppointments: 0,
    upcomingAppointments: 0,
    completedAppointments: 0,
    cancelledAppointments: 0
  });

  ngOnInit(): void {
    this.loadPatientData();
  }

  loadPatientData(): void {
    const user = this.accountService.currentUser();
    if (user?.id) {
      this.patientService.getPatientByUserId(user.id).subscribe({
        next: (patient) => {
          if (patient.patientId) {
            this.patientId.set(patient.patientId);
            this.loadAppointments(patient.patientId);
          }
        },
        error: () => {
          this.toastService.warning('Please complete your patient registration');
          this.isLoading.set(false);
        }
      });
    } else {
      this.isLoading.set(false);
    }
  }

  loadAppointments(patientId: string): void {
    this.appointmentService.getAppointmentsByPatientId(patientId).subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.filterAppointments(appointments);
        this.calculateStats(appointments);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load appointments');
        this.isLoading.set(false);
      }
    });
  }

  filterAppointments(appointments: Appointment[]): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    this.upcomingAppointments.set(
      appointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        aptDate.setHours(0, 0, 0, 0);
        return aptDate >= today && apt.appointmentStatus !== 'Cancelled';
      }).sort((a, b) => {
        const dateCompare = new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime();
        return dateCompare !== 0 ? dateCompare : a.appointmentTime.localeCompare(b.appointmentTime);
      })
    );

    this.pastAppointments.set(
      appointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        aptDate.setHours(0, 0, 0, 0);
        return aptDate < today || apt.appointmentStatus === 'Completed' || apt.appointmentStatus === 'Cancelled';
      }).sort((a, b) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime())
    );
  }

  calculateStats(appointments: Appointment[]): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    this.stats.set({
      totalAppointments: appointments.length,
      upcomingAppointments: appointments.filter(a => {
        const aptDate = new Date(a.appointmentDate);
        aptDate.setHours(0, 0, 0, 0);
        return aptDate >= today && a.appointmentStatus !== 'Cancelled';
      }).length,
      completedAppointments: appointments.filter(a => a.appointmentStatus === 'Completed').length,
      cancelledAppointments: appointments.filter(a => a.appointmentStatus === 'Cancelled').length
    });
  }

  loadDoctors(): void {
    if (this.doctors().length > 0) return;
    
    this.isLoadingDoctors.set(true);
    this.doctorService.getAllDoctors().subscribe({
      next: (doctors) => {
        this.doctors.set(doctors);
        this.isLoadingDoctors.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load doctors');
        this.isLoadingDoctors.set(false);
      }
    });
  }

  setActiveTab(tab: 'upcoming' | 'past' | 'doctors' | 'hospitals'): void {
    this.activeTab.set(tab);
    if (tab === 'doctors') {
      this.loadDoctors();
    }
  }

  cancelAppointment(appointment: Appointment): void {
    if (confirm('Are you sure you want to cancel this appointment?')) {
      this.appointmentService.cancelAppointment(appointment.appointmentId).subscribe({
        next: () => {
          this.toastService.success('Appointment cancelled successfully');
          if (this.patientId()) {
            this.loadAppointments(this.patientId()!);
          }
        },
        error: () => {
          this.toastService.error('Failed to cancel appointment');
        }
      });
    }
  }

  bookAppointment(doctorId: string): void {
    this.router.navigate(['/book-appointment', doctorId]);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'scheduled':
      case 'pending':
        return 'status-pending';
      case 'completed':
        return 'status-completed';
      case 'cancelled':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }

  onLogout(): void {
    this.accountService.logout();
    this.router.navigate(['/login']);
  }

  navigateToBooking(): void {
    this.router.navigate(['/doctors']);
  }
}
