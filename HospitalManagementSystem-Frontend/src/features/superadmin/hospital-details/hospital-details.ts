import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HospitalService } from '../../../core/services/hospital-service';
import { ToastService } from '../../../core/services/toast-service';

interface HospitalDetailsData {
  hospitalId: string;
  name: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
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
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './hospital-details.html',
  styleUrl: './hospital-details.css',
})
export class HospitalDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private hospitalService = inject(HospitalService);
  private toastService = inject(ToastService);

  hospitalDetails = signal<HospitalDetailsData | null>(null);
  isLoading = signal(true);
  activeTab = signal<'overview' | 'departments' | 'admins' | 'bookings'>('overview');

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
    this.activeTab.set(tab);
  }

  goBack(): void {
    this.router.navigate(['/superadmin/dashboard']);
  }

  editHospital(): void {
    // Navigate back to dashboard and open edit modal
    this.router.navigate(['/superadmin/dashboard'], { 
      queryParams: { editHospital: this.hospitalDetails()?.hospitalId } 
    });
  }
}
