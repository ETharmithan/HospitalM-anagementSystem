import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DoctorService } from '../../../core/services/doctor-service';
import { HospitalService, Hospital } from '../../../core/services/hospital-service';
import { ToastService } from '../../../core/services/toast-service';
import { Doctor } from '../../../types/doctor';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-hospital-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, Nav],
  templateUrl: './hospital-detail.html',
  styleUrl: './hospital-detail.css',
})
export class HospitalDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private hospitalService = inject(HospitalService);
  private doctorService = inject(DoctorService);
  private toastService = inject(ToastService);

  hospital = signal<Hospital | null>(null);
  doctors = signal<Doctor[]>([]);
  isLoading = signal(true);
  isLoadingDoctors = signal(false);

  ngOnInit(): void {
    const hospitalId = this.route.snapshot.paramMap.get('id');
    if (hospitalId) {
      this.loadHospitalDetails(hospitalId);
      this.loadHospitalDoctors(hospitalId);
    } else {
      this.toastService.error('Invalid hospital ID');
      this.router.navigate(['/hospitals']);
    }
  }

  loadHospitalDetails(hospitalId: string): void {
    this.hospitalService.getHospitalById(hospitalId).subscribe({
      next: (hospital) => {
        this.hospital.set(hospital);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load hospital details');
        this.isLoading.set(false);
        this.router.navigate(['/hospitals']);
      }
    });
  }

  loadHospitalDoctors(hospitalId: string): void {
    this.isLoadingDoctors.set(true);
    // Get all doctors - in a real scenario, you'd filter based on hospital availability
    this.doctorService.getAllDoctors().subscribe({
      next: (doctors) => {
        // For now, show all active doctors. In a real scenario, you'd filter based on doctor's availability at this hospital
        this.doctors.set(doctors.filter(d => d.status.toLowerCase() === 'active'));
        this.isLoadingDoctors.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load doctors');
        this.isLoadingDoctors.set(false);
      }
    });
  }

  bookAppointment(doctorId: string): void {
    this.router.navigate(['/book-appointment', doctorId]);
  }

  viewDoctorDetails(doctorId: string): void {
    this.router.navigate(['/doctor-detail', doctorId]);
  }

  goBack(): void {
    this.router.navigate(['/hospitals']);
  }
}
