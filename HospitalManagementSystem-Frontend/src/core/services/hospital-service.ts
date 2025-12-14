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

export interface Admin {
  userId: string;
  username: string;
  email: string;
  isEmailVerified: boolean;
  hospitals: {
    hospitalId: string;
    hospitalName: string;
    assignedAt: string;
  }[];
}

export interface CreateAdminDto {
  username: string;
  email: string;
  password: string;
  hospitalId?: string;
}

export interface UpdateAdminDto {
  username?: string;
  email?: string;
  password?: string;
}

export interface Patient {
  patientId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
  nic: string;
  city: string;
  province: string;
  country: string;
  imageUrl?: string;
  appointmentCount: number;
}

export interface SystemUser {
  userId: string;
  username: string;
  email: string;
  role: string;
  isEmailVerified: boolean;
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

  // Get all hospitals (Public - for patients)
  getPublicHospitals(): Observable<Hospital[]> {
    return this.http.get<Hospital[]>(`${this.baseUrl}/publichospital/hospitals`).pipe(
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

  // Create hospital with admin in atomic transaction
  createHospitalWithAdmin(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/superadmin/hospitals/with-admin`, data).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating hospital with admin:', error);
        const message = error.error?.message || 'Failed to create hospital with admin.';
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

  // Get hospital details with statistics
  getHospitalDetails(hospitalId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/superadmin/hospitals/${hospitalId}/details`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching hospital details:', error);
        return throwError(() => new Error('Failed to fetch hospital details.'));
      })
    );
  }

  // Admin Management (CRUD)
  
  // Get all admins
  getAllAdmins(): Observable<Admin[]> {
    return this.http.get<Admin[]>(`${this.baseUrl}/superadmin/admins`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching admins:', error);
        return throwError(() => new Error('Failed to fetch admins.'));
      })
    );
  }

  // Create admin
  createAdmin(admin: CreateAdminDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/superadmin/admins`, admin).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating admin:', error);
        const message = error.error?.message || 'Failed to create admin.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Update admin
  updateAdmin(userId: string, admin: UpdateAdminDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/superadmin/admins/${userId}`, admin).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating admin:', error);
        const message = error.error?.message || 'Failed to update admin.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Delete admin
  deleteAdmin(userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/superadmin/admins/${userId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting admin:', error);
        const message = error.error?.message || 'Failed to delete admin.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Patient Management
  
  // Get all patients
  getAllPatients(): Observable<Patient[]> {
    return this.http.get<Patient[]>(`${this.baseUrl}/superadmin/patients`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching patients:', error);
        return throwError(() => new Error('Failed to fetch patients.'));
      })
    );
  }

  // Delete patient
  deletePatient(patientId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/superadmin/patients/${patientId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting patient:', error);
        const message = error.error?.message || 'Failed to delete patient.';
        return throwError(() => new Error(message));
      })
    );
  }

  // User Management (All roles)
  
  // Get all users
  getAllUsers(): Observable<SystemUser[]> {
    return this.http.get<SystemUser[]>(`${this.baseUrl}/superadmin/users`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching users:', error);
        return throwError(() => new Error('Failed to fetch users.'));
      })
    );
  }

  // Delete user
  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/superadmin/users/${userId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting user:', error);
        const message = error.error?.message || 'Failed to delete user.';
        return throwError(() => new Error(message));
      })
    );
  }
}
