import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DoctorService } from '../../../core/services/doctor-service';
import { HospitalService, Hospital } from '../../../core/services/hospital-service';
import { ToastService } from '../../../core/services/toast-service';
import { Doctor } from '../../../types/doctor';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-doctor-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, Nav],
  templateUrl: './doctor-detail.html',
  styleUrl: './doctor-detail.css',
})
export class DoctorDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private doctorService = inject(DoctorService);
  private hospitalService = inject(HospitalService);
  private toastService = inject(ToastService);

  doctor = signal<Doctor | null>(null);
  hospitals = signal<Hospital[]>([]);
  isLoading = signal(true);
  isLoadingHospitals = signal(false);

  ngOnInit(): void {
    const doctorId = this.route.snapshot.paramMap.get('id');
    if (doctorId) {
      this.loadDoctorDetails(doctorId);
      this.loadDoctorHospitals(doctorId);
    } else {
      this.toastService.error('Invalid doctor ID');
      this.router.navigate(['/doctors']);
    }
  }

  loadDoctorDetails(doctorId: string): void {
    this.doctorService.getDoctorById(doctorId).subscribe({
      next: (doctor) => {
        this.doctor.set(doctor);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load doctor details');
        this.isLoading.set(false);
        this.router.navigate(['/doctors']);
      }
    });
  }

  loadDoctorHospitals(doctorId: string): void {
    this.isLoadingHospitals.set(true);
    // Get all hospitals and filter based on doctor availability
    this.hospitalService.getPublicHospitals().subscribe({
      next: (hospitals) => {
        // For now, show all hospitals. In a real scenario, you'd filter based on doctor's availability/schedule
        this.hospitals.set(hospitals);
        this.isLoadingHospitals.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load hospitals');
        this.isLoadingHospitals.set(false);
      }
    });
  }

  bookAppointment(): void {
    if (this.doctor()) {
      this.router.navigate(['/book-appointment', this.doctor()!.doctorId]);
    }
  }

  getHospitalMapsUrl(hospital: Hospital): string {
    const parts = [hospital.name, hospital.address, hospital.city, hospital.state, hospital.country, hospital.postalCode]
      .filter(Boolean)
      .map(v => String(v).trim())
      .filter(v => v.length > 0);
    const query = parts.join(', ');
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  }

  goBack(): void {
    this.router.navigate(['/doctors']);
  }
}
