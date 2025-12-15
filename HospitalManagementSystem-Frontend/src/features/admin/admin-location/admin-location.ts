import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminDashboardService, AdminHospitalLocation, UpdateMyHospitalRequest } from '../../../core/services/admin-dashboard-service';
import { LocationSearchResult, LocationService } from '../../../core/services/location-service';
import { ToastService } from '../../../core/services/toast-service';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-admin-location',
  standalone: true,
  imports: [CommonModule, FormsModule, Nav],
  templateUrl: './admin-location.html',
  styleUrl: './admin-location.css',
})
export class AdminLocation implements OnInit {
  private adminDashboardService = inject(AdminDashboardService);
  private locationService = inject(LocationService);
  private toastService = inject(ToastService);

  isLoading = signal(true);
  isSaving = signal(false);
  hospital = signal<AdminHospitalLocation | null>(null);

  // Location search
  locationSearchQuery = signal<string>('');
  locationSearchResults = signal<LocationSearchResult[]>([]);
  isSearchingLocation = signal(false);
  showLocationResults = signal(false);
  private locationSearchDebounce: any = null;

  form: UpdateMyHospitalRequest = {
    name: '',
    address: '',
    city: '',
    state: '',
    country: '',
    postalCode: '',
    phoneNumber: '',
    email: '',
    website: '',
    description: '',
    latitude: null,
    longitude: null,
  };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.adminDashboardService.getMyHospital().subscribe({
      next: (data) => {
        this.hospital.set(data);
        this.form = {
          name: data.name,
          address: data.address,
          city: data.city,
          state: data.state,
          country: data.country,
          postalCode: data.postalCode,
          phoneNumber: data.phoneNumber,
          email: data.email,
          website: data.website || '',
          description: data.description || '',
          latitude: data.latitude ?? null,
          longitude: data.longitude ?? null,
        };
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.error(err?.message || 'Failed to load hospital location');
        this.isLoading.set(false);
      },
    });
  }

  save(): void {
    if (!this.form.name || !this.form.address || !this.form.city || !this.form.state || !this.form.country || !this.form.postalCode || !this.form.phoneNumber || !this.form.email) {
      this.toastService.error('Please fill all required fields');
      return;
    }

    this.isSaving.set(true);
    this.adminDashboardService.updateMyHospital(this.form).subscribe({
      next: (updated) => {
        this.hospital.set(updated);
        this.toastService.success('Hospital location updated');
        this.isSaving.set(false);
      },
      error: (err) => {
        this.toastService.error(err?.message || 'Failed to update hospital location');
        this.isSaving.set(false);
      },
    });
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
    this.locationService.search(trimmed, 8, 'lk', false).subscribe({
      next: (results) => {
        this.locationSearchResults.set(results);
        this.showLocationResults.set(true);
        this.isSearchingLocation.set(false);
      },
      error: () => {
        this.locationSearchResults.set([]);
        this.isSearchingLocation.set(false);
      },
    });
  }

  selectLocationResult(result: LocationSearchResult): void {
    if (!result) return;
    this.form.latitude = Number(result.lat);
    this.form.longitude = Number(result.lng);

    const a = result.address;
    const street = [a?.houseNumber, a?.road]
      .filter(Boolean)
      .map((v) => String(v).trim())
      .filter((v) => v.length > 0)
      .join(' ');

    const city = a?.city || a?.town || a?.village || '';

    if (street) this.form.address = street;
    if (city) this.form.city = city;
    if (a?.state) this.form.state = a.state;
    if (a?.country) this.form.country = a.country;
    if (a?.postcode) this.form.postalCode = a.postcode;

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

  getMapsSearchUrl(): string {
    const parts = [
      this.form.name,
      this.form.address,
      this.form.city,
      this.form.state,
      this.form.country,
      this.form.postalCode,
    ]
      .filter(Boolean)
      .map((v) => String(v).trim())
      .filter((v) => v.length > 0);

    const query = parts.join(', ');
    if (!query) return '';
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  }
}
