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
import { DoctorScheduleService } from '../../../core/services/doctor-schedule-service';
import { Doctor } from '../../../types/doctor';
import { CalendarPicker } from '../components/calendar-picker/calendar-picker';
import { TimeSlotPicker } from '../components/time-slot-picker/time-slot-picker';
import { Nav } from '../../../layout/nav/nav';
import { forkJoin } from 'rxjs';

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
  private doctorScheduleService = inject(DoctorScheduleService);

  doctor = signal<Doctor | null>(null);
  appointmentForm!: FormGroup;
  isLoading = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  isLoadingAvailability = signal<boolean>(false);
  patientId = signal<string | null>(null);

  hospitalOptions = signal<Array<{ hospitalId: string; name: string }>>([]);
  selectedHospitalId = signal<string | null>(null);
  timeSlotsByHospital = signal<Record<string, TimeSlot[]>>({});
  
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
      hospitalId: [''],
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
        this.loadHospitalsForDoctor(doctorId);
      },
      error: (error) => {
        this.toastService.error('Failed to load doctor details');
        this.isLoading.set(false);
        this.router.navigate(['/doctors']);
      },
    });
  }

  loadTimeSlotsForAllHospitals(doctorId: string, date: Date) {
    const hospitals = this.hospitalOptions();
    if (!hospitals || hospitals.length === 0) {
      this.loadTimeSlots(doctorId, date);
      return;
    }

    // If there is only one hospital, just behave like normal.
    if (hospitals.length === 1) {
      this.loadTimeSlots(doctorId, date, hospitals[0].hospitalId);
      return;
    }

    this.isLoadingAvailability.set(true);
    this.availableTimeSlots.set([]);

    const calls = hospitals.map(h =>
      this.availabilityService.getAvailability(doctorId, date, h.hospitalId)
    );

    forkJoin(calls).subscribe({
      next: (results) => {
        const map: Record<string, TimeSlot[]> = {};
        hospitals.forEach((h, idx) => {
          map[h.hospitalId] = results[idx]?.availableSlots ?? [];
        });
        this.timeSlotsByHospital.set(map);
        this.isLoadingAvailability.set(false);
      },
      error: () => {
        this.toastService.error('Failed to load available time slots');
        this.timeSlotsByHospital.set({});
        this.isLoadingAvailability.set(false);
      }
    });
  }

  loadHospitalsForDoctor(doctorId: string) {
    this.doctorScheduleService.getSchedulesByDoctorId(doctorId).subscribe({
      next: (schedules) => {
        const map = new Map<string, string>();
        (schedules ?? []).forEach((s: any) => {
          const hid = s?.hospitalId;
          if (!hid) return;
          if (!map.has(hid)) {
            map.set(hid, s?.hospitalName || 'Hospital');
          }
        });

        const options = Array.from(map.entries()).map(([hospitalId, name]) => ({ hospitalId, name }));
        this.hospitalOptions.set(options);

        // If doctor has exactly one hospital, auto-select it.
        if (options.length === 1) {
          this.onHospitalSelected(options[0].hospitalId);
          return;
        }

        // If multiple hospitals, allow patient to either pick a hospital OR view grouped slots.
        if (options.length > 1) {
          this.selectedHospitalId.set(null);
          this.appointmentForm.patchValue({ hospitalId: '' });
          this.loadAvailableDates(doctorId);
          return;
        }

        // No hospital-specific schedules; fall back to non-hospital-specific availability.
        this.loadAvailableDates(doctorId);
      },
      error: () => {
        // Fall back to non-hospital-specific availability.
        this.loadAvailableDates(doctorId);
      }
    });
  }

  onHospitalSelected(hospitalId: string) {
    if (!hospitalId) {
      this.selectedHospitalId.set(null);
      this.timeSlotsByHospital.set({});
      this.appointmentForm.patchValue({ hospitalId: '', appointmentDate: '', appointmentTime: '' });
      this.selectedDate.set(null);
      this.availableTimeSlots.set([]);

      const doctorId = this.doctor()?.doctorId;
      if (doctorId) {
        this.loadAvailableDates(doctorId);
      }
      return;
    }

    this.selectedHospitalId.set(hospitalId);
    this.timeSlotsByHospital.set({});
    this.appointmentForm.patchValue({ hospitalId, appointmentDate: '', appointmentTime: '' });
    this.selectedDate.set(null);
    this.availableTimeSlots.set([]);

    const doctorId = this.doctor()?.doctorId;
    if (doctorId) {
      this.loadAvailableDates(doctorId, hospitalId);
    }
  }

  loadAvailableDates(doctorId: string, hospitalId?: string) {
    this.availabilityService.getAvailableDates(doctorId, this.minDate, this.maxDate, hospitalId).subscribe({
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
      const selectedHospitalId = this.selectedHospitalId();
      if (selectedHospitalId) {
        this.loadTimeSlots(this.doctor()!.doctorId, date, selectedHospitalId);
      } else {
        this.loadTimeSlotsForAllHospitals(this.doctor()!.doctorId, date);
      }
    }
  }

  loadTimeSlots(doctorId: string, date: Date, hospitalId?: string) {
    this.isLoadingAvailability.set(true);
    this.availabilityService.getAvailability(doctorId, date, hospitalId).subscribe({
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

  onTimeSelectedForHospital(hospitalId: string, time: string) {
    this.selectedHospitalId.set(hospitalId);
    this.appointmentForm.patchValue({ hospitalId, appointmentTime: time });
  }

  getTimeSlotsForHospital(hospitalId: string): TimeSlot[] {
    return this.timeSlotsByHospital()?.[hospitalId] ?? [];
  }

  getHospitalsWithAvailableSlots(): Array<{ hospitalId: string; name: string }> {
    const slotsMap = this.timeSlotsByHospital();
    return this.hospitalOptions().filter(h => (slotsMap[h.hospitalId] ?? []).some(s => s.available));
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

    // If doctor works in multiple hospitals and user didn't pick a hospital explicitly,
    // they must still have chosen a time from one hospital group (which sets hospitalId).
    if (this.hospitalOptions().length > 1 && !this.selectedHospitalId()) {
      this.toastService.error('Please choose a time slot from a hospital');
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
        formValue.appointmentTime,
        this.selectedHospitalId() || undefined
      ).subscribe({
        next: (checkResult) => {
          if (!checkResult.available) {
            this.toastService.error('This time slot is no longer available. Please select another time.');
            this.loadTimeSlots(this.doctor()!.doctorId, this.selectedDate()!, this.selectedHospitalId() || undefined);
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
      hospitalId: this.selectedHospitalId() || undefined,
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

  get selectedHospitalName(): string {
    const id = this.selectedHospitalId();
    if (!id) return '';
    return this.hospitalOptions().find(o => o.hospitalId === id)?.name || '';
  }

  get reason() {
    return this.appointmentForm.get('reason');
  }

  getDisabledDates(): Date[] {
    // Combine unavailable dates and fully booked dates
    return [...this.unavailableDates, ...this.fullyBookedDates];
  }
}
