import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../../core/services/account-service';
import { HospitalService } from '../../../core/services/hospital-service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contact.html',
  styleUrl: './contact.css'
})
export class Contact implements OnInit {
  private accountService = inject(AccountService);
  private hospitalService = inject(HospitalService);

  currentUser = computed(() => this.accountService.currentUser());
  userRole = computed(() => this.currentUser()?.role || 'Guest');
  
  hospitals = signal<any[]>([]);
  isLoading = signal(true);

  // Superadmin contact details
  superadminContact = {
    name: 'Shadow-Syndicates',
    email: 'admin@shadow-syndicates.com',
    phone: '+1 (555) 123-4567',
    address: '123 Healthcare Plaza, Medical District',
    city: 'Metro City',
    country: 'United States',
    website: 'www.shadow-syndicates.com'
  };

  ngOnInit(): void {
    this.loadHospitals();
  }

  loadHospitals(): void {
    this.hospitalService.getAllHospitals().subscribe({
      next: (data) => {
        this.hospitals.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
