import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AppointmentService } from '../../../core/services/appointment-service';
import { AccountService } from '../../../core/services/account-service';
import { ToastService } from '../../../core/services/toast-service';
import { DoctorScheduleService, DoctorSchedule, HospitalOption, CreateScheduleRequest } from '../../../core/services/doctor-schedule-service';
import { PrescriptionService, PrescriptionRequest, PatientMedicalProfile } from '../../../core/services/prescription-service';
import { Appointment } from '../../../types/doctor';

interface PatientInfo {
  patientId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

interface MedicalProfileModal {
  isOpen: boolean;
  isLoading: boolean;
  profile: PatientMedicalProfile | null;
}

interface PrescriptionForm {
  diagnosis: string;
  prescription: string;
  notes: string;
}

interface ScheduleForm {
  scheduleDate: string;
  startTime: string;
  endTime: string;
  hospitalId: string;
}

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DatePipe],
  templateUrl: './doctor-dashboard.html',
  styleUrl: './doctor-dashboard.css',
})
export class DoctorDashboard implements OnInit {
  private appointmentService = inject(AppointmentService);
  private scheduleService = inject(DoctorScheduleService);
  private prescriptionService = inject(PrescriptionService);
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
  activeTab = signal<'today' | 'upcoming' | 'all' | 'availability'>('today');
  
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

  // Availability/Schedule management
  schedules = signal<DoctorSchedule[]>([]);
  hospitals = signal<HospitalOption[]>([]);
  showScheduleModal = signal(false);
  isLoadingSchedules = signal(false);
  isSubmittingSchedule = signal(false);
  scheduleForm: ScheduleForm = {
    scheduleDate: '',
    startTime: '09:00',
    endTime: '17:00',
    hospitalId: ''
  };

  // Schedule filters
  scheduleViewMode = signal<'byHospital' | 'byDate' | 'byPeriod'>('byHospital');
  scheduleFilterHospital = signal<string>('all');
  scheduleFilterStartDate = signal<string>('');
  scheduleFilterEndDate = signal<string>('');

  // Store the actual doctor ID
  doctorId = signal<string | null>(null);

  // Patient Medical Profile Modal
  medicalProfileModal = signal<MedicalProfileModal>({
    isOpen: false,
    isLoading: false,
    profile: null
  });

  ngOnInit(): void {
    this.loadDoctorIdAndAppointments();
    this.loadHospitals();
  }

  loadDoctorIdAndAppointments(): void {
    this.isLoading.set(true);
    
    // First get the doctor ID from the backend using the logged-in user's email
    this.scheduleService.getMyDoctorId().subscribe({
      next: (result) => {
        this.doctorId.set(result.doctorId);
        this.loadAppointmentsByDoctorId(result.doctorId);
      },
      error: (error) => {
        console.error('Failed to get doctor ID:', error);
        // Fallback to loading all and filtering
        this.loadAppointmentsFallback();
      }
    });
  }

  loadAppointmentsByDoctorId(doctorId: string): void {
    this.appointmentService.getAppointmentsByDoctorId(doctorId).subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.calculateStats(appointments);
        this.filterAppointments(appointments);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.toastService.error('Failed to load appointments');
        this.isLoading.set(false);
      }
    });
  }

  loadAppointmentsFallback(): void {
    const user = this.accountService.currentUser();
    
    this.appointmentService.getAllAppointments().subscribe({
      next: (appointments) => {
        // Filter appointments for this doctor (matching by email)
        const doctorAppointments = appointments.filter(apt => 
          apt.doctor?.email === user?.email
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

  setActiveTab(tab: 'today' | 'upcoming' | 'all' | 'availability'): void {
    this.activeTab.set(tab);
    if (tab === 'availability') {
      this.loadSchedules();
    }
  }

  // Schedule/Availability methods
  loadHospitals(): void {
    this.scheduleService.getHospitals().subscribe({
      next: (hospitals) => {
        this.hospitals.set(hospitals);
      },
      error: (error) => {
        console.error('Failed to load hospitals:', error);
      }
    });
  }

  loadSchedules(): void {
    this.isLoadingSchedules.set(true);
    // Use the new endpoint that gets schedules for current logged-in doctor
    this.scheduleService.getMySchedules().subscribe({
      next: (schedules) => {
        this.schedules.set(schedules);
        this.isLoadingSchedules.set(false);
      },
      error: (error) => {
        this.toastService.error('Failed to load schedules');
        this.isLoadingSchedules.set(false);
      }
    });
  }

  openScheduleModal(): void {
    // Set default date to today
    const today = new Date();
    const dateStr = today.toISOString().split('T')[0];
    this.scheduleForm = {
      scheduleDate: dateStr,
      startTime: '09:00',
      endTime: '17:00',
      hospitalId: this.hospitals().length > 0 ? this.hospitals()[0].hospitalId : ''
    };
    this.showScheduleModal.set(true);
  }

  closeScheduleModal(): void {
    this.showScheduleModal.set(false);
  }

  submitSchedule(): void {
    if (!this.scheduleForm.scheduleDate || !this.scheduleForm.startTime || !this.scheduleForm.endTime) {
      this.toastService.error('Please fill in all required fields');
      return;
    }

    if (!this.scheduleForm.hospitalId) {
      this.toastService.error('Please select a hospital');
      return;
    }

    this.isSubmittingSchedule.set(true);

    // doctorId will be set by the backend from the logged-in user's email
    const request: CreateScheduleRequest = {
      doctorId: '00000000-0000-0000-0000-000000000000', // Will be overridden by backend
      scheduleDate: this.scheduleForm.scheduleDate,
      isRecurring: false,
      startTime: this.scheduleForm.startTime,
      endTime: this.scheduleForm.endTime,
      hospitalId: this.scheduleForm.hospitalId
    };

    this.scheduleService.createSchedule(request).subscribe({
      next: () => {
        this.toastService.success('Schedule added successfully');
        this.isSubmittingSchedule.set(false);
        this.closeScheduleModal();
        this.loadSchedules();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to add schedule');
        this.isSubmittingSchedule.set(false);
      }
    });
  }

  deleteSchedule(schedule: DoctorSchedule): void {
    const dateStr = schedule.scheduleDate ? new Date(schedule.scheduleDate).toLocaleDateString() : schedule.dayOfWeek;
    if (confirm(`Are you sure you want to delete this schedule for ${dateStr}?`)) {
      this.scheduleService.deleteSchedule(schedule.scheduleId).subscribe({
        next: () => {
          this.toastService.success('Schedule deleted successfully');
          this.loadSchedules();
        },
        error: (error) => {
          this.toastService.error('Failed to delete schedule');
        }
      });
    }
  }

  getSchedulesByHospital(): Map<string, DoctorSchedule[]> {
    const grouped = new Map<string, DoctorSchedule[]>();
    for (const schedule of this.schedules()) {
      const key = schedule.hospitalName || 'General';
      if (!grouped.has(key)) {
        grouped.set(key, []);
      }
      grouped.get(key)!.push(schedule);
    }
    return grouped;
  }

  cancelAppointment(appointment: Appointment): void {
    if (confirm('Are you sure you want to cancel this appointment?')) {
      this.appointmentService.cancelAppointment(appointment.appointmentId).subscribe({
        next: () => {
          this.toastService.success('Appointment cancelled successfully');
          this.loadDoctorIdAndAppointments();
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

    const apt = this.selectedAppointment();
    const doctorId = this.doctorId();
    
    if (!apt || !doctorId) {
      this.toastService.error('Missing appointment or doctor information');
      return;
    }

    this.isSubmittingPrescription.set(true);
    
    const prescriptionData: PrescriptionRequest = {
      diagnosis: this.prescriptionForm.diagnosis,
      prescription: this.prescriptionForm.prescription,
      notes: this.prescriptionForm.notes,
      visitDate: new Date().toISOString(),
      doctorId: doctorId,
      patientId: apt.patientId
    };

    this.prescriptionService.createPrescription(prescriptionData).subscribe({
      next: () => {
        this.toastService.success('Prescription saved and emailed to patient!');
        this.isSubmittingPrescription.set(false);
        this.closePrescriptionModal();
        
        // Mark appointment as completed
        this.appointmentService.updateAppointment(apt.appointmentId, {
          ...apt,
          appointmentStatus: 'Completed',
          createdDate: apt.createdDate
        }).subscribe({
          next: () => this.loadDoctorIdAndAppointments(),
          error: () => {}
        });
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to save prescription');
        this.isSubmittingPrescription.set(false);
      }
    });
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

  getHospitalSchedules(): { hospitalName: string; schedules: DoctorSchedule[] }[] {
    const grouped = this.getSchedulesByHospital();
    return Array.from(grouped.entries()).map(([hospitalName, schedules]) => ({
      hospitalName,
      schedules: schedules.sort((a, b) => {
        // Sort by date first, then by start time
        const dateA = a.scheduleDate ? new Date(a.scheduleDate).getTime() : 0;
        const dateB = b.scheduleDate ? new Date(b.scheduleDate).getTime() : 0;
        if (dateA !== dateB) return dateA - dateB;
        return a.startTime.localeCompare(b.startTime);
      })
    }));
  }

  formatScheduleDate(dateStr: string | undefined): string {
    if (!dateStr) return 'N/A';
    return new Date(dateStr).toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  // View Patient Medical Profile
  viewPatientProfile(appointment: Appointment): void {
    this.medicalProfileModal.set({
      isOpen: true,
      isLoading: true,
      profile: null
    });

    this.prescriptionService.getPatientMedicalProfile(appointment.patientId).subscribe({
      next: (profile) => {
        this.medicalProfileModal.set({
          isOpen: true,
          isLoading: false,
          profile
        });
      },
      error: (error) => {
        this.toastService.error('Failed to load patient profile');
        this.medicalProfileModal.set({
          isOpen: false,
          isLoading: false,
          profile: null
        });
      }
    });
  }

  closeMedicalProfileModal(): void {
    this.medicalProfileModal.set({
      isOpen: false,
      isLoading: false,
      profile: null
    });
  }

  formatProfileDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  // Schedule filtering methods
  setScheduleViewMode(mode: 'byHospital' | 'byDate' | 'byPeriod'): void {
    this.scheduleViewMode.set(mode);
  }

  setScheduleFilterHospital(hospitalId: string): void {
    this.scheduleFilterHospital.set(hospitalId);
  }

  setScheduleFilterDates(startDate: string, endDate: string): void {
    this.scheduleFilterStartDate.set(startDate);
    this.scheduleFilterEndDate.set(endDate);
  }

  clearScheduleFilters(): void {
    this.scheduleFilterHospital.set('all');
    this.scheduleFilterStartDate.set('');
    this.scheduleFilterEndDate.set('');
  }

  getFilteredSchedules(): DoctorSchedule[] {
    let filtered = [...this.schedules()];
    
    // Filter by hospital
    const hospitalFilter = this.scheduleFilterHospital();
    if (hospitalFilter !== 'all') {
      filtered = filtered.filter(s => s.hospitalId === hospitalFilter);
    }
    
    // Filter by date range
    const startDate = this.scheduleFilterStartDate();
    const endDate = this.scheduleFilterEndDate();
    
    if (startDate) {
      const start = new Date(startDate);
      filtered = filtered.filter(s => s.scheduleDate && new Date(s.scheduleDate) >= start);
    }
    
    if (endDate) {
      const end = new Date(endDate);
      filtered = filtered.filter(s => s.scheduleDate && new Date(s.scheduleDate) <= end);
    }
    
    return filtered;
  }

  getSchedulesByDateSorted(): DoctorSchedule[] {
    return this.getFilteredSchedules().sort((a, b) => {
      const dateA = a.scheduleDate ? new Date(a.scheduleDate).getTime() : 0;
      const dateB = b.scheduleDate ? new Date(b.scheduleDate).getTime() : 0;
      return dateA - dateB;
    });
  }

  getFilteredHospitalSchedules(): { hospitalName: string; hospitalId: string; schedules: DoctorSchedule[] }[] {
    const filtered = this.getFilteredSchedules();
    const grouped = new Map<string, DoctorSchedule[]>();
    
    filtered.forEach(schedule => {
      const hospitalName = schedule.hospitalName || 'Unknown Hospital';
      if (!grouped.has(hospitalName)) {
        grouped.set(hospitalName, []);
      }
      grouped.get(hospitalName)!.push(schedule);
    });

    return Array.from(grouped.entries()).map(([hospitalName, schedules]) => ({
      hospitalName,
      hospitalId: schedules[0]?.hospitalId || '',
      schedules: schedules.sort((a, b) => {
        const dateA = a.scheduleDate ? new Date(a.scheduleDate).getTime() : 0;
        const dateB = b.scheduleDate ? new Date(b.scheduleDate).getTime() : 0;
        return dateA - dateB;
      })
    }));
  }

  getTotalFilteredCount(): number {
    return this.getFilteredSchedules().length;
  }
}
