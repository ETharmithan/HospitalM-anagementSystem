import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { DoctorScheduleService } from '../../../core/services/doctor-schedule-service';
import { EPrescriptionResponse, EPrescriptionService } from '../../../core/services/e-prescription-service';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-show-e-prescription',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './show-e-prescription.html',
  styleUrl: './show-e-prescription.css',
})
export class ShowEPrescription {

  private accountService = inject(AccountService);
  private scheduleService = inject(DoctorScheduleService);
  private ePrescriptionService = inject(EPrescriptionService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  role = signal<string>(this.accountService.currentUser()?.role ?? '');
  isLoading = signal(true);
  items = signal<EPrescriptionResponse[]>([]);

  // doctor-only edit
  editing = signal<EPrescriptionResponse | null>(null);
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
          this.ePrescriptionService.getByDoctorId(result.doctorId).subscribe({
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
    this.ePrescriptionService.getMyPrescriptions().subscribe({
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
  }

  startEdit(item: EPrescriptionResponse): void {
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

    this.ePrescriptionService.update(current.ePrescriptionId, {
      visitDate: value.visitDate!,
      diagnosis: value.diagnosis!,
      prescription: value.prescription!,
      notes: value.notes ?? '',
    }).subscribe({
      next: (updated) => {
        const nextList = this.items().map(x => x.ePrescriptionId === updated.ePrescriptionId ? updated : x);
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

  delete(item: EPrescriptionResponse): void {
    if (this.role() !== 'Doctor') return;
    const ok = confirm('Delete this prescription?');
    if (!ok) return;

    this.ePrescriptionService.delete(item.ePrescriptionId).subscribe({
      next: () => {
        this.items.set(this.items().filter(x => x.ePrescriptionId !== item.ePrescriptionId));
        this.toast.success('Prescription deleted');
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Failed to delete prescription');
      },
    });
  }

  download(item: EPrescriptionResponse): void {
    if (this.role() !== 'Patient') return;

    this.ePrescriptionService.download(item.ePrescriptionId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `e-prescription-${item.ePrescriptionId}.txt`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Failed to download');
      },
    });
  }
}
