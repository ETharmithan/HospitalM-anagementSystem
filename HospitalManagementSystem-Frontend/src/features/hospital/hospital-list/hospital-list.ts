import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';

interface Hospital {
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
  updatedAt: string;
}

@Component({
  selector: 'app-hospital-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './hospital-list.html',
  styleUrl: './hospital-list.css',
})
export class HospitalList implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private baseUrl = 'http://localhost:5245/api';

  hospitals = signal<Hospital[]>([]);
  filteredHospitals = signal<Hospital[]>([]);
  isLoading = signal(true);
  searchTerm = '';
  selectedCity = '';
  cities = signal<string[]>([]);

  ngOnInit(): void {
    this.loadHospitals();
  }

  loadHospitals(): void {
    this.isLoading.set(true);
    this.http.get<Hospital[]>(`${this.baseUrl}/superadmin/hospitals`).subscribe({
      next: (hospitals) => {
        const activeHospitals = hospitals.filter(h => h.isActive);
        this.hospitals.set(activeHospitals);
        this.filteredHospitals.set(activeHospitals);
        
        // Extract unique cities
        const uniqueCities = [...new Set(activeHospitals.map(h => h.city))].sort();
        this.cities.set(uniqueCities);
        
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Failed to load hospitals:', error);
        this.toastService.error('Failed to load hospitals');
        this.isLoading.set(false);
      }
    });
  }

  filterHospitals(): void {
    let filtered = this.hospitals();

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(h =>
        h.name.toLowerCase().includes(term) ||
        h.address.toLowerCase().includes(term) ||
        h.city.toLowerCase().includes(term)
      );
    }

    if (this.selectedCity) {
      filtered = filtered.filter(h => h.city === this.selectedCity);
    }

    this.filteredHospitals.set(filtered);
  }

  onSearch(): void {
    this.filterHospitals();
  }

  onCityChange(): void {
    this.filterHospitals();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCity = '';
    this.filteredHospitals.set(this.hospitals());
  }

  viewHospitalDoctors(hospitalId: string): void {
    this.router.navigate(['/doctors'], { queryParams: { hospitalId } });
  }

  goBack(): void {
    this.router.navigate(['/home']);
  }
}
