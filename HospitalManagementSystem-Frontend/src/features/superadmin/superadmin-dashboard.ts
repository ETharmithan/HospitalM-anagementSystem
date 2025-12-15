import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AdminDashboardService } from '../../core/services/admin-dashboard-service';
import { HospitalService, Hospital, CreateHospitalRequest, Admin, CreateAdminDto, UpdateAdminDto, Patient, SystemUser } from '../../core/services/hospital-service';
import { LocationSearchResult, LocationService } from '../../core/services/location-service';
import { UserManagementService, UserInfo, CreateUserRequest } from '../../core/services/user-management-service';
import { AccountService } from '../../core/services/account-service';
import { ToastService } from '../../core/services/toast-service';
import { AdminOverview } from '../../types/admin-overview';
import { Nav } from '../../layout/nav/nav';

@Component({
  selector: 'app-superadmin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Nav],
  templateUrl: './superadmin-dashboard.html',
  styleUrl: './superadmin-dashboard.css',
})
export class SuperAdminDashboard implements OnInit {
  private dashboardService = inject(AdminDashboardService);
  private hospitalService = inject(HospitalService);
  private locationService = inject(LocationService);
  private userService = inject(UserManagementService);
  private accountService = inject(AccountService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);

  getHospitalMapsSearchUrlForHospital(hospital: any): string {
    if (!hospital) return '';

    const lat = hospital.latitude;
    const lng = hospital.longitude;
    if (lat != null && lng != null) {
      return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${lat},${lng}`)}`;
    }

    const parts = [
      hospital.name,
      hospital.address,
      hospital.city,
      hospital.state,
      hospital.country,
      hospital.postalCode,
    ]
      .filter(Boolean)
      .map((v: any) => String(v).trim())
      .filter((v: string) => v.length > 0);

    const query = parts.join(', ');
    if (!query) return '';
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  }

  locationSearchQuery = signal<string>('');
  locationSearchResults = signal<LocationSearchResult[]>([]);
  isSearchingLocation = signal(false);
  isUsingCurrentLocation = signal(false);
  showLocationResults = signal(false);
  private locationSearchDebounce: any = null;

  // Overview data
  overview = signal<AdminOverview | null>(null);
  isLoading = signal(true);
  
  // Tab management
  activeTab = signal<'overview' | 'hospitals' | 'admins' | 'patients' | 'users'>('overview');
  
  // Hospitals
  hospitals = signal<Hospital[]>([]);
  isLoadingHospitals = signal(false);
  
  // Admins
  admins = signal<Admin[]>([]);
  isLoadingAdmins = signal(false);
  
  // Patients
  patients = signal<Patient[]>([]);
  isLoadingPatients = signal(false);
  
  // Users
  users = signal<SystemUser[]>([]);
  isLoadingUsers = signal(false);
  
  // User filtering and sorting
  userSearchTerm = signal<string>('');
  userRoleFilter = signal<string>('all');
  userSortBy = signal<'username' | 'role'>('username');
  userSortOrder = signal<'asc' | 'desc'>('asc');
  
  // Group admins by hospital
  adminsByHospital = computed(() => {
    const grouped = new Map<string, { hospital: string; admins: Admin[] }>();
    
    // Group by hospital
    this.admins().forEach(admin => {
      if (admin.hospitals && admin.hospitals.length > 0) {
        admin.hospitals.forEach(hospital => {
          const key = hospital.hospitalId;
          if (!grouped.has(key)) {
            grouped.set(key, {
              hospital: hospital.hospitalName,
              admins: []
            });
          }
          grouped.get(key)!.admins.push(admin);
        });
      }
    });
    
    // Unassigned admins
    const unassigned = this.admins().filter(admin => !admin.hospitals || admin.hospitals.length === 0);
    if (unassigned.length > 0) {
      grouped.set('unassigned', {
        hospital: 'Unassigned Admins',
        admins: unassigned
      });
    }
    
    return Array.from(grouped.values());
  });

  // Filtered and sorted users
  filteredAndSortedUsers = computed(() => {
    let filtered = this.users();
    
    // Filter by role
    if (this.userRoleFilter() !== 'all') {
      filtered = filtered.filter(user => user.role === this.userRoleFilter());
    }
    
    // Filter by search term (username or email)
    const searchTerm = this.userSearchTerm().toLowerCase();
    if (searchTerm) {
      filtered = filtered.filter(user => 
        user.username.toLowerCase().includes(searchTerm) ||
        user.email.toLowerCase().includes(searchTerm)
      );
    }
    
    // Sort
    const sortBy = this.userSortBy();
    const sortOrder = this.userSortOrder();
    
    return filtered.sort((a, b) => {
      let comparison = 0;
      
      if (sortBy === 'username') {
        comparison = a.username.localeCompare(b.username);
      } else if (sortBy === 'role') {
        comparison = a.role.localeCompare(b.role);
      }
      
      return sortOrder === 'asc' ? comparison : -comparison;
    });
  });
  
  // Modals
  showHospitalModal = signal(false);
  showAdminModal = signal(false);
  showAssignAdminModal = signal(false);
  isSubmitting = signal(false);
  editingHospital = signal<Hospital | null>(null);
  editingAdmin = signal<Admin | null>(null);
  selectedHospitalForAdmin = signal<Hospital | null>(null);
  
  // Forms
  hospitalForm: CreateHospitalRequest = {
    name: '',
    address: '',
    city: '',
    state: '',
    country: 'Sri Lanka',
    postalCode: '',
    latitude: null,
    longitude: null,
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
    this.loadPatients();
    this.loadUsers();
  }

  // Tab switching
  switchTab(tab: 'overview' | 'hospitals' | 'admins' | 'patients' | 'users'): void {
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
    this.hospitalService.getAllAdmins().subscribe({
      next: (data) => {
        this.admins.set(data);
        this.isLoadingAdmins.set(false);
      },
      error: () => {
        this.isLoadingAdmins.set(false);
        this.toastService.error('Failed to load admins');
      }
    });
  }

  // Load patients
  loadPatients(): void {
    this.isLoadingPatients.set(true);
    this.hospitalService.getAllPatients().subscribe({
      next: (data) => {
        this.patients.set(data);
        this.isLoadingPatients.set(false);
      },
      error: () => {
        this.isLoadingPatients.set(false);
        this.toastService.error('Failed to load patients');
      }
    });
  }

  // Load users
  loadUsers(): void {
    this.isLoadingUsers.set(true);
    this.hospitalService.getAllUsers().subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoadingUsers.set(false);
      },
      error: () => {
        this.isLoadingUsers.set(false);
        this.toastService.error('Failed to load users');
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
      latitude: hospital.latitude ?? null,
      longitude: hospital.longitude ?? null,
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
    this.clearLocationSearch();
  }

  resetHospitalForm(): void {
    this.hospitalForm = {
      name: '',
      address: '',
      city: '',
      state: '',
      country: 'Sri Lanka',
      postalCode: '',
      latitude: null,
      longitude: null,
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

  onLocationSearchInput(value: string): void {
    this.locationSearchQuery.set(value);
    const query = (value || '').trim();
    if (!query) {
      this.locationSearchResults.set([]);
      this.showLocationResults.set(false);
      return;
    }

    this.showLocationResults.set(true);

    if (this.locationSearchDebounce) {
      clearTimeout(this.locationSearchDebounce);
    }

    this.locationSearchDebounce = setTimeout(() => {
      this.searchLocation(query);
    }, 350);
  }

  searchLocation(query: string): void {
    const trimmed = (query || '').trim();
    if (!trimmed) return;

    this.isSearchingLocation.set(true);
    this.locationService.search(trimmed, 10, undefined, true).subscribe({
      next: (results) => {
        this.locationSearchResults.set(results);
        this.showLocationResults.set(true);
        this.isSearchingLocation.set(false);
      },
      error: () => {
        this.locationSearchResults.set([]);
        this.isSearchingLocation.set(false);
      }
    });
  }

  selectLocationResult(result: LocationSearchResult): void {
    if (!result) return;
    this.hospitalForm.latitude = Number(result.lat);
    this.hospitalForm.longitude = Number(result.lng);
    this.applyAddressFromLocationResult(result);
    this.locationSearchQuery.set(result.displayName);
    this.showLocationResults.set(false);
  }

  clearLocationSearch(): void {
    this.locationSearchQuery.set('');
    this.locationSearchResults.set([]);
    this.showLocationResults.set(false);
    if (this.locationSearchDebounce) {
      clearTimeout(this.locationSearchDebounce);
      this.locationSearchDebounce = null;
    }
  }

  useCurrentLocation(): void {
    const isSecure = (window as any).isSecureContext === true;
    if (!isSecure) {
      this.toastService.error('Current location requires HTTPS or localhost');
      return;
    }

    if (!navigator.geolocation) {
      this.toastService.error('Geolocation not supported in this browser');
      return;
    }

    this.isUsingCurrentLocation.set(true);

    const onSuccess = (pos: GeolocationPosition) => {
      const lat = pos.coords.latitude;
      const lng = pos.coords.longitude;
      this.hospitalForm.latitude = lat;
      this.hospitalForm.longitude = lng;

      this.locationService.reverseGeocode(lat, lng).subscribe({
        next: (result) => {
          if (result) {
            this.applyAddressFromLocationResult(result);
            this.locationSearchQuery.set(result.displayName);
          }
          this.isUsingCurrentLocation.set(false);
        },
        error: () => {
          this.isUsingCurrentLocation.set(false);
        }
      });
    };

    const onError = (err: GeolocationPositionError) => {
      let msg = 'Unable to get your current location';
      if (err?.code === 1) msg = 'Location permission denied. Allow location access in browser settings.';
      if (err?.code === 2) msg = 'Location unavailable. Turn on GPS / Location services.';
      if (err?.code === 3) msg = 'Location request timed out. Try again.';
      this.toastService.error(msg);
      this.isUsingCurrentLocation.set(false);
    };

    const primaryOptions: PositionOptions = { enableHighAccuracy: true, timeout: 15000, maximumAge: 60000 };
    const fallbackOptions: PositionOptions = { enableHighAccuracy: false, timeout: 15000, maximumAge: 60000 };

    navigator.geolocation.getCurrentPosition(
      onSuccess,
      (err) => {
        if (err?.code === 2 || err?.code === 3) {
          navigator.geolocation.getCurrentPosition(onSuccess, onError, fallbackOptions);
          return;
        }
        onError(err);
      },
      primaryOptions
    );
  }

  private applyAddressFromLocationResult(result: LocationSearchResult): void {
    const a = result.address;

    const street = [a?.houseNumber, a?.road]
      .filter(Boolean)
      .map(v => String(v).trim())
      .filter(v => v.length > 0)
      .join(' ');

    const city = a?.city || a?.town || a?.village || '';

    if (street) this.hospitalForm.address = street;
    if (city) this.hospitalForm.city = city;
    if (a?.state) this.hospitalForm.state = a.state;
    if (a?.country) this.hospitalForm.country = a.country;
    if (a?.postcode) this.hospitalForm.postalCode = a.postcode;
  }

  getHospitalMapQuery(): string {
    if (this.hospitalForm.latitude != null && this.hospitalForm.longitude != null) {
      return `${this.hospitalForm.latitude},${this.hospitalForm.longitude}`;
    }

    const parts = [
      this.hospitalForm.name,
      this.hospitalForm.address,
      this.hospitalForm.city,
      this.hospitalForm.state,
      this.hospitalForm.country,
      this.hospitalForm.postalCode
    ]
      .filter(Boolean)
      .map(v => String(v).trim())
      .filter(v => v.length > 0);

    return parts.join(', ');
  }

  getHospitalMapsEmbedUrl(): SafeResourceUrl | null {
    const query = this.getHospitalMapQuery();
    if (!query) return null;
    const url = `https://www.google.com/maps?q=${encodeURIComponent(query)}&output=embed`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }

  getHospitalMapsSearchUrl(): string {
    const query = this.getHospitalMapQuery();
    if (!query) return '';
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
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
      // Create hospital with admin in atomic transaction
      const createData = {
        ...this.hospitalForm,
        adminEmail: this.hospitalAdminForm.email,
        adminDisplayName: this.hospitalAdminForm.displayName,
        adminPassword: this.hospitalAdminForm.password
      };

      this.hospitalService.createHospitalWithAdmin(createData).subscribe({
        next: () => {
          this.toastService.success('Hospital and admin created successfully');
          this.closeHospitalModal();
          this.loadHospitals();
          this.loadAdmins();
          this.loadOverview();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          this.toastService.error(err.message || 'Failed to create hospital with admin');
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

  // View hospital details
  viewHospitalDetails(hospitalId: string): void {
    this.router.navigate(['/superadmin/hospital-details', hospitalId]);
  }

  // Get admins for a specific hospital
  getHospitalAdmins(hospitalId: string): any[] {
    const hospital = this.hospitals().find(h => h.hospitalId === hospitalId);
    if (!hospital || !hospital.hospitalAdmins) return [];
    
    const adminUserIds = hospital.hospitalAdmins.map(ha => ha.userId);
    return this.admins().filter(admin => adminUserIds.includes(admin.userId));
  }

  // Get admin count for a hospital
  getHospitalAdminCount(hospitalId: string): number {
    return this.getHospitalAdmins(hospitalId).length;
  }

  // Edit admin
  editAdmin(admin: Admin): void {
    this.editingAdmin.set(admin);
    this.adminForm = {
      email: admin.email,
      displayName: admin.username,
      password: '', // Don't pre-fill password
      role: 'Admin',
      hospitalId: admin.hospitals?.[0]?.hospitalId || ''
    };
    this.showAdminModal.set(true);
  }

  // Update admin
  updateAdmin(): void {
    const admin = this.editingAdmin();
    if (!admin) return;

    if (!this.adminForm.email || !this.adminForm.displayName) {
      this.toastService.error('Please fill all required fields');
      return;
    }

    if (this.adminForm.password && this.adminForm.password.length < 8) {
      this.toastService.error('Password must be at least 8 characters');
      return;
    }

    this.isSubmitting.set(true);

    const updateData: UpdateAdminDto = {
      username: this.adminForm.displayName,
      email: this.adminForm.email
    };

    if (this.adminForm.password) {
      updateData.password = this.adminForm.password;
    }

    this.hospitalService.updateAdmin(admin.userId, updateData).subscribe({
      next: () => {
        this.toastService.success('Admin updated successfully');
        this.closeAdminModal();
        this.loadAdmins();
        this.isSubmitting.set(false);
      },
      error: (err) => {
        this.toastService.error(err.message || 'Failed to update admin');
        this.isSubmitting.set(false);
      }
    });
  }

  // Delete admin
  deleteAdmin(admin: Admin): void {
    const hospitalNames = admin.hospitals?.map(h => h.hospitalName).join(', ') || 'no hospitals';
    if (!confirm(`Are you sure you want to delete admin "${admin.username}"?\n\nThis admin is currently assigned to: ${hospitalNames}\n\nThis will remove the admin user and all hospital assignments.`)) {
      return;
    }

    this.hospitalService.deleteAdmin(admin.userId).subscribe({
      next: () => {
        this.toastService.success('Admin deleted successfully');
        this.loadAdmins();
        this.loadHospitals();
        this.loadOverview();
      },
      error: (err) => {
        this.toastService.error(err.message || 'Failed to delete admin');
      }
    });
  }

  // Delete patient
  deletePatient(patient: Patient): void {
    if (!confirm(`Are you sure you want to delete patient "${patient.firstName} ${patient.lastName}"?\n\nThis will remove the patient and all their appointments (${patient.appointmentCount} appointments).`)) {
      return;
    }

    this.hospitalService.deletePatient(patient.patientId).subscribe({
      next: () => {
        this.toastService.success('Patient deleted successfully');
        this.loadPatients();
        this.loadOverview();
      },
      error: (err) => {
        this.toastService.error(err.message || 'Failed to delete patient');
      }
    });
  }

  // Delete user
  deleteUser(user: SystemUser): void {
    if (!confirm(`Are you sure you want to delete user "${user.username}" (${user.role})?\n\nThis will remove the user and all associated data.`)) {
      return;
    }

    this.hospitalService.deleteUser(user.userId).subscribe({
      next: () => {
        this.toastService.success('User deleted successfully');
        this.loadUsers();
        this.loadAdmins();
        this.loadPatients();
        this.loadOverview();
      },
      error: (err) => {
        this.toastService.error(err.message || 'Failed to delete user');
      }
    });
  }

  // Logout
  onLogout(): void {
    this.accountService.logout();
    this.router.navigate(['/login']);
  }
}
