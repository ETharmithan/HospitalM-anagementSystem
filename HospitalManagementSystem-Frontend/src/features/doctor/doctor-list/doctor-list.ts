import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { DoctorService } from '../../../core/services/doctor-service';
import { ToastService } from '../../../core/services/toast-service';
import { Doctor, Department } from '../../../types/doctor';

@Component({
  selector: 'app-doctor-list',
  imports: [CommonModule, RouterModule],
  templateUrl: './doctor-list.html',
  styleUrl: './doctor-list.css',
})
export class DoctorList implements OnInit {
  private doctorService = inject(DoctorService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  doctors = signal<Doctor[]>([]);
  departments = signal<Department[]>([]);
  filteredDoctors = signal<Doctor[]>([]);
  selectedDepartment = signal<string>('all');
  searchQuery = signal<string>('');
  isLoading = signal<boolean>(false);

  ngOnInit() {
    this.loadDoctors();
    this.loadDepartments();
  }

  loadDoctors() {
    this.isLoading.set(true);
    this.doctorService.getAllDoctors().subscribe({
      next: (doctors) => {
        this.doctors.set(doctors);
        this.filteredDoctors.set(doctors);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.toastService.error('Failed to load doctors. Please try again.');
        this.isLoading.set(false);
        console.error(error);
      },
    });
  }

  loadDepartments() {
    this.doctorService.getAllDepartments().subscribe({
      next: (departments) => {
        this.departments.set(departments);
      },
      error: (error) => {
        console.error('Failed to load departments:', error);
      },
    });
  }

  onDepartmentChange(departmentId: string) {
    this.selectedDepartment.set(departmentId);
    this.applyFilters();
  }

  onSearchChange(query: string) {
    this.searchQuery.set(query);
    this.applyFilters();
  }

  applyFilters() {
    let filtered = [...this.doctors()];

    // Filter by department
    if (this.selectedDepartment() !== 'all') {
      filtered = filtered.filter(
        (doctor) => doctor.departmentId === this.selectedDepartment()
      );
    }

    // Filter by search query
    if (this.searchQuery().trim()) {
      const query = this.searchQuery().toLowerCase();
      filtered = filtered.filter(
        (doctor) =>
          doctor.name.toLowerCase().includes(query) ||
          doctor.qualification.toLowerCase().includes(query) ||
          doctor.departmentName?.toLowerCase().includes(query) ||
          doctor.email.toLowerCase().includes(query)
      );
    }

    this.filteredDoctors.set(filtered);
  }

  bookAppointment(doctorId: string) {
    this.router.navigate(['/book-appointment', doctorId]);
  }

  viewDoctorDetails(doctorId: string) {
    this.router.navigate(['/doctor', doctorId]);
  }
}

