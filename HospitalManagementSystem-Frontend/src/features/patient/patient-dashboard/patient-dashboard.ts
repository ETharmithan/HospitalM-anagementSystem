import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AppointmentService } from '../../../core/services/appointment-service';
import { DoctorService } from '../../../core/services/doctor-service';
import { PatientService } from '../../../core/services/patient-service';
import { AccountService } from '../../../core/services/account-service';
import { PrescriptionService, PrescriptionResponse } from '../../../core/services/prescription-service';
import { ToastService } from '../../../core/services/toast-service';
import { Appointment, Doctor } from '../../../types/doctor';
import { ChatNotificationBellComponent } from '../../../shared/components/chat-notification-bell.component';
import { Nav } from '../../../layout/nav/nav';
import { HospitalService, Hospital } from '../../../core/services/hospital-service';

interface PatientStats {
  totalAppointments: number;
  upcomingAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
}

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ChatNotificationBellComponent, Nav],
  templateUrl: './patient-dashboard.html',
  styleUrl: './patient-dashboard.css',
})
export class PatientDashboard implements OnInit {
  private appointmentService = inject(AppointmentService);
  private doctorService = inject(DoctorService);
  private patientService = inject(PatientService);
  private prescriptionService = inject(PrescriptionService);
  private hospitalService = inject(HospitalService);
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
  hospitals = signal<Hospital[]>([]);
  doctorNameMap = signal<Map<string, string>>(new Map());
  patientId = signal<string | null>(null);
  isLoading = signal(true);
  isLoadingDoctors = signal(false);
  isLoadingHospitals = signal(false);
  activeTab = signal<'upcoming' | 'past' | 'doctors' | 'hospitals'>('upcoming');

  stats = signal<PatientStats>({
    totalAppointments: 0,
    upcomingAppointments: 0,
    completedAppointments: 0,
    cancelledAppointments: 0
  });

  prescriptions = signal<PrescriptionResponse[]>([]);
  isLoadingPrescriptions = signal(false);
  latestPrescription = computed<PrescriptionResponse | null>(() => {
    const list = this.prescriptions();
    if (!list || list.length === 0) return null;

    // Prefer visitDate; fallback to createdAt
    const sorted = [...list].sort((a, b) => {
      const aDate = new Date(a.visitDate).getTime();
      const bDate = new Date(b.visitDate).getTime();
      return bDate - aDate;
    });

    return sorted[0] ?? null;
  });

  ngOnInit(): void {
    this.loadPatientData();
    this.loadMyPrescriptions();
  }

  loadMyPrescriptions(): void {
    this.isLoadingPrescriptions.set(true);
    const user = this.accountService.currentUser();
    if (!user?.id) {
      this.isLoadingPrescriptions.set(false);
      return;
    }

    // user.id is the UserId, not the PatientId. Resolve PatientId first.
    this.patientService.getPatientByUserId(user.id).subscribe({
      next: (patient) => {
        if (!patient?.patientId) {
          this.prescriptions.set([]);
          this.isLoadingPrescriptions.set(false);
          return;
        }

        this.prescriptionService.getPrescriptionsByPatientId(patient.patientId).subscribe({
          next: (items) => {
            this.prescriptions.set(items ?? []);
            this.isLoadingPrescriptions.set(false);
          },
          error: () => {
            this.isLoadingPrescriptions.set(false);
          },
        });
      },
      error: () => {
        this.isLoadingPrescriptions.set(false);
      },
    });
  }

  openPrescriptionHistory(): void {
    this.router.navigate(['/e-prescription']);
  }

  downloadPrescription(p: PrescriptionResponse): void {
    // Download functionality removed - prescriptions are view-only
    this.toastService.info('View prescription details in E-Prescription page');
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
          // this.toastService.warning('Please complete your patient registration');
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
        this.loadDoctorNamesForAppointments(appointments);
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

  loadHospitals(): void {
    if (this.hospitals().length > 0) return;
    
    this.isLoadingHospitals.set(true);
    this.hospitalService.getPublicHospitals().subscribe({
      next: (hospitals) => {
        this.hospitals.set(hospitals);
        this.isLoadingHospitals.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load hospitals');
        this.isLoadingHospitals.set(false);
      }
    });
  }

  setActiveTab(tab: 'upcoming' | 'past' | 'doctors' | 'hospitals'): void {
    this.activeTab.set(tab);
    if (tab === 'doctors') {
      this.loadDoctors();
    } else if (tab === 'hospitals') {
      this.loadHospitals();
    }
  }

  cancelAppointment(appointment: Appointment): void {
    // Check if appointment is within 60 minutes
    const appointmentDateTime = new Date(`${appointment.appointmentDate}T${appointment.appointmentTime}`);
    const timeUntilAppointment = appointmentDateTime.getTime() - new Date().getTime();
    const minutesUntilAppointment = timeUntilAppointment / (1000 * 60);
    
    if (minutesUntilAppointment < 60) {
      this.toastService.error('Cancellations must be requested at least 60 minutes before the appointment time.');
      return;
    }

    // Predefined cancellation reasons
    const reasons = [
      'Personal emergency',
      'Schedule conflict',
      'Feeling better, no longer need appointment',
      'Transportation issues',
      'Financial reasons',
      'Other'
    ];

    const reasonsText = reasons.map((r, i) => `${i + 1}. ${r}`).join('\n');
    const selection = prompt(`Please select a reason for cancellation:\n\n${reasonsText}\n\nEnter number (1-${reasons.length}) or type your own reason:`);
    
    if (selection === null) return; // User cancelled

    let reason: string;
    const selectionNum = parseInt(selection);
    if (selectionNum >= 1 && selectionNum <= reasons.length) {
      reason = reasons[selectionNum - 1];
      if (reason === 'Other') {
        const customReason = prompt('Please specify your reason:');
        if (!customReason) return;
        reason = customReason;
      }
    } else {
      reason = selection;
    }

    if (!reason.trim()) {
      this.toastService.error('Please provide a cancellation reason');
      return;
    }

    this.appointmentService.requestCancellation(appointment.appointmentId, reason).subscribe({
      next: () => {
        this.toastService.success('Cancellation request submitted successfully. Your doctor will review and approve the request.');
        if (this.patientId()) {
          this.loadAppointments(this.patientId()!);
        }
      },
      error: (error: any) => {
        this.toastService.error(error.error?.message || 'Failed to submit cancellation request');
      }
    });
  }

  bookAppointment(doctorId: string): void {
    this.router.navigate(['/book-appointment', doctorId]);
  }

  loadDoctorNamesForAppointments(appointments: Appointment[]): void {
    const ids = [...new Set((appointments ?? []).map(a => a.doctorId).filter(Boolean))];
    ids.forEach((doctorId) => {
      if (this.doctorNameMap().has(doctorId)) return;

      this.doctorService.getDoctorById(doctorId).subscribe({
        next: (doctor) => {
          const nextMap = new Map(this.doctorNameMap());
          nextMap.set(doctorId, doctor?.name ?? doctorId);
          this.doctorNameMap.set(nextMap);
        },
        error: () => {
          const nextMap = new Map(this.doctorNameMap());
          nextMap.set(doctorId, doctorId);
          this.doctorNameMap.set(nextMap);
        }
      });
    });
  }

  getDoctorDisplayName(apt: Appointment): string {
    const id = apt?.doctorId;
    if (!id) return '';
    return this.doctorNameMap().get(id) || apt.doctorName || apt.doctor?.name || id;
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
