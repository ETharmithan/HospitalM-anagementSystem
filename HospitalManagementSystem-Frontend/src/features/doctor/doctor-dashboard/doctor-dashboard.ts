import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AppointmentService } from '../../../core/services/appointment-service';
import { AccountService } from '../../../core/services/account-service';
import { ToastService } from '../../../core/services/toast-service';
import { Appointment } from '../../../types/doctor';

interface PatientInfo {
  patientId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

interface PrescriptionForm {
  diagnosis: string;
  prescription: string;
  notes: string;
}

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './doctor-dashboard.html',
  styleUrl: './doctor-dashboard.css',
})
export class DoctorDashboard implements OnInit {
  private appointmentService = inject(AppointmentService);
  accountService = inject(AccountService); // public for template access
  private toastService = inject(ToastService);
  private router = inject(Router);

  // Getter for user display name
  get userName(): string {
    return this.accountService.currentUser()?.displayName || 'Doctor';
  }

  appointments = signal<Appointment[]>([]);
  todayAppointments = signal<Appointment[]>([]);
  upcomingAppointments = signal<Appointment[]>([]);
  isLoading = signal(true);
  activeTab = signal<'today' | 'upcoming' | 'all' | 'hospitals'>('today');
  
  // Prescription modal
  showPrescriptionModal = signal(false);
  selectedAppointment = signal<Appointment | null>(null);
  prescriptionForm: PrescriptionForm = {
    diagnosis: '',
    prescription: '',
    notes: ''
  };
  isSubmittingPrescription = signal(false);

  // Stats
  totalAppointments = signal(0);
  completedAppointments = signal(0);
  cancelledAppointments = signal(0);
  pendingAppointments = signal(0);

  ngOnInit(): void {
    this.loadAppointments();
  }

  loadAppointments(): void {
    this.isLoading.set(true);
    const user = this.accountService.currentUser();
    
    // For now, load all appointments and filter by doctor
    // In production, you'd have a backend endpoint for doctor-specific appointments
    this.appointmentService.getAllAppointments().subscribe({
      next: (appointments) => {
        // Filter appointments for this doctor (matching by email for now)
        const doctorAppointments = appointments.filter(apt => 
          apt.doctorId === user?.id || apt.doctor?.email === user?.email
        );
        
        this.appointments.set(doctorAppointments);
        this.calculateStats(doctorAppointments);
        this.filterAppointments(doctorAppointments);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.toastService.error('Failed to load appointments');
        this.isLoading.set(false);
      }
    });
  }

  calculateStats(appointments: Appointment[]): void {
    this.totalAppointments.set(appointments.length);
    this.completedAppointments.set(appointments.filter(a => a.appointmentStatus === 'Completed').length);
    this.cancelledAppointments.set(appointments.filter(a => a.appointmentStatus === 'Cancelled').length);
    this.pendingAppointments.set(appointments.filter(a => 
      a.appointmentStatus === 'Scheduled' || a.appointmentStatus === 'Pending'
    ).length);
  }

  filterAppointments(appointments: Appointment[]): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    this.todayAppointments.set(
      appointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        aptDate.setHours(0, 0, 0, 0);
        return aptDate.getTime() === today.getTime() && 
               apt.appointmentStatus !== 'Cancelled';
      }).sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime))
    );

    this.upcomingAppointments.set(
      appointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        aptDate.setHours(0, 0, 0, 0);
        return aptDate.getTime() > today.getTime() && 
               apt.appointmentStatus !== 'Cancelled';
      }).sort((a, b) => {
        const dateCompare = new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime();
        return dateCompare !== 0 ? dateCompare : a.appointmentTime.localeCompare(b.appointmentTime);
      })
    );
  }

  setActiveTab(tab: 'today' | 'upcoming' | 'all' | 'hospitals'): void {
    this.activeTab.set(tab);
  }

  cancelAppointment(appointment: Appointment): void {
    if (confirm('Are you sure you want to cancel this appointment?')) {
      this.appointmentService.cancelAppointment(appointment.appointmentId).subscribe({
        next: () => {
          this.toastService.success('Appointment cancelled successfully');
          this.loadAppointments();
        },
        error: (error) => {
          this.toastService.error('Failed to cancel appointment');
        }
      });
    }
  }

  openPrescriptionModal(appointment: Appointment): void {
    this.selectedAppointment.set(appointment);
    this.prescriptionForm = { diagnosis: '', prescription: '', notes: '' };
    this.showPrescriptionModal.set(true);
  }

  closePrescriptionModal(): void {
    this.showPrescriptionModal.set(false);
    this.selectedAppointment.set(null);
  }

  submitPrescription(): void {
    if (!this.prescriptionForm.diagnosis || !this.prescriptionForm.prescription) {
      this.toastService.error('Please fill in diagnosis and prescription');
      return;
    }

    this.isSubmittingPrescription.set(true);
    
    // TODO: Implement prescription API call
    // For now, simulate success
    setTimeout(() => {
      this.toastService.success('Prescription saved successfully');
      this.isSubmittingPrescription.set(false);
      this.closePrescriptionModal();
      
      // Mark appointment as completed
      const apt = this.selectedAppointment();
      if (apt) {
        this.appointmentService.updateAppointment(apt.appointmentId, {
          ...apt,
          appointmentStatus: 'Completed',
          createdDate: apt.createdDate
        }).subscribe({
          next: () => this.loadAppointments(),
          error: () => {}
        });
      }
    }, 1000);
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
}
