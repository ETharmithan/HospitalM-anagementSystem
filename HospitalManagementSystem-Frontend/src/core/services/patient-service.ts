import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError, timeout } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface PatientData {
  patientId?: string;
  userId?: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  imageUrl?: string;
  nic?: string;
  phoneNumber?: string;
  emailAddress?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  province?: string;
  postalCode?: string;
  country?: string;
  nationality?: string;
  contactInfo?: {
    patientId?: string;
    phoneNumber?: string;
    emailAddress?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    state?: string;
    postalCode?: string;
    country?: string;
    nationality?: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class PatientService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5245/api/patient';

  // Create a new patient
  createPatient(patientData: PatientData): Observable<PatientData> {
    return this.http.post<PatientData>(this.apiUrl, patientData).pipe(
      timeout(10000), // 10 second timeout
      catchError(error => {
        if (error.name === 'TimeoutError') {
          return throwError(() => new Error('Registration timed out. Please try again.'));
        }
        return throwError(() => error);
      })
    );
  }

  // Get all patients
  getAllPatients(): Observable<PatientData[]> {
    return this.http.get<PatientData[]>(this.apiUrl);
  }

  // Get patient by ID
  getPatientById(patientId: string): Observable<PatientData> {
    return this.http.get<PatientData>(`${this.apiUrl}/${patientId}`);
  }

  // Get patient by user ID
  getPatientByUserId(userId: string): Observable<PatientData> {
    return this.http.get<PatientData>(`${this.apiUrl}/user/${userId}`);
  }

  // Get patients by gender
  getPatientsByGender(gender: string): Observable<PatientData[]> {
    return this.http.get<PatientData[]>(`${this.apiUrl}/gender/${gender}`);
  }

  // Update patient
  updatePatient(patientId: string, patientData: PatientData): Observable<PatientData> {
    return this.http.put<PatientData>(`${this.apiUrl}/${patientId}`, patientData);
  }

  // Delete patient
  deletePatient(patientId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${patientId}`);
  }

  // Upload patient image
  uploadImage(file: File): Observable<{ message: string; imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ message: string; imageUrl: string }>(`${this.apiUrl}/upload-image`, formData).pipe(
      timeout(10000), // 10 second timeout
      catchError(error => {
        if (error.name === 'TimeoutError') {
          return throwError(() => new Error('Image upload timed out. Please try again.'));
        }
        return throwError(() => error);
      })
    );
  }

  // Save additional patient details
  saveAdditionalDetails(additionalDetails: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/additional-details`, additionalDetails).pipe(
      timeout(10000), // 10 second timeout
      catchError(error => {
        if (error.name === 'TimeoutError') {
          return throwError(() => new Error('Save additional details timed out. Please try again.'));
        }
        return throwError(() => error);
      })
    );
  }

  // Update patient profile
  updatePatientProfile(patientId: string, profileData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${patientId}/profile`, profileData).pipe(
      timeout(10000),
      catchError(error => {
        if (error.name === 'TimeoutError') {
          return throwError(() => new Error('Update profile timed out. Please try again.'));
        }
        return throwError(() => error);
      })
    );
  }

  // Skip additional info
  skipAdditionalInfo(patientId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${patientId}/skip-additional-info`, {});
  }

  // Get additional info status
  getAdditionalInfoStatus(patientId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${patientId}/additional-info-status`);
  }
}
