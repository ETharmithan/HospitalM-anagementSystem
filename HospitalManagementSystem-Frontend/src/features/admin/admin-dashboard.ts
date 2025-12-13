import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminDashboardService, HospitalInfo } from '../../core/services/admin-dashboard-service';
import { HospitalAdminService, Doctor, Department, DoctorSchedule, CreateDoctorRequest, CreateDepartmentRequest, CreateScheduleRequest } from '../../core/services/hospital-admin-service';
import { ToastService } from '../../core/services/toast-service';
import { AccountService } from '../../core/services/account-service';
import { AdminOverview } from '../../types/admin-overview';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard implements OnInit {
  private dashboardService = inject(AdminDashboardService);
  private adminService = inject(HospitalAdminService);
  private toastService = inject(ToastService);
  private accountService = inject(AccountService);
  private router = inject(Router);

  // User info
  currentUser = computed(() => this.accountService.currentUser());
  
  // Overview data
  overview = signal<AdminOverview | null>(null);
  hospitalInfo = signal<HospitalInfo | null>(null);
  
  // Lists
  doctors = signal<Doctor[]>([]);
  departments = signal<Department[]>([]);
  schedules = signal<DoctorSchedule[]>([]);
  
  // Loading states
  isLoading = signal(true);
  isLoadingDoctors = signal(false);
  isLoadingDepartments = signal(false);
  isLoadingSchedules = signal(false);
  isSubmitting = signal(false);
  
  // UI state
  activeTab = signal<'overview' | 'doctors' | 'departments' | 'schedules' | 'appointments'>('overview');
  errorMessage = signal('');
  
  // Modals
  showDoctorModal = signal(false);
  showDepartmentModal = signal(false);
  showScheduleModal = signal(false);
  
  // Editing state
  editingDoctor = signal<Doctor | null>(null);
  editingDepartment = signal<Department | null>(null);
  selectedDoctorForSchedule = signal<Doctor | null>(null);
  
  // Forms
  doctorForm: CreateDoctorRequest = {
    name: '',
    email: '',
    phone: '',
    departmentId: '',
    qualification: '',
    licenseNumber: '',
    status: 'Active',
    profileImage: ''
  };
  
  departmentForm: CreateDepartmentRequest = {
    name: '',
    description: '',
    hospitalId: ''
  };
  
  scheduleForm: CreateScheduleRequest = {
    doctorId: '',
    dayOfWeek: 1,
    startTime: '09:00',
    endTime: '17:00',
    maxPatients: 20
  };

  // Days of week for schedule
  daysOfWeek = [
    { value: 0, name: 'Sunday' },
    { value: 1, name: 'Monday' },
    { value: 2, name: 'Tuesday' },
    { value: 3, name: 'Wednesday' },
    { value: 4, name: 'Thursday' },
    { value: 5, name: 'Friday' },
    { value: 6, name: 'Saturday' }
  ];

  ngOnInit(): void {
    this.loadHospitalInfo();
    this.loadOverview();
    this.loadDepartments();
    this.loadDoctors();
  }

  // Load hospital info
  private loadHospitalInfo(): void {
    this.dashboardService.getHospitalInfo().subscribe({
      next: (data) => {
        this.hospitalInfo.set(data);
      },
      error: (error) => {
        console.error('Error loading hospital info:', error);
        this.toastService.error('Failed to load hospital information');
      }
    });
  }

  setActiveTab(tab: 'overview' | 'doctors' | 'departments' | 'schedules' | 'appointments'): void {
    this.activeTab.set(tab);
    if (tab === 'doctors') this.loadDoctors();
    if (tab === 'departments') this.loadDepartments();
  }

  // Data Loading
  private loadOverview(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.dashboardService.getOverview().subscribe({
      next: (data) => {
        this.overview.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error?.message ?? 'Unable to load overview.');
        this.isLoading.set(false);
      }
    });
  }

  loadDoctors(): void {
    this.isLoadingDoctors.set(true);
    this.adminService.getDoctors().subscribe({
      next: (data) => {
        this.doctors.set(data);
        this.isLoadingDoctors.set(false);
      },
      error: (err) => {
        this.toastService.error(err.message);
        this.isLoadingDoctors.set(false);
      }
    });
  }

  loadDepartments(): void {
    this.isLoadingDepartments.set(true);
    this.adminService.getDepartments().subscribe({
      next: (data) => {
        this.departments.set(data);
        this.isLoadingDepartments.set(false);
      },
      error: (err) => {
        this.toastService.error(err.message);
        this.isLoadingDepartments.set(false);
      }
    });
  }

  loadDoctorSchedules(doctor: Doctor): void {
    this.selectedDoctorForSchedule.set(doctor);
    this.isLoadingSchedules.set(true);
    this.adminService.getDoctorSchedules(doctor.doctorId).subscribe({
      next: (data) => {
        this.schedules.set(data);
        this.isLoadingSchedules.set(false);
      },
      error: (err) => {
        this.toastService.error(err.message);
        this.isLoadingSchedules.set(false);
      }
    });
  }

  // Doctor CRUD
  openCreateDoctorModal(): void {
    this.editingDoctor.set(null);
    this.resetDoctorForm();
    this.showDoctorModal.set(true);
  }

  openEditDoctorModal(doctor: Doctor): void {
    this.editingDoctor.set(doctor);
    this.doctorForm = {
      name: doctor.name,
      email: doctor.email,
      phone: doctor.phone || '',
      departmentId: doctor.departmentId,
      qualification: doctor.qualification || '',
      licenseNumber: doctor.licenseNumber || '',
      status: doctor.status,
      profileImage: doctor.profileImage || ''
    };
    this.showDoctorModal.set(true);
  }

  closeDoctorModal(): void {
    this.showDoctorModal.set(false);
    this.resetDoctorForm();
  }

  resetDoctorForm(): void {
    this.doctorForm = {
      name: '',
      email: '',
      phone: '',
      departmentId: '',
      qualification: '',
      licenseNumber: '',
      status: 'Active',
      profileImage: ''
    };
  }

  saveDoctor(): void {
    if (!this.doctorForm.name || !this.doctorForm.email || !this.doctorForm.departmentId || !this.doctorForm.qualification || !this.doctorForm.licenseNumber) {
      this.toastService.error('Please fill all required fields');
      return;
    }

    this.isSubmitting.set(true);
    const editing = this.editingDoctor();

    if (editing) {
      this.adminService.updateDoctor(editing.doctorId, this.doctorForm).subscribe({
        next: () => {
          this.toastService.success('Doctor updated successfully');
          this.closeDoctorModal();
          this.loadDoctors();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.toastService.error(err.message);
          this.isSubmitting.set(false);
        }
      });
    } else {
      this.adminService.createDoctor(this.doctorForm).subscribe({
        next: () => {
          this.toastService.success('Doctor created successfully');
          this.closeDoctorModal();
          this.loadDoctors();
          this.loadOverview();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.toastService.error(err.message);
          this.isSubmitting.set(false);
        }
      });
    }
  }

  deleteDoctor(doctor: Doctor): void {
    if (!confirm(`Are you sure you want to delete Dr. ${doctor.name}?`)) {
      return;
    }

    this.adminService.deleteDoctor(doctor.doctorId).subscribe({
      next: () => {
        this.toastService.success('Doctor deleted successfully');
        this.loadDoctors();
        this.loadOverview();
      },
      error: (err) => {
        this.toastService.error(err.message);
      }
    });
  }

  // Department CRUD
  openCreateDepartmentModal(): void {
    this.editingDepartment.set(null);
    this.resetDepartmentForm();
    this.showDepartmentModal.set(true);
  }

  openEditDepartmentModal(department: Department): void {
    this.editingDepartment.set(department);
    this.departmentForm = {
      name: department.name,
      description: department.description || '',
      hospitalId: department.hospitalId
    };
    this.showDepartmentModal.set(true);
  }

  closeDepartmentModal(): void {
    this.showDepartmentModal.set(false);
    this.resetDepartmentForm();
  }

  resetDepartmentForm(): void {
    this.departmentForm = {
      name: '',
      description: ''
    };
  }

  saveDepartment(): void {
    if (!this.departmentForm.name) {
      this.toastService.error('Department name is required');
      return;
    }

    this.isSubmitting.set(true);
    const editing = this.editingDepartment();

    if (editing) {
      this.adminService.updateDepartment(editing.departmentId, this.departmentForm).subscribe({
        next: () => {
          this.toastService.success('Department updated successfully');
          this.closeDepartmentModal();
          this.loadDepartments();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.toastService.error(err.message);
          this.isSubmitting.set(false);
        }
      });
    } else {
      this.adminService.createDepartment(this.departmentForm).subscribe({
        next: () => {
          this.toastService.success('Department created successfully');
          this.closeDepartmentModal();
          this.loadDepartments();
          this.loadOverview();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.toastService.error(err.message);
          this.isSubmitting.set(false);
        }
      });
    }
  }

  deleteDepartment(department: Department): void {
    if (!confirm(`Are you sure you want to delete ${department.name}?`)) {
      return;
    }

    this.adminService.deleteDepartment(department.departmentId).subscribe({
      next: () => {
        this.toastService.success('Department deleted successfully');
        this.loadDepartments();
        this.loadOverview();
      },
      error: (err) => {
        this.toastService.error(err.message);
      }
    });
  }

  // Schedule Management
  openScheduleModal(doctor: Doctor): void {
    this.selectedDoctorForSchedule.set(doctor);
    this.loadDoctorSchedules(doctor);
    this.resetScheduleForm();
    this.scheduleForm.doctorId = doctor.doctorId;
    this.showScheduleModal.set(true);
  }

  closeScheduleModal(): void {
    this.showScheduleModal.set(false);
    this.selectedDoctorForSchedule.set(null);
    this.schedules.set([]);
  }

  resetScheduleForm(): void {
    this.scheduleForm = {
      doctorId: '',
      dayOfWeek: 1,
      startTime: '09:00',
      endTime: '17:00',
      maxPatients: 20
    };
  }

  addSchedule(): void {
    const doctor = this.selectedDoctorForSchedule();
    if (!doctor) return;

    this.isSubmitting.set(true);
    this.adminService.createDoctorSchedule(doctor.doctorId, this.scheduleForm).subscribe({
      next: () => {
        this.toastService.success('Schedule added successfully');
        this.loadDoctorSchedules(doctor);
        this.resetScheduleForm();
        this.scheduleForm.doctorId = doctor.doctorId;
        this.isSubmitting.set(false);
      },
      error: (err) => {
        this.toastService.error(err.message);
        this.isSubmitting.set(false);
      }
    });
  }

  deleteSchedule(schedule: DoctorSchedule): void {
    const doctor = this.selectedDoctorForSchedule();
    if (!doctor) return;

    if (!confirm('Are you sure you want to delete this schedule?')) {
      return;
    }

    this.adminService.deleteDoctorSchedule(doctor.doctorId, schedule.scheduleId).subscribe({
      next: () => {
        this.toastService.success('Schedule deleted successfully');
        this.loadDoctorSchedules(doctor);
      },
      error: (err) => {
        this.toastService.error(err.message);
      }
    });
  }

  getDayName(dayOfWeek: number): string {
    return this.daysOfWeek.find(d => d.value === dayOfWeek)?.name || '';
  }

  // Logout
  logout(): void {
    this.accountService.logout();
    this.router.navigate(['/']);
  }
}
