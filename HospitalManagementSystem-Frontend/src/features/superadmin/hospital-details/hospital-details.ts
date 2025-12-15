import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { HospitalService } from '../../../core/services/hospital-service';
import { ToastService } from '../../../core/services/toast-service';
import { Doctor } from '../../../types/doctor';
import { Nav } from '../../../layout/nav/nav';

interface HospitalDetailsData {
  hospitalId: string;
  name: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  latitude?: number | null;
  longitude?: number | null;
  phoneNumber: string;
  email: string;
  website?: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  totalDepartments: number;
  totalDoctors: number;
  totalAdmins: number;
  totalBookings: number;
  upcomingBookings: number;
  completedBookings: number;
  cancelledBookings: number;
  departments: Department[];
  admins: Admin[];
}

interface Department {
  departmentId: string;
  name: string;
  description?: string;
  doctorCount: number;
}

interface Admin {
  userId: string;
  username: string;
  email: string;
  isActive: boolean;
}

@Component({
  selector: 'app-hospital-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Nav],
  templateUrl: './hospital-details.html',
  styleUrl: './hospital-details.css',
})
export class HospitalDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private hospitalService = inject(HospitalService);
  private toastService = inject(ToastService);
  private sanitizer = inject(DomSanitizer);

  hospitalDetails = signal<HospitalDetailsData | null>(null);
  isLoading = signal(true);
  activeTab = signal<'overview' | 'departments' | 'doctors' | 'admins' | 'bookings'>('overview');

  doctors = signal<Doctor[]>([]);
  isLoadingDoctors = signal(false);

  ngOnInit(): void {
    const hospitalId = this.route.snapshot.paramMap.get('id');
    if (hospitalId) {
      this.loadHospitalDetails(hospitalId);
    } else {
      this.router.navigate(['/superadmin/dashboard']);
    }
  }

  loadHospitalDetails(hospitalId: string): void {
    this.isLoading.set(true);
    this.hospitalService.getHospitalDetails(hospitalId).subscribe({
      next: (data: any) => {
        this.hospitalDetails.set(data);
        this.isLoading.set(false);
      },
      error: (error: any) => {
        console.error('Error loading hospital details:', error);
        this.toastService.error('Failed to load hospital details: ' + (error.error?.message || error.message || 'Unknown error'));
        this.isLoading.set(false);
        setTimeout(() => {
          this.router.navigate(['/superadmin/dashboard']);
        }, 2000);
      }
    });
  }

  setActiveTab(tab: 'overview' | 'departments' | 'admins' | 'bookings'): void {
    this.activeTab.set(tab as any);
    if (tab === 'departments') return;
  }

  loadHospitalDoctors(hospitalId: string): void {
    if (this.doctors().length > 0) return;

    this.isLoadingDoctors.set(true);
    this.hospitalService.getHospitalDoctors(hospitalId).subscribe({
      next: (items) => {
        this.doctors.set(items ?? []);
        this.isLoadingDoctors.set(false);
      },
      error: (error: any) => {
        const message = error?.message || error?.error?.message || 'Failed to load doctors';
        this.toastService.error(message);
        this.isLoadingDoctors.set(false);
      }
    });
  }

  setActiveTabExtended(tab: 'overview' | 'departments' | 'doctors' | 'admins' | 'bookings'): void {
    this.activeTab.set(tab);
    if (tab === 'doctors') {
      const id = this.hospitalDetails()?.hospitalId;
      if (id) this.loadHospitalDoctors(id);
    }
  }

  goBack(): void {
    this.router.navigate(['/superadmin/dashboard']);
  }

  getHospitalMapQuery(): string {
    const h = this.hospitalDetails();
    if (!h) return '';

    if (h.latitude != null && h.longitude != null) {
      return `${h.latitude},${h.longitude}`;
    }

    const parts = [h.name, h.address, h.city, h.state, h.country, h.postalCode]
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

  editHospital(): void {
    // Navigate back to dashboard and open edit modal
    this.router.navigate(['/superadmin/dashboard'], { 
      queryParams: { editHospital: this.hospitalDetails()?.hospitalId } 
    });
  }
}
