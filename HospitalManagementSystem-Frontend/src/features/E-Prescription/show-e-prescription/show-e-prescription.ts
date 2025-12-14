import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { DoctorScheduleService } from '../../../core/services/doctor-schedule-service';
import { PatientService } from '../../../core/services/patient-service';
import { PrescriptionService, PrescriptionResponse } from '../../../core/services/prescription-service';
import { ToastService } from '../../../core/services/toast-service';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-show-e-prescription',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './show-e-prescription.html',
  styleUrl: './show-e-prescription.css',
})
export class ShowEPrescription {

  private router = inject(Router);
  private accountService = inject(AccountService);
  private scheduleService = inject(DoctorScheduleService);
  private patientService = inject(PatientService);
  private prescriptionService = inject(PrescriptionService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  role = signal<string>(this.accountService.currentUser()?.role ?? '');
  isLoading = signal(true);
  items = signal<PrescriptionResponse[]>([]);

  // doctor-only edit
  editing = signal<PrescriptionResponse | null>(null);
  isSaving = signal(false);
  editForm = this.fb.group({
    visitDate: this.fb.control<string>('', { validators: [Validators.required] }),
    diagnosis: this.fb.control<string>('', { validators: [Validators.required] }),
    prescription: this.fb.control<string>('', { validators: [Validators.required] }),
    notes: this.fb.control<string>(''),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);

    if (this.role() === 'Doctor') {
      this.scheduleService.getMyDoctorId().subscribe({
        next: (result) => {
          this.prescriptionService.getPrescriptionsByDoctorId(result.doctorId).subscribe({
            next: (list) => {
              this.items.set(list);
              this.isLoading.set(false);
            },
            error: (err) => {
              console.error(err);
              this.toast.error('Failed to load prescriptions');
              this.isLoading.set(false);
            },
          });
        },
        error: (err) => {
          console.error(err);
          this.toast.error('Failed to load doctor details');
          this.isLoading.set(false);
        },
      });
      return;
    }

    // Patient
    const user = this.accountService.currentUser();
    if (!user?.id) {
      this.toast.error('User not found');
      this.isLoading.set(false);
      return;
    }
    
    // Get patient record to get correct patientId
    console.log('Loading prescriptions for user:', user.id);
    this.patientService.getPatientByUserId(user.id).subscribe({
      next: (patient) => {
        console.log('Patient data retrieved:', patient);
        if (!patient.patientId) {
          console.error('No patientId found for user:', user.id);
          this.toast.error('Patient record not found');
          this.isLoading.set(false);
          return;
        }
        
        console.log('Fetching prescriptions for patientId:', patient.patientId);
        this.prescriptionService.getPrescriptionsByPatientId(patient.patientId).subscribe({
          next: (list) => {
            console.log('Prescriptions received:', list);
            this.items.set(list);
            this.isLoading.set(false);
          },
          error: (err) => {
            console.error('Error loading prescriptions:', err);
            this.toast.error('Failed to load prescriptions');
            this.isLoading.set(false);
          },
        });
      },
      error: (err) => {
        console.error('Error loading patient data:', err);
        this.toast.error('Failed to load patient data');
        this.isLoading.set(false);
      },
    });
  }

  startEdit(item: PrescriptionResponse): void {
    if (this.role() !== 'Doctor') return;
    this.editing.set(item);

    const dateOnly = item.visitDate ? String(item.visitDate).slice(0, 10) : '';
    this.editForm.setValue({
      visitDate: dateOnly,
      diagnosis: item.diagnosis,
      prescription: item.prescription,
      notes: item.notes ?? '',
    });
  }

  cancelEdit(): void {
    this.editing.set(null);
  }

  saveEdit(): void {
    const current = this.editing();
    if (!current) return;
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.toast.warning('Please fill all required fields');
      return;
    }

    const value = this.editForm.getRawValue();
    this.isSaving.set(true);

    this.prescriptionService.updatePrescription(current.recordId, {
      visitDate: value.visitDate!,
      diagnosis: value.diagnosis!,
      prescription: value.prescription!,
      notes: value.notes ?? '',
      doctorId: current.doctorId,
      patientId: current.patientId
    }).subscribe({
      next: (updated) => {
        const nextList = this.items().map(x => x.recordId === updated.recordId ? updated : x);
        this.items.set(nextList);
        this.toast.success('Prescription updated');
        this.isSaving.set(false);
        this.editing.set(null);
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Failed to update prescription');
        this.isSaving.set(false);
      },
    });
  }

  delete(item: PrescriptionResponse): void {
    if (this.role() !== 'Doctor') return;
    const ok = confirm('Delete this prescription?');
    if (!ok) return;

    this.prescriptionService.deletePrescription(item.recordId).subscribe({
      next: () => {
        this.items.set(this.items().filter(x => x.recordId !== item.recordId));
        this.toast.success('Prescription deleted');
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Failed to delete prescription');
      },
    });
  }

  download(item: PrescriptionResponse): void {
    // Download functionality removed - prescriptions are view-only
    this.toast.info('Prescription details are displayed above');
  }

  goBack(): void {
    const role = this.role();
    if (role === 'Patient') {
      this.router.navigate(['/patient/dashboard']);
    } else if (role === 'Doctor') {
      this.router.navigate(['/doctor/dashboard']);
    } else {
      this.router.navigate(['/home']);
    }
  }
}
