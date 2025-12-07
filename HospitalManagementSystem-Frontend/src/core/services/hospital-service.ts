import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface Hospital {
  hospitalId: string;
  name: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  phoneNumber?: string;
  email?: string;
  website?: string;
  description?: string;
  isActive: boolean;
  createdDate?: string;
  hospitalAdmins?: HospitalAdmin[];
}

export interface HospitalAdmin {
  hospitalAdminId: string;
  hospitalId: string;
  userId: string;
  userName?: string;
  userEmail?: string;
}

export interface CreateHospitalRequest {
  name: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  phoneNumber: string;
  email: string;
  website?: string;
  description?: string;
}

export interface CreateAdminRequest {
  email: string;
  displayName: string;
  password: string;
  role: string;
  imageUrl?: string;
}

@Injectable({
  providedIn: 'root',
})
export class HospitalService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api';

  // Get all hospitals (SuperAdmin)
  getAllHospitals(): Observable<Hospital[]> {
    return this.http.get<Hospital[]>(`${this.baseUrl}/superadmin/hospitals`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching hospitals:', error);
        return throwError(() => new Error('Failed to fetch hospitals.'));
      })
    );
  }

  // Get hospital by ID
  getHospitalById(hospitalId: string): Observable<Hospital> {
    return this.http.get<Hospital>(`${this.baseUrl}/superadmin/hospitals/${hospitalId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching hospital:', error);
        return throwError(() => new Error('Failed to fetch hospital details.'));
      })
    );
  }

  // Create hospital
  createHospital(hospital: CreateHospitalRequest): Observable<Hospital> {
    console.log('Creating hospital with data:', hospital);
    return this.http.post<Hospital>(`${this.baseUrl}/superadmin/hospitals`, hospital).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating hospital:', error);
        console.error('Error details:', JSON.stringify(error.error));
        const message = error.error?.message || error.error?.title || JSON.stringify(error.error?.errors) || 'Failed to create hospital.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Update hospital
  updateHospital(hospitalId: string, hospital: CreateHospitalRequest): Observable<Hospital> {
    return this.http.put<Hospital>(`${this.baseUrl}/superadmin/hospitals/${hospitalId}`, hospital).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating hospital:', error);
        return throwError(() => new Error('Failed to update hospital.'));
      })
    );
  }

  // Delete hospital
  deleteHospital(hospitalId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/superadmin/hospitals/${hospitalId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting hospital:', error);
        return throwError(() => new Error('Failed to delete hospital.'));
      })
    );
  }

  // Assign admin to hospital
  assignHospitalAdmin(hospitalId: string, userId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/superadmin/hospitals/${hospitalId}/admins`, { userId }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error assigning admin:', error);
        const message = error.error?.message || 'Failed to assign admin.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Remove admin from hospital
  removeHospitalAdmin(hospitalId: string, userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/superadmin/hospitals/${hospitalId}/admins/${userId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error removing admin:', error);
        return throwError(() => new Error('Failed to remove admin.'));
      })
    );
  }
}
