import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface CreateEPrescriptionRequest {
  diagnosis: string;
  prescription: string;
  notes?: string;
  visitDate: string; // ISO string
  doctorId: string;
  patientId: string;
}

export interface EPrescriptionResponse {
  ePrescriptionId: string;
  diagnosis: string;
  prescription: string;
  notes: string;
  visitDate: string;
  createdAt: string;
  doctorId: string;
  patientId: string;
}

export interface UpdateEPrescriptionRequest {
  diagnosis: string;
  prescription: string;
  notes?: string;
  visitDate: string;
}

@Injectable({
  providedIn: 'root',
})
export class EPrescriptionService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api/eprescription';

  create(payload: CreateEPrescriptionRequest): Observable<EPrescriptionResponse> {
    return this.http.post<EPrescriptionResponse>(this.baseUrl, payload).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating e-prescription:', error);
        return throwError(() => error);
      })
    );
  }

  getById(id: string): Observable<EPrescriptionResponse> {
    return this.http.get<EPrescriptionResponse>(`${this.baseUrl}/${id}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching e-prescription:', error);
        return throwError(() => error);
      })
    );
  }

  getByPatientId(patientId: string): Observable<EPrescriptionResponse[]> {
    return this.http.get<EPrescriptionResponse[]>(`${this.baseUrl}/patient/${patientId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching patient e-prescriptions:', error);
        return throwError(() => error);
      })
    );
  }

  getByDoctorId(doctorId: string): Observable<EPrescriptionResponse[]> {
    return this.http.get<EPrescriptionResponse[]>(`${this.baseUrl}/doctor/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor e-prescriptions:', error);
        return throwError(() => error);
      })
    );
  }

  getMyPrescriptions(): Observable<EPrescriptionResponse[]> {
    return this.http.get<EPrescriptionResponse[]>(`${this.baseUrl}/my`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching my e-prescriptions:', error);
        return throwError(() => error);
      })
    );
  }

  update(id: string, payload: UpdateEPrescriptionRequest): Observable<EPrescriptionResponse> {
    return this.http.put<EPrescriptionResponse>(`${this.baseUrl}/${id}`, payload).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating e-prescription:', error);
        return throwError(() => error);
      })
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting e-prescription:', error);
        return throwError(() => error);
      })
    );
  }

  download(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/download`, { responseType: 'blob' }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error downloading e-prescription:', error);
        return throwError(() => error);
      })
    );
  }
}
