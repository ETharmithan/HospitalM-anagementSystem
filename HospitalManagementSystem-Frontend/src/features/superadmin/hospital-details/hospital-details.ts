import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Subject, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { HospitalService } from '../../../core/services/hospital-service';
import { LocationSearchResult, LocationService } from '../../../core/services/location-service';
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
export class HospitalDetails implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private hospitalService = inject(HospitalService);
  private locationService = inject(LocationService);
  private toastService = inject(ToastService);
  private sanitizer = inject(DomSanitizer);

  private destroy$ = new Subject<void>();
  private locationSearchInput$ = new Subject<string>();

  hospitalDetails = signal<HospitalDetailsData | null>(null);
  isLoading = signal(true);
  activeTab = signal<'overview' | 'departments' | 'doctors' | 'admins' | 'bookings'>('overview');

  doctors = signal<Doctor[]>([]);
  isLoadingDoctors = signal(false);

  isEditingLocation = signal(false);
  isSavingLocation = signal(false);

  editLatitude = signal<number | null>(null);
  editLongitude = signal<number | null>(null);

  locationSearchQuery = signal<string>('');
  locationSearchResults = signal<LocationSearchResult[]>([]);
  isSearchingLocation = signal(false);
  showLocationResults = signal(false);

  ngOnInit(): void {
    const hospitalId = this.route.snapshot.paramMap.get('id');
    if (hospitalId) {
      this.loadHospitalDetails(hospitalId);
    } else {
      this.router.navigate(['/superadmin/dashboard']);
    }

    this.locationSearchInput$
      .pipe(
        map((value) => (value || '').trim()),
        debounceTime(350),
        distinctUntilChanged(),
        tap((query) => {
          if (query.length < 3) {
            this.isSearchingLocation.set(false);
            this.locationSearchResults.set([]);
            this.showLocationResults.set(false);
          }
        }),
        switchMap((query) => {
          if (query.length < 3) return of([] as LocationSearchResult[]);

          this.isSearchingLocation.set(true);
          this.showLocationResults.set(true);

          return this.locationService.search(query, 10, undefined, true).pipe(
            catchError(() => of([] as LocationSearchResult[])),
            finalize(() => this.isSearchingLocation.set(false))
          );
        }),
        takeUntil(this.destroy$)
      )
      .subscribe((results) => {
        this.locationSearchResults.set(results);
        this.showLocationResults.set(true);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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

  startEditLocation(): void {
    const h = this.hospitalDetails();
    if (!h) return;
    this.isEditingLocation.set(true);
    this.editLatitude.set(h.latitude ?? null);
    this.editLongitude.set(h.longitude ?? null);
    this.locationSearchQuery.set('');
    this.locationSearchResults.set([]);
    this.showLocationResults.set(false);
  }

  cancelEditLocation(): void {
    this.isEditingLocation.set(false);
    this.locationSearchQuery.set('');
    this.locationSearchResults.set([]);
    this.showLocationResults.set(false);
  }

  onLocationSearchInput(value: string): void {
    this.locationSearchQuery.set(value);
    this.locationSearchInput$.next(value);
  }

  selectLocationResult(result: LocationSearchResult): void {
    if (!result) return;
    this.editLatitude.set(Number(result.lat));
    this.editLongitude.set(Number(result.lng));
    this.locationSearchQuery.set(result.displayName);
    this.showLocationResults.set(false);
  }

  clearLocationSearch(): void {
    this.locationSearchQuery.set('');
    this.locationSearchResults.set([]);
    this.showLocationResults.set(false);
    this.isSearchingLocation.set(false);
    this.locationSearchInput$.next('');
  }

  saveLocation(): void {
    const h = this.hospitalDetails();
    if (!h) return;

    const hospitalId = h.hospitalId;
    const latitude = this.editLatitude();
    const longitude = this.editLongitude();

    if ((latitude == null) !== (longitude == null)) {
      this.toastService.error('Please provide both Latitude and Longitude');
      return;
    }

    this.isSavingLocation.set(true);

    this.hospitalService.updateHospital(hospitalId, {
      name: h.name,
      address: h.address,
      city: h.city,
      state: h.state,
      country: h.country,
      postalCode: h.postalCode,
      latitude: latitude ?? null,
      longitude: longitude ?? null,
      phoneNumber: h.phoneNumber,
      email: h.email,
      website: h.website,
      description: h.description,
    }).subscribe({
      next: () => {
        this.toastService.success('Location updated');
        this.isSavingLocation.set(false);
        this.isEditingLocation.set(false);
        this.loadHospitalDetails(hospitalId);
      },
      error: (err: any) => {
        const message = err?.message || err?.error?.message || 'Failed to update location';
        this.toastService.error(message);
        this.isSavingLocation.set(false);
      }
    });
  }

  clearPin(): void {
    this.editLatitude.set(null);
    this.editLongitude.set(null);
    this.saveLocation();
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
