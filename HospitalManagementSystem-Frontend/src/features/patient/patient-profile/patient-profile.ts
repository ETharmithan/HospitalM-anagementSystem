import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PatientService } from '../../../core/services/patient-service';
import { AuthService } from '../../../core/services/auth-service';
import { ToastService } from '../../../core/services/toast-service';
import { Nav } from '../../../layout/nav/nav';

@Component({
  selector: 'app-patient-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Nav],
  templateUrl: './patient-profile.html',
  styleUrl: './patient-profile.css',
})
export class PatientProfile implements OnInit {
  private fb = inject(FormBuilder);
  private patientService = inject(PatientService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);

  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  isLoading = signal(false);
  isSaving = signal(false);
  isChangingPassword = signal(false);
  patientId = signal<string | null>(null);
  activeTab = signal<'profile' | 'password'>('profile');

  locationQuery = signal('');

  mapsEmbedUrl = computed<SafeResourceUrl | null>(() => {
    const query = this.locationQuery();
    if (!query) return null;
    const url = `https://www.google.com/maps?q=${encodeURIComponent(query)}&output=embed`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  });

  mapsSearchUrl = computed(() => {
    const query = this.locationQuery();
    if (!query) return '';
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  });

  ngOnInit(): void {
    this.initializeForms();
    this.loadPatientProfile();

    this.profileForm.valueChanges.subscribe(() => {
      this.updateLocationQuery();
    });
  }

  initializeForms(): void {
    this.profileForm = this.fb.group({
      phoneNumber: ['', [Validators.pattern(/^\+?[\d\s-()]+$/)]],
      addressLine1: [''],
      addressLine2: [''],
      city: [''],
      state: [''],
      postalCode: [''],
      country: [''],
      nationality: [''],
      dateOfBirth: [''],
      gender: [''],
      bloodType: [''],
      allergies: [''],
      chronicConditions: [''],
      emergencyContactName: [''],
      emergencyContactEmail: ['', [Validators.email]],
      emergencyContactPhone: ['', [Validators.pattern(/^\+?[\d\s-()]+$/)]],
      emergencyContactRelationship: ['']
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmNewPassword')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  loadPatientProfile(): void {
    this.isLoading.set(true);
    const user = this.authService.currentUser();
    
    if (user?.id) {
      this.patientService.getPatientByUserId(user.id).subscribe({
        next: (patient) => {
          console.log('Patient data received:', patient);
          console.log('ContactInfo:', patient.contactInfo);
          console.log('Nationality:', patient.contactInfo?.nationality);
          console.log('Phone Number:', patient.contactInfo?.phoneNumber);
          this.patientId.set(patient.patientId || null);
          this.populateForm(patient);
          this.isLoading.set(false);
        },
        error: () => {
          this.toastService.error('Failed to load profile');
          this.isLoading.set(false);
        }
      });
    } else {
      this.isLoading.set(false);
    }
  }

  populateForm(patient: any): void {
    this.profileForm.patchValue({
      phoneNumber: patient.contactInfo?.phoneNumber || '',
      addressLine1: patient.contactInfo?.addressLine1 || '',
      addressLine2: patient.contactInfo?.addressLine2 || '',
      city: patient.contactInfo?.city || '',
      state: patient.contactInfo?.state || '',
      postalCode: patient.contactInfo?.postalCode || '',
      country: patient.contactInfo?.country || '',
      nationality: patient.contactInfo?.nationality || '',
      dateOfBirth: patient.dateOfBirth ? new Date(patient.dateOfBirth).toISOString().split('T')[0] : '',
      gender: patient.gender || '',
      bloodType: patient.medicalRelatedInfo?.bloodType || '',
      allergies: patient.medicalRelatedInfo?.allergies || '',
      chronicConditions: patient.medicalRelatedInfo?.chronicConditions || '',
      emergencyContactName: patient.emergencyContact?.contactName || '',
      emergencyContactEmail: patient.emergencyContact?.contactEmail || '',
      emergencyContactPhone: patient.emergencyContact?.contactPhone || '',
      emergencyContactRelationship: patient.emergencyContact?.relationshipToPatient || ''
    });

    this.updateLocationQuery();
  }

  private updateLocationQuery(): void {
    const v = this.profileForm?.value;
    const parts = [v?.addressLine1, v?.addressLine2, v?.city, v?.state, v?.postalCode, v?.country]
      .filter(Boolean)
      .map((x: any) => String(x).trim())
      .filter((x: string) => x.length > 0);
    this.locationQuery.set(parts.join(', '));
  }

  onSaveProfile(): void {
    if (this.profileForm.invalid) {
      this.toastService.warning('Please fill in all required fields correctly');
      return;
    }

    const patientId = this.patientId();
    if (!patientId) {
      this.toastService.error('Patient ID not found');
      return;
    }

    this.isSaving.set(true);
    const formData = this.profileForm.value;

    this.patientService.updatePatientProfile(patientId, formData).subscribe({
      next: (response) => {
        this.toastService.success(response.message || 'Profile updated successfully');
        this.isSaving.set(false);
        // Use the returned patient data to immediately update the form
        if (response.patient) {
          this.populateForm(response.patient);
        } else {
          // Fallback to reloading from server if patient data not returned
          this.loadPatientProfile();
        }
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'Failed to update profile';
        this.toastService.error(errorMessage);
        this.isSaving.set(false);
        console.error('Error updating profile:', error);
      }
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) {
      if (this.passwordForm.hasError('passwordMismatch')) {
        this.toastService.warning('Passwords do not match');
      } else {
        this.toastService.warning('Please fill in all password fields correctly');
      }
      return;
    }

    this.isChangingPassword.set(true);
    const passwordData = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword,
      confirmNewPassword: this.passwordForm.value.confirmNewPassword
    };

    this.authService.changePassword(passwordData).subscribe({
      next: () => {
        this.toastService.success('Password changed successfully');
        this.passwordForm.reset();
        this.isChangingPassword.set(false);
      },
      error: (error: any) => {
        this.toastService.error(error.error?.message || 'Failed to change password');
        this.isChangingPassword.set(false);
      }
    });
  }

  setActiveTab(tab: 'profile' | 'password'): void {
    this.activeTab.set(tab);
  }

  goBack(): void {
    this.router.navigate(['/patient/dashboard']);
  }
}
