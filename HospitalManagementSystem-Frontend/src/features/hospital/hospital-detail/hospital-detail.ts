import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
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
  private sanitizer = inject(DomSanitizer);

  hospital = signal<Hospital | null>(null);
  doctors = signal<Doctor[]>([]);
  isLoading = signal(true);
  isLoadingDoctors = signal(false);

  userLocation = signal<{ lat: number; lng: number } | null>(null);
  isGettingDirections = signal(false);

  mapQuery = computed(() => {
    const h = this.hospital();
    if (!h) return '';

     if (h.latitude != null && h.longitude != null) {
      return `${h.latitude},${h.longitude}`;
    }

    const parts = [h.name, h.address, h.city, h.state, h.country, h.postalCode]
      .filter(Boolean)
      .map(v => String(v).trim())
      .filter(v => v.length > 0);
    return parts.join(', ');
  });

  mapsEmbedUrl = computed<SafeResourceUrl | null>(() => {
    const query = this.mapQuery();
    if (!query) return null;
    const url = `https://www.google.com/maps?q=${encodeURIComponent(query)}&output=embed`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  });

  mapsSearchUrl = computed(() => {
    const query = this.mapQuery();
    if (!query) return '';
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  });

  private getDestinationForDirections(): string {
    const h = this.hospital();
    if (!h) return '';
    if (h.latitude != null && h.longitude != null) {
      return `${h.latitude},${h.longitude}`;
    }
    return this.mapQuery();
  }

  private buildDirectionsUrl(origin?: { lat: number; lng: number } | null): string {
    const dest = this.getDestinationForDirections();
    if (!dest) return '';

    const destination = encodeURIComponent(dest);

    if (origin) {
      const originStr = encodeURIComponent(`${origin.lat},${origin.lng}`);
      return `https://www.google.com/maps/dir/?api=1&origin=${originStr}&destination=${destination}&travelmode=driving`;
    }

    return `https://www.google.com/maps/dir/?api=1&destination=${destination}&travelmode=driving`;
  }

  openDirections(): void {
    const existing = this.userLocation();
    const urlNow = this.buildDirectionsUrl(existing);
    if (existing && urlNow) {
      window.open(urlNow, '_blank', 'noopener');
      return;
    }

    if (!navigator.geolocation) {
      const url = this.buildDirectionsUrl(null);
      if (url) window.open(url, '_blank', 'noopener');
      return;
    }

    this.isGettingDirections.set(true);

    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const origin = { lat: pos.coords.latitude, lng: pos.coords.longitude };
        this.userLocation.set(origin);
        const url = this.buildDirectionsUrl(origin);
        if (url) window.open(url, '_blank', 'noopener');
        this.isGettingDirections.set(false);
      },
      () => {
        const url = this.buildDirectionsUrl(null);
        if (url) window.open(url, '_blank', 'noopener');
        this.isGettingDirections.set(false);
      },
      { enableHighAccuracy: true, timeout: 8000, maximumAge: 30000 }
    );
  }

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
