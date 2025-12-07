import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '../core/services/toast-service';
import { PatientService } from '../core/services/patient-service';
import { AuthService } from '../core/services/auth.service';
import { AccountService } from '../core/services/account-service';

@Component({
  selector: 'app-patient-register',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-register.html',
  styleUrl: './patient-register.css',
})
export class PatientRegister implements OnInit {
  private fb = inject(FormBuilder);
  private patientService = inject(PatientService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private authService = inject(AuthService);
  private accountService = inject(AccountService);

  patientForm!: FormGroup;
  isLoading = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploadMethod: 'file' | 'url' = 'file';
  registeredPatient: any = null;
  showPatientDetails = false;
  showAdditionalDetails = false;
  additionalDetailsForm!: FormGroup;

  srilankanProvinces = [
    'Central',
    'Eastern',
    'North Central',
    'Northern',
    'North Western',
    'Sabaragamuwa',
    'Southern',
    'Uva',
    'Western',
    'Other'
  ];

  ngOnInit(): void {
    this.initializeForm();
    this.initializeAdditionalDetailsForm();
    
    // Listen to imageUrl changes to update preview
    this.patientForm.get('imageUrl')?.valueChanges.subscribe(() => {
      // Trigger change detection for preview
    });
  }

  initializeForm(): void {
    this.patientForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required],
      imageUrl: [''],
      // Authentication
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)]],
      confirmPassword: ['', Validators.required],
      // Identification
      nic: ['', [Validators.required, Validators.minLength(9), Validators.maxLength(12)]],
      // Contact Information
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      emailAddress: ['', [Validators.required, Validators.email]],
      addressLine1: ['', Validators.required],
      addressLine2: [''],
      city: ['', Validators.required],
      province: ['', Validators.required],
      postalCode: ['', Validators.required],
      country: ['', Validators.required],
      nationality: ['', Validators.required],
    }, { validators: this.passwordMatchValidator });
  }

  // Password match validator
  passwordMatchValidator(form: FormGroup): { [key: string]: boolean } | null {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (!password || !confirmPassword) {
      return null;
    }
    
    if (password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    // Clear password mismatch error if passwords match
    if (confirmPassword.hasError('passwordMismatch')) {
      confirmPassword.setErrors(null);
    }
    
    return null;
  }

  initializeAdditionalDetailsForm(): void {
    this.additionalDetailsForm = this.fb.group({
      // Emergency Contact
      emergencyContactName: ['', Validators.required],
      emergencyContactEmail: ['', [Validators.required, Validators.email]],
      emergencyContactPhone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      emergencyContactRelationship: ['', Validators.required],
      
      // Medical History
      pastIllnesses: [''],
      surgeries: [''],
      medicalHistoryNotes: [''],
      
      // Medical Related Info
      bloodType: ['', Validators.required],
      allergies: [''],
      chronicConditions: ['']
    });
  }

  registerPatient(): void {
    if (this.patientForm.invalid) {
      this.toastService.error('Please fill all required fields correctly');
      return;
    }

    this.isLoading = true;
    const formValue = this.patientForm.value;

    // Handle image upload if file is selected
    if (this.uploadMethod === 'file' && this.selectedFile) {
      this.patientService.uploadImage(this.selectedFile).subscribe({
        next: (response) => {
          this.submitPatientData(formValue, response.imageUrl);
        },
        error: (error) => {
          this.isLoading = false;
          const errorMessage = error?.error?.message || 'Failed to upload image';
          this.toastService.error(errorMessage);
          console.error('Upload error:', error);
        },
      });
    } else {
      // Use URL directly or no image
      this.submitPatientData(formValue, formValue.imageUrl || '');
    }
  }

  private submitPatientData(formValue: any, imageUrl: string): void {
    // Prepare patient data
    const patientData: any = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      dateOfBirth: formValue.dateOfBirth,
      gender: formValue.gender,
      imageUrl: imageUrl,
      nic: formValue.nic,
      phoneNumber: formValue.phoneNumber,
      emailAddress: formValue.emailAddress,
      addressLine1: formValue.addressLine1,
      addressLine2: formValue.addressLine2,
      city: formValue.city,
      province: formValue.province,
      postalCode: formValue.postalCode,
      country: formValue.country,
      nationality: formValue.nationality,
    };

    // First register user account, then create patient
    this.registerUserAccount(formValue, null, imageUrl, (userResponse: any) => {
      // Associate patient with the created user account
      patientData.userId = userResponse.id;
      // Also try with capital U for C# convention
      patientData.UserId = userResponse.id;
      
      // Only create patient after successful user account creation
      this.patientService.createPatient(patientData).subscribe({
        next: (patientResponse: any) => {
          // Show patient details after both user account and patient creation succeed
          this.registeredPatient = {
            ...patientResponse,
            // Add form data for complete details
            nic: formValue.nic,
            phoneNumber: formValue.phoneNumber,
            emailAddress: formValue.emailAddress,
            addressLine1: formValue.addressLine1,
            addressLine2: formValue.addressLine2,
            city: formValue.city,
            province: formValue.province,
            postalCode: formValue.postalCode,
            country: formValue.country,
            nationality: formValue.nationality,
            imageUrl: imageUrl
          };
          
          this.showPatientDetails = true;
          this.toastService.success('Patient registered successfully!');
          this.patientForm.reset();
          this.selectedFile = null;
          this.imagePreview = null;
          this.isLoading = false;
        },
        error: (error: any) => {
          this.isLoading = false;
          const errorMessage = error?.error?.message || error?.error?.title || 'Failed to create patient record';
          this.toastService.error(errorMessage);
          console.error('Patient creation error:', error);
        },
      });
    });
  }

  private registerUserAccount(formValue: any, patientResponse: any, imageUrl: string, onSuccess: (userResponse: any) => void): void {
    // Prepare user account data
    const registerData = {
      email: formValue.emailAddress,
      displayName: `${formValue.firstName} ${formValue.lastName}`,
      password: formValue.password,
      role: 'Patient', // Backend expects 'Role' not 'userType'
      imageUrl: imageUrl // Include the image URL
    };

        
    // Register user account
    this.accountService.register(registerData).subscribe({
      next: (response: any) => {
        // Don't set isLoading = false here - patient creation will handle it
        
        // Check if email verification is required
        if (response.requiresVerification) {
          this.isLoading = false;
          this.toastService.success(response.message || 'Registration successful! Please verify your email.');
          
          // Navigate to verify-email page
          this.router.navigate(['/verify-email'], { 
            queryParams: { email: registerData.email },
            state: { email: registerData.email }
          });
          return;
        }
        
        this.toastService.success('User account created successfully!');
        
        // Call success callback with user response to continue with patient creation
        const userResponse = response.user || response;
        onSuccess(userResponse);
      },
      error: (error: any) => {
        this.isLoading = false;
        let errorMessage = 'Failed to create user account';
        
        // Handle specific error cases
        if (error?.error?.message) {
          errorMessage = error.error.message;
          // Make email already taken error more user-friendly
          if (errorMessage.toLowerCase().includes('email is already taken')) {
            errorMessage = 'This email address is already registered. Please use a different email or try logging in.';
          }
        } else if (error?.error?.title) {
          errorMessage = error.error.title;
        } else if (error?.status === 400 && error?.error?.errors) {
          // Handle validation errors
          const errorMessages = Object.values(error.error.errors).flat();
          errorMessage = errorMessages.join(', ') || 'Validation failed';
        }
        
        this.toastService.error(errorMessage);
        console.error('Full error response:', JSON.stringify(error, null, 2));
      }
    });
  }

  private generateGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  get firstName() {
    return this.patientForm.get('firstName');
  }

  get lastName() {
    return this.patientForm.get('lastName');
  }

  get dateOfBirth() {
    return this.patientForm.get('dateOfBirth');
  }

  get gender() {
    return this.patientForm.get('gender');
  }

  get imageUrl() {
    return this.patientForm.get('imageUrl');
  }

  get nic() {
    return this.patientForm.get('nic');
  }

  get phoneNumber() {
    return this.patientForm.get('phoneNumber');
  }

  get emailAddress() {
    return this.patientForm.get('emailAddress');
  }

  get addressLine1() {
    return this.patientForm.get('addressLine1');
  }

  get city() {
    return this.patientForm.get('city');
  }

  get province() {
    return this.patientForm.get('province');
  }

  get postalCode() {
    return this.patientForm.get('postalCode');
  }

  get country() {
    return this.patientForm.get('country');
  }

  get nationality() {
    return this.patientForm.get('nationality');
  }

  get password() {
    return this.patientForm.get('password');
  }

  get confirmPassword() {
    return this.patientForm.get('confirmPassword');
  }

  // Password validation helper methods
  hasUppercase(): boolean {
    const password = this.password?.value;
    return password ? /[A-Z]/.test(password) : false;
  }

  hasLowercase(): boolean {
    const password = this.password?.value;
    return password ? /[a-z]/.test(password) : false;
  }

  hasNumber(): boolean {
    const password = this.password?.value;
    return password ? /\d/.test(password) : false;
  }

  hasSpecialChar(): boolean {
    const password = this.password?.value;
    return password ? /[@$!%*?&]/.test(password) : false;
  }

  hasMinLength(): boolean {
    const password = this.password?.value;
    return password ? password.length >= 8 : false;
  }

  passwordsMatch(): boolean {
    const password = this.password?.value;
    const confirmPassword = this.confirmPassword?.value;
    return password && confirmPassword ? password === confirmPassword : false;
  }

  
  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = input.files;

    if (files && files.length > 0) {
      const file = files[0];

      // Validate file type
      const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
      if (!validTypes.includes(file.type)) {
        this.toastService.error('Only image files (jpg, png, gif, webp) are allowed');
        return;
      }

      // Validate file size (5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.toastService.error('File size must be less than 5MB');
        return;
      }

      this.selectedFile = file;

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result;
        this.imagePreview = typeof result === 'string' ? result : null;
        // Trigger change detection to update the view
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    }
  }

  switchUploadMethod(method: 'file' | 'url'): void {
    this.uploadMethod = method;
    this.selectedFile = null;
    this.imagePreview = null;
    if (method === 'url') {
      this.patientForm.patchValue({ imageUrl: '' });
    }
    this.cdr.detectChanges();
  }

  removeImage(): void {
    this.selectedFile = null;
    this.imagePreview = null;
    this.patientForm.patchValue({ imageUrl: '' });
    this.cdr.detectChanges();
  }

  hidePatientDetails(): void {
    this.registeredPatient = null;
    this.showPatientDetails = false;
  }

  onImageError(event: any): void {
    event.target.src = '/assets/avatar-placeholder.svg';
  }

  getImageUrl(imageUrl: string): string {
    if (!imageUrl) return '/assets/avatar-placeholder.svg';
    
    // If it's already a full URL, return as is
    if (imageUrl.startsWith('http')) {
      return imageUrl;
    }
    
    // If it starts with /uploads, it's already correct
    if (imageUrl.startsWith('/uploads')) {
      return `http://localhost:5245${imageUrl}`;
    }
    
    // If it starts with uploads/, add the leading slash
    if (imageUrl.startsWith('uploads/')) {
      return `http://localhost:5245/${imageUrl}`;
    }
    
    // Default: assume it's a relative path
    return `http://localhost:5245/${imageUrl}`;
  }

  showAdditionalDetailsForm(): void {
    this.showPatientDetails = false;
    this.showAdditionalDetails = true;
  }

  submitAdditionalDetails(): void {
    // Save additional details to backend
    this.saveAdditionalDetailsToBackend();
  }

  skipAdditionalDetails(): void {
    this.showAdditionalDetails = false;
    this.redirectToHome();
  }

  saveAdditionalDetailsToBackend(): void {
    const formValue = this.additionalDetailsForm.value;
    const additionalDetails = {
      patientId: this.registeredPatient.patientId,
      emergencyContact: {
        contactName: formValue.emergencyContactName,
        contactEmail: formValue.emergencyContactEmail,
        contactPhone: formValue.emergencyContactPhone,
        relationshipToPatient: formValue.emergencyContactRelationship
      },
      medicalHistory: {
        pastIllnesses: formValue.pastIllnesses || '',
        surgeries: formValue.surgeries || '',
        medicalHistoryNotes: formValue.medicalHistoryNotes || ''
      },
      medicalInfo: {
        bloodType: formValue.bloodType,
        allergies: formValue.allergies || '',
        chronicConditions: formValue.chronicConditions || ''
      }
    };

    this.patientService.saveAdditionalDetails(additionalDetails).subscribe({
      next: () => {
        this.showAdditionalDetails = false;
        this.toastService.success('Additional patient details saved successfully!');
        this.redirectToHome();
      },
      error: (error) => {
        console.error('Error saving additional details:', error);
        this.toastService.error('Failed to save additional details. You can update them later.');
        this.redirectToHome();
      }
    });
  }

  redirectToHome(): void {
    // Clear state
    this.registeredPatient = null;
    this.showPatientDetails = false;
    this.showAdditionalDetails = false;
    this.patientForm.reset();
    this.additionalDetailsForm.reset();
    
    this.toastService.success('Registration complete! Please login with your new account.');
    
    // Navigate to login page
    this.router.navigate(['/login']);
  }
}
