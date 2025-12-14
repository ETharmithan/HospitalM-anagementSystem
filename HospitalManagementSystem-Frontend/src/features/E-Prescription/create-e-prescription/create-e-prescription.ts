import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DoctorScheduleService } from '../../../core/services/doctor-schedule-service';
import { PrescriptionService } from '../../../core/services/prescription-service';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-create-e-prescription',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-e-prescription.html',
  styleUrl: './create-e-prescription.css',
})
export class CreateEPrescription {

  private fb = inject(FormBuilder);
  private scheduleService = inject(DoctorScheduleService);
  private prescriptionService = inject(PrescriptionService);
  private toast = inject(ToastService);

  isLoadingDoctor = signal(true);
  isSubmitting = signal(false);

  private guidPattern = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/;

  form = this.fb.group({
    doctorId: this.fb.control<string>('', { validators: [Validators.required] }),
    patientId: this.fb.control<string>('', { validators: [Validators.required, Validators.pattern(this.guidPattern)] }),
    visitDate: this.fb.control<string>('', { validators: [Validators.required] }),
    diagnosis: this.fb.control<string>('', { validators: [Validators.required] }),
    prescription: this.fb.control<string>('', { validators: [Validators.required] }),
    notes: this.fb.control<string>(''),
  });

  ngOnInit(): void {
    this.loadDoctorId();
  }

  private loadDoctorId(): void {
    this.isLoadingDoctor.set(true);
    this.scheduleService.getMyDoctorId().subscribe({
      next: (result) => {
        this.form.patchValue({ doctorId: result.doctorId });
        this.isLoadingDoctor.set(false);
      },
      error: (error) => {
        console.error('Failed to load doctor id', error);
        this.toast.error('Failed to load doctor details');
        this.isLoadingDoctor.set(false);
      },
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.warning('Please fill all required fields');
      return;
    }

    const raw = this.form.getRawValue();

    if (!raw.doctorId) {
      this.toast.error('Doctor ID is missing. Please reload the page and try again.');
      return;
    }

    this.isSubmitting.set(true);

    this.prescriptionService.createPrescription({
      doctorId: raw.doctorId!,
      patientId: raw.patientId!,
      visitDate: raw.visitDate!,
      diagnosis: raw.diagnosis!,
      prescription: raw.prescription!,
      notes: raw.notes ?? '',
    }).subscribe({
      next: () => {
        this.toast.success('E-Prescription created successfully');
        this.form.reset({
          doctorId: raw.doctorId,
          patientId: '',
          visitDate: '',
          diagnosis: '',
          prescription: '',
          notes: '',
        });
        this.isSubmitting.set(false);
      },
      error: (error) => {
        console.error('Failed to create e-prescription', error);
        const message = error?.error?.message || error?.message || 'Failed to create e-prescription';
        this.toast.error(message);
        this.isSubmitting.set(false);
      },
    });
  }
}
