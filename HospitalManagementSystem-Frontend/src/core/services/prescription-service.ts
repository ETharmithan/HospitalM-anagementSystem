import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface PrescriptionRequest {
  diagnosis: string;
  prescription: string;
  notes: string;
  visitDate: string;
  doctorId: string;
  patientId: string;
}

export interface PrescriptionResponse {
  recordId: string;
  diagnosis: string;
  prescription: string;
  notes: string;
  visitDate: string;
  doctorId: string;
  doctorName?: string;
  patientId: string;
  patientName?: string;
}

export interface PatientMedicalProfile {
  patientId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  imageUrl?: string;
  email?: string;
  phone?: string;
  address?: string;
  bloodType?: string;
  allergies?: string;
  chronicConditions?: string;
  pastIllnesses?: string;
  surgeries?: string;
  medicalHistoryNotes?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  emergencyContactRelationship?: string;
  prescriptions: PrescriptionSummary[];
  pastAppointments: AppointmentSummary[];
}

export interface PrescriptionSummary {
  recordId: string;
  diagnosis: string;
  prescription: string;
  notes: string;
  visitDate: string;
  doctorName?: string;
  doctorId: string;
}

export interface AppointmentSummary {
  appointmentId: string;
  appointmentDate: string;
  appointmentTime: string;
  status: string;
  doctorName?: string;
  hospitalName?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PrescriptionService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api/doctorpatientrecord';

  // Create a new prescription
  createPrescription(prescription: PrescriptionRequest): Observable<PrescriptionResponse> {
    return this.http.post<PrescriptionResponse>(this.baseUrl, prescription).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating prescription:', error);
        return throwError(() => new Error('Failed to save prescription.'));
      })
    );
  }

  // Get all prescriptions
  getAllPrescriptions(): Observable<PrescriptionResponse[]> {
    return this.http.get<PrescriptionResponse[]>(this.baseUrl).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching prescriptions:', error);
        return throwError(() => new Error('Failed to fetch prescriptions.'));
      })
    );
  }

  // Get prescription by ID
  getPrescriptionById(id: string): Observable<PrescriptionResponse> {
    return this.http.get<PrescriptionResponse>(`${this.baseUrl}/${id}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching prescription:', error);
        return throwError(() => new Error('Failed to fetch prescription.'));
      })
    );
  }

  // Update prescription
  updatePrescription(id: string, prescription: PrescriptionRequest): Observable<PrescriptionResponse> {
    return this.http.put<PrescriptionResponse>(`${this.baseUrl}/${id}`, prescription).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating prescription:', error);
        return throwError(() => new Error('Failed to update prescription.'));
      })
    );
  }

  // Delete prescription
  deletePrescription(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting prescription:', error);
        return throwError(() => new Error('Failed to delete prescription.'));
      })
    );
  }

  // Get prescriptions by patient ID
  getPrescriptionsByPatientId(patientId: string): Observable<PrescriptionResponse[]> {
    const url = `${this.baseUrl}/patient/${patientId}`;
    console.log('Making API call to:', url);
    return this.http.get<PrescriptionResponse[]>(url).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching patient prescriptions:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        console.error('Error details:', error.error);
        return throwError(() => new Error('Failed to fetch patient prescriptions.'));
      })
    );
  }

  // Get prescriptions by doctor ID
  getPrescriptionsByDoctorId(doctorId: string): Observable<PrescriptionResponse[]> {
    return this.http.get<PrescriptionResponse[]>(`${this.baseUrl}/doctor/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor prescriptions:', error);
        return throwError(() => new Error('Failed to fetch doctor prescriptions.'));
      })
    );
  }

  // Get patient's complete medical profile (for doctors)
  getPatientMedicalProfile(patientId: string): Observable<PatientMedicalProfile> {
    return this.http.get<PatientMedicalProfile>(`http://localhost:5245/api/patient/${patientId}/medical-profile`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching patient medical profile:', error);
        return throwError(() => new Error('Failed to fetch patient medical profile.'));
      })
    );
  }
}
