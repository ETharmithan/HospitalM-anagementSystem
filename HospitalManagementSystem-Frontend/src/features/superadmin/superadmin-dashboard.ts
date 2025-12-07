import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AdminDashboardService } from '../../core/services/admin-dashboard-service';
import { HospitalService, Hospital, CreateHospitalRequest } from '../../core/services/hospital-service';
import { UserManagementService, UserInfo, CreateUserRequest } from '../../core/services/user-management-service';
import { AccountService } from '../../core/services/account-service';
import { ToastService } from '../../core/services/toast-service';
import { AdminOverview } from '../../types/admin-overview';

@Component({
  selector: 'app-superadmin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './superadmin-dashboard.html',
  styleUrl: './superadmin-dashboard.css',
})
export class SuperAdminDashboard implements OnInit {
  private dashboardService = inject(AdminDashboardService);
  private hospitalService = inject(HospitalService);
  private userService = inject(UserManagementService);
  private accountService = inject(AccountService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  // Overview data
  overview = signal<AdminOverview | null>(null);
  isLoading = signal(true);
  
  // Tab management
  activeTab = signal<'overview' | 'hospitals' | 'admins' | 'doctors'>('overview');
  
  // Hospitals
  hospitals = signal<Hospital[]>([]);
  isLoadingHospitals = signal(false);
  
  // Admins
  admins = signal<UserInfo[]>([]);
  isLoadingAdmins = signal(false);
  
  // Modals
  showHospitalModal = signal(false);
  showAdminModal = signal(false);
  showAssignAdminModal = signal(false);
  isSubmitting = signal(false);
  editingHospital = signal<Hospital | null>(null);
  selectedHospitalForAdmin = signal<Hospital | null>(null);
  
  // Forms
  hospitalForm: CreateHospitalRequest = {
    name: '',
    address: '',
    city: '',
    state: '',
    country: 'Sri Lanka',
    postalCode: '',
    phoneNumber: '',
    email: '',
    website: '',
    description: ''
  };
  
  // Admin form with hospital selection
  adminForm = {
    email: '',
    displayName: '',
    password: '',
    role: 'Admin',
    hospitalId: ''
  };
  
  // Hospital form with initial admin
  hospitalAdminForm = {
    email: '',
    displayName: '',
    password: ''
  };

  get userName(): string {
    return this.accountService.currentUser()?.displayName || 'Super Admin';
  }

  ngOnInit(): void {
    this.loadOverview();
    this.loadHospitals();
    this.loadAdmins();
  }

  // Tab switching
  switchTab(tab: 'overview' | 'hospitals' | 'admins' | 'doctors'): void {
    this.activeTab.set(tab);
  }

  // Load overview data
  loadOverview(): void {
    this.isLoading.set(true);
    this.dashboardService.getOverview().subscribe({
      next: (data) => {
        this.overview.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  // Load hospitals
  loadHospitals(): void {
    this.isLoadingHospitals.set(true);
    this.hospitalService.getAllHospitals().subscribe({
      next: (data) => {
        this.hospitals.set(data);
        this.isLoadingHospitals.set(false);
      },
      error: () => {
        this.isLoadingHospitals.set(false);
        this.toastService.error('Failed to load hospitals');
      }
    });
  }

  // Load admins
  loadAdmins(): void {
    this.isLoadingAdmins.set(true);
    this.userService.getUsersByRole('Admin').subscribe({
      next: (data) => {
        this.admins.set(data);
        this.isLoadingAdmins.set(false);
      },
      error: () => {
        this.isLoadingAdmins.set(false);
      }
    });
  }

  // Hospital CRUD
  openCreateHospitalModal(): void {
    this.editingHospital.set(null);
    this.resetHospitalForm();
    this.showHospitalModal.set(true);
  }

  openEditHospitalModal(hospital: Hospital): void {
    this.editingHospital.set(hospital);
    this.hospitalForm = {
      name: hospital.name,
      address: hospital.address || '',
      city: hospital.city || '',
      state: hospital.state || '',
      country: hospital.country || 'Sri Lanka',
      postalCode: hospital.postalCode || '',
      phoneNumber: hospital.phoneNumber || '',
      email: hospital.email || '',
      website: hospital.website || '',
      description: hospital.description || ''
    };
    this.showHospitalModal.set(true);
  }

  closeHospitalModal(): void {
    this.showHospitalModal.set(false);
    this.resetHospitalForm();
  }

  resetHospitalForm(): void {
    this.hospitalForm = {
      name: '',
      address: '',
      city: '',
      state: '',
      country: 'Sri Lanka',
      postalCode: '',
      phoneNumber: '',
      email: '',
      website: '',
      description: ''
    };
    this.hospitalAdminForm = {
      email: '',
      displayName: '',
      password: ''
    };
  }

  saveHospital(): void {
    // Validate required fields
    if (!this.hospitalForm.name || !this.hospitalForm.address || !this.hospitalForm.city ||
        !this.hospitalForm.state || !this.hospitalForm.country || !this.hospitalForm.postalCode ||
        !this.hospitalForm.phoneNumber || !this.hospitalForm.email) {
      this.toastService.error('Please fill all required fields (marked with *)');
      return;
    }

    const editing = this.editingHospital();
    
    // For new hospital, require admin details
    if (!editing) {
      if (!this.hospitalAdminForm.email || !this.hospitalAdminForm.displayName || !this.hospitalAdminForm.password) {
        this.toastService.error('Admin details are required for new hospital');
        return;
      }
      if (this.hospitalAdminForm.password.length < 8) {
        this.toastService.error('Admin password must be at least 8 characters');
        return;
      }
    }

    this.isSubmitting.set(true);

    if (editing) {
      this.hospitalService.updateHospital(editing.hospitalId, this.hospitalForm).subscribe({
        next: () => {
          this.toastService.success('Hospital updated successfully');
          this.closeHospitalModal();
          this.loadHospitals();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.toastService.error(err.message || 'Failed to update hospital');
          this.isSubmitting.set(false);
        }
      });
    } else {
      // Create hospital first, then create admin and assign
      this.hospitalService.createHospital(this.hospitalForm).subscribe({
        next: (hospital: any) => {
          const hospitalId = hospital.hospitalId || hospital.HospitalId;
          
          // Now create the admin user
          const adminRequest = {
            email: this.hospitalAdminForm.email,
            displayName: this.hospitalAdminForm.displayName,
            password: this.hospitalAdminForm.password,
            role: 'Admin'
          };
          
          this.userService.createUser(adminRequest).subscribe({
            next: (userResponse: any) => {
              const userId = userResponse.userId || userResponse.user?.id || userResponse.user?.userId;
              
              if (userId && hospitalId) {
                // Assign admin to hospital
                this.hospitalService.assignHospitalAdmin(hospitalId, userId).subscribe({
                  next: () => {
                    this.toastService.success('Hospital and admin created successfully');
                    this.closeHospitalModal();
                    this.loadHospitals();
                    this.loadAdmins();
                    this.loadOverview();
                    this.isSubmitting.set(false);
                  },
                  error: () => {
                    this.toastService.warning('Hospital created, admin created, but assignment failed');
                    this.closeHospitalModal();
                    this.loadHospitals();
                    this.loadAdmins();
                    this.isSubmitting.set(false);
                  }
                });
              } else {
                this.toastService.success('Hospital and admin created successfully');
                this.closeHospitalModal();
                this.loadHospitals();
                this.loadAdmins();
                this.loadOverview();
                this.isSubmitting.set(false);
              }
            },
            error: (err) => {
              this.toastService.warning('Hospital created but failed to create admin: ' + (err.message || 'Unknown error'));
              this.closeHospitalModal();
              this.loadHospitals();
              this.isSubmitting.set(false);
            }
          });
        },
        error: (err) => {
          this.toastService.error(err.message || 'Failed to create hospital');
          this.isSubmitting.set(false);
        }
      });
    }
  }

  deleteHospital(hospital: Hospital): void {
    if (!confirm(`Are you sure you want to delete "${hospital.name}"?`)) return;

    this.hospitalService.deleteHospital(hospital.hospitalId).subscribe({
      next: () => {
        this.toastService.success('Hospital deleted successfully');
        this.loadHospitals();
        this.loadOverview();
      },
      error: (err) => {
        this.toastService.error(err.message);
      }
    });
  }

  // Admin CRUD
  openCreateAdminModal(): void {
    this.resetAdminForm();
    // Always reload hospitals for the dropdown to ensure fresh data
    this.loadHospitals();
    this.showAdminModal.set(true);
  }

  closeAdminModal(): void {
    this.showAdminModal.set(false);
    this.resetAdminForm();
  }

  resetAdminForm(): void {
    this.adminForm = {
      email: '',
      displayName: '',
      password: '',
      role: 'Admin',
      hospitalId: ''
    };
  }

  createAdmin(): void {
    if (!this.adminForm.email || !this.adminForm.displayName || !this.adminForm.password) {
      this.toastService.error('Please fill all required fields');
      return;
    }

    if (!this.adminForm.hospitalId) {
      this.toastService.error('Please select a hospital for this admin');
      return;
    }

    if (this.adminForm.password.length < 8) {
      this.toastService.error('Password must be at least 8 characters');
      return;
    }

    this.isSubmitting.set(true);
    
    // Create admin user with hospital assignment
    const createRequest = {
      email: this.adminForm.email,
      displayName: this.adminForm.displayName,
      password: this.adminForm.password,
      role: 'Admin'
    };
    
    this.userService.createUser(createRequest).subscribe({
      next: (response: any) => {
        // After creating user, assign to hospital
        const userId = response.userId || response.user?.id || response.user?.userId;
        if (userId && this.adminForm.hospitalId) {
          this.hospitalService.assignHospitalAdmin(this.adminForm.hospitalId, userId).subscribe({
            next: () => {
              this.toastService.success('Admin created and assigned to hospital successfully');
              this.closeAdminModal();
              this.loadAdmins();
              this.loadHospitals();
              this.loadOverview();
              this.isSubmitting.set(false);
            },
            error: (err) => {
              this.toastService.warning('Admin created but failed to assign to hospital: ' + err.message);
              this.closeAdminModal();
              this.loadAdmins();
              this.isSubmitting.set(false);
            }
          });
        } else {
          this.toastService.success('Admin created successfully');
          this.closeAdminModal();
          this.loadAdmins();
          this.loadOverview();
          this.isSubmitting.set(false);
        }
      },
      error: (err) => {
        this.toastService.error(err.message || 'Failed to create admin');
        this.isSubmitting.set(false);
      }
    });
  }

  // Assign admin to hospital
  openAssignAdminModal(hospital: Hospital): void {
    this.selectedHospitalForAdmin.set(hospital);
    this.showAssignAdminModal.set(true);
  }

  closeAssignAdminModal(): void {
    this.showAssignAdminModal.set(false);
    this.selectedHospitalForAdmin.set(null);
  }

  assignAdminToHospital(admin: UserInfo): void {
    const hospital = this.selectedHospitalForAdmin();
    if (!hospital) return;

    this.hospitalService.assignHospitalAdmin(hospital.hospitalId, admin.userId).subscribe({
      next: () => {
        this.toastService.success(`${admin.username} assigned to ${hospital.name}`);
        this.closeAssignAdminModal();
        this.loadHospitals();
      },
      error: (err) => {
        this.toastService.error(err.message);
      }
    });
  }

  // Logout
  onLogout(): void {
    this.accountService.logout();
    this.router.navigate(['/login']);
  }
}
