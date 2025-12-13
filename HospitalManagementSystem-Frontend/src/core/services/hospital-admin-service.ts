import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface Doctor {
  doctorId: string;
  name: string;
  email: string;
  phone?: string;
  departmentId: string;
  departmentName?: string;
  qualification?: string;
  licenseNumber?: string;
  status: string;
  profileImage?: string;
}

export interface DoctorSchedule {
  scheduleId: string;
  doctorId: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  maxPatients: number;
  isActive: boolean;
}

export interface Department {
  departmentId: string;
  name: string;
  description?: string;
  hospitalId: string;
  isActive: boolean;
}

export interface Appointment {
  appointmentId: string;
  patientId: string;
  patientName?: string;
  doctorId: string;
  doctorName?: string;
  appointmentDate: string;
  appointmentTime: string;
  status: string;
  reason?: string;
  notes?: string;
}

export interface CreateDoctorRequest {
  name: string;
  email: string;
  phone: string;
  departmentId: string;
  qualification: string;
  licenseNumber: string;
  status: string;
  profileImage?: string;
}

export interface CreateDepartmentRequest {
  name: string;
  description?: string;
  hospitalId?: string;
}

export interface CreateScheduleRequest {
  doctorId: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  maxPatients: number;
}

@Injectable({
  providedIn: 'root',
})
export class HospitalAdminService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api/hospitaladmin';

  // Doctor Management
  getDoctors(departmentId?: string): Observable<Doctor[]> {
    const url = departmentId 
      ? `${this.baseUrl}/doctors?departmentId=${departmentId}`
      : `${this.baseUrl}/doctors`;
    return this.http.get<Doctor[]>(url).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctors:', error);
        return throwError(() => new Error('Failed to fetch doctors.'));
      })
    );
  }

  getDoctor(doctorId: string): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.baseUrl}/doctors/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor:', error);
        return throwError(() => new Error('Failed to fetch doctor details.'));
      })
    );
  }

  createDoctor(doctor: CreateDoctorRequest): Observable<Doctor> {
    return this.http.post<Doctor>(`${this.baseUrl}/doctors`, doctor).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating doctor:', error);
        const message = error.error?.message || 'Failed to create doctor.';
        return throwError(() => new Error(message));
      })
    );
  }

  updateDoctor(doctorId: string, doctor: CreateDoctorRequest): Observable<Doctor> {
    return this.http.put<Doctor>(`${this.baseUrl}/doctors/${doctorId}`, doctor).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating doctor:', error);
        const message = error.error?.message || 'Failed to update doctor.';
        return throwError(() => new Error(message));
      })
    );
  }

  deleteDoctor(doctorId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/doctors/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting doctor:', error);
        return throwError(() => new Error('Failed to delete doctor.'));
      })
    );
  }

  // Doctor Schedule Management
  getDoctorSchedules(doctorId: string): Observable<DoctorSchedule[]> {
    return this.http.get<DoctorSchedule[]>(`${this.baseUrl}/doctors/${doctorId}/schedules`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching schedules:', error);
        return throwError(() => new Error('Failed to fetch doctor schedules.'));
      })
    );
  }

  createDoctorSchedule(doctorId: string, schedule: CreateScheduleRequest): Observable<DoctorSchedule> {
    return this.http.post<DoctorSchedule>(`${this.baseUrl}/doctors/${doctorId}/schedules`, schedule).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating schedule:', error);
        const message = error.error?.message || 'Failed to create schedule.';
        return throwError(() => new Error(message));
      })
    );
  }

  deleteDoctorSchedule(doctorId: string, scheduleId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/doctors/${doctorId}/schedules/${scheduleId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting schedule:', error);
        return throwError(() => new Error('Failed to delete schedule.'));
      })
    );
  }

  // Department Management
  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.baseUrl}/departments`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching departments:', error);
        return throwError(() => new Error('Failed to fetch departments.'));
      })
    );
  }

  getDepartment(departmentId: string): Observable<Department> {
    return this.http.get<Department>(`${this.baseUrl}/departments/${departmentId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching department:', error);
        return throwError(() => new Error('Failed to fetch department details.'));
      })
    );
  }

  createDepartment(department: CreateDepartmentRequest): Observable<Department> {
    return this.http.post<Department>(`${this.baseUrl}/departments`, department).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating department:', error);
        const message = error.error?.message || 'Failed to create department.';
        return throwError(() => new Error(message));
      })
    );
  }

  updateDepartment(departmentId: string, department: CreateDepartmentRequest): Observable<Department> {
    return this.http.put<Department>(`${this.baseUrl}/departments/${departmentId}`, department).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating department:', error);
        const message = error.error?.message || 'Failed to update department.';
        return throwError(() => new Error(message));
      })
    );
  }

  deleteDepartment(departmentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/departments/${departmentId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting department:', error);
        return throwError(() => new Error('Failed to delete department.'));
      })
    );
  }
}
