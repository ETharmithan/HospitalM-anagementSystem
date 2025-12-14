import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AppointmentService } from '../../../core/services/appointment-service';
import { DoctorService } from '../../../core/services/doctor-service';
import { PatientService } from '../../../core/services/patient-service';
import { ToastService } from '../../../core/services/toast-service';
import { AccountService } from '../../../core/services/account-service';
import { AvailabilityService, TimeSlot } from '../../../core/services/availability-service';
import { Doctor } from '../../../types/doctor';
import { CalendarPicker } from '../components/calendar-picker/calendar-picker';
import { TimeSlotPicker } from '../components/time-slot-picker/time-slot-picker';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-book-appointment',
  imports: [CommonModule, ReactiveFormsModule, RouterModule, CalendarPicker, TimeSlotPicker, Nav],
  templateUrl: './book-appointment.html',
  styleUrl: './book-appointment.css',
})
export class BookAppointment implements OnInit {
  private route = inject(ActivatedRoute);
  router = inject(Router);
  private fb = inject(FormBuilder);
  private doctorService = inject(DoctorService);
  private appointmentService = inject(AppointmentService);
  private patientService = inject(PatientService);
  private toastService = inject(ToastService);
  private accountService = inject(AccountService);
  private availabilityService = inject(AvailabilityService);

  doctor = signal<Doctor | null>(null);
  appointmentForm!: FormGroup;
  isLoading = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  isLoadingAvailability = signal<boolean>(false);
  patientId = signal<string | null>(null);
  
  // Calendar and availability
  selectedDate = signal<Date | null>(null);
  availableTimeSlots = signal<TimeSlot[]>([]);
  availableDates: Date[] = [];
  fullyBookedDates: Date[] = [];
  unavailableDates: Date[] = [];
  
  minDate: Date = new Date();
  maxDate: Date = new Date();

  ngOnInit() {
    // Set date range (today to 3 months ahead)
    this.minDate = new Date();
    this.minDate.setHours(0, 0, 0, 0);
    this.maxDate = new Date();
    this.maxDate.setMonth(this.maxDate.getMonth() + 3);
    this.maxDate.setHours(23, 59, 59, 999);

    // Initialize form
    this.appointmentForm = this.fb.group({
      appointmentDate: ['', [Validators.required]],
      appointmentTime: ['', [Validators.required]],
      reason: ['', [Validators.required, Validators.minLength(10)]],
    });

    // Get doctor ID from route
    const doctorId = this.route.snapshot.paramMap.get('doctorId');
    if (doctorId) {
      this.loadDoctor(doctorId);
    } else {
      this.toastService.error('Doctor ID is required');
      this.router.navigate(['/doctors']);
    }

    // Get patient ID
    this.loadPatientId();
  }

  loadDoctor(doctorId: string) {
    this.isLoading.set(true);
    this.doctorService.getDoctorById(doctorId).subscribe({
      next: (doctor) => {
        this.doctor.set(doctor);
        this.isLoading.set(false);
        this.loadAvailableDates(doctorId);
      },
      error: (error) => {
        this.toastService.error('Failed to load doctor details');
        this.isLoading.set(false);
        this.router.navigate(['/doctors']);
      },
    });
  }

  loadAvailableDates(doctorId: string) {
    this.availabilityService.getAvailableDates(doctorId, this.minDate, this.maxDate).subscribe({
      next: (response) => {
        // Convert date strings to Date objects
        this.availableDates = response.availableDates.map(d => new Date(d));
        this.fullyBookedDates = response.fullyBookedDates.map(d => new Date(d));
        this.unavailableDates = response.unavailableDates.map(d => new Date(d));
      },
      error: (error) => {
        console.error('Failed to load available dates:', error);
      },
    });
  }

  onDateSelected(date: Date) {
    this.selectedDate.set(date);
    // Format date as YYYY-MM-DD in local timezone (not UTC)
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const localDateString = `${year}-${month}-${day}`;
    
    this.appointmentForm.patchValue({ 
      appointmentDate: localDateString,
      appointmentTime: '' // Reset time when date changes
    });
    
    // Load available time slots for selected date
    if (this.doctor()?.doctorId) {
      this.loadTimeSlots(this.doctor()!.doctorId, date);
    }
  }

  loadTimeSlots(doctorId: string, date: Date) {
    this.isLoadingAvailability.set(true);
    this.availabilityService.getAvailability(doctorId, date).subscribe({
      next: (availability) => {
        this.availableTimeSlots.set(availability.availableSlots);
        this.isLoadingAvailability.set(false);
        
        if (availability.isFullyBooked) {
          this.toastService.warning('This date is fully booked. Please select another date.');
        } else if (!availability.hasSchedule) {
          this.toastService.warning('Doctor has no schedule for this date.');
        } else if (availability.isOnLeave) {
          this.toastService.warning('Doctor is on leave on this date.');
        }
      },
      error: (error) => {
        this.toastService.error('Failed to load available time slots');
        this.isLoadingAvailability.set(false);
        this.availableTimeSlots.set([]);
      },
    });
  }

  onTimeSelected(time: string) {
    this.appointmentForm.patchValue({ appointmentTime: time });
  }

  loadPatientId() {
    const currentUser = this.accountService.currentUser();
    if (currentUser?.id) {
      this.patientService.getPatientByUserId(currentUser.id).subscribe({
        next: (patient) => {
          if (patient.patientId) {
            this.patientId.set(patient.patientId);
          }
        },
        error: () => {
          this.toastService.warning('Please complete your patient registration first');
        },
      });
    }
  }

  onSubmit() {
    if (this.appointmentForm.invalid) {
      this.appointmentForm.markAllAsTouched();
      this.toastService.error('Please fill in all required fields correctly');
      return;
    }

    if (!this.patientId()) {
      this.toastService.error('Patient information not found. Please complete your registration.');
      return;
    }

    if (!this.selectedDate()) {
      this.toastService.error('Please select a date');
      return;
    }

    // Double-check availability before booking
    const formValue = this.appointmentForm.value;
    if (this.doctor()?.doctorId) {
      this.availabilityService.checkSlotAvailability(
        this.doctor()!.doctorId,
        this.selectedDate()!,
        formValue.appointmentTime
      ).subscribe({
        next: (checkResult) => {
          if (!checkResult.available) {
            this.toastService.error('This time slot is no longer available. Please select another time.');
            this.loadTimeSlots(this.doctor()!.doctorId, this.selectedDate()!);
            return;
          }
          this.createAppointment(formValue);
        },
        error: () => {
          this.toastService.error('Failed to verify availability. Please try again.');
        },
      });
    }
  }

  createAppointment(formValue: any) {
    const appointmentData = {
      appointmentDate: formValue.appointmentDate,
      appointmentTime: formValue.appointmentTime,
      appointmentStatus: 'Scheduled',
      createdDate: new Date().toISOString(),
      patientId: this.patientId()!,
      doctorId: this.doctor()!.doctorId,
    };

    this.isSubmitting.set(true);
    this.appointmentService.createAppointment(appointmentData).subscribe({
      next: (appointment) => {
        this.toastService.success('Appointment booked successfully!');
        this.router.navigate(['/my-appointments']);
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to book appointment');
        this.isSubmitting.set(false);
      },
    });
  }

  get appointmentDate() {
    return this.appointmentForm.get('appointmentDate');
  }

  get appointmentTime() {
    return this.appointmentForm.get('appointmentTime');
  }

  get reason() {
    return this.appointmentForm.get('reason');
  }

  getDisabledDates(): Date[] {
    // Combine unavailable dates and fully booked dates
    return [...this.unavailableDates, ...this.fullyBookedDates];
  }
}
