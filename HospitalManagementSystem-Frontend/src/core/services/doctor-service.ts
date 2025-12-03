import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { Doctor, Department } from '../../types/doctor';

@Injectable({
  providedIn: 'root',
})
export class DoctorService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api';

  // Get all doctors
  getAllDoctors(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(`${this.baseUrl}/doctor`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctors:', error);
        return throwError(() => new Error('Failed to fetch doctors. Please try again.'));
      })
    );
  }

  // Get doctor by ID
  getDoctorById(doctorId: string): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.baseUrl}/doctor/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor:', error);
        return throwError(() => new Error('Failed to fetch doctor details.'));
      })
    );
  }

  // Get all departments
  getAllDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.baseUrl}/department`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching departments:', error);
        return throwError(() => new Error('Failed to fetch departments.'));
      })
    );
  }

  // Get doctors by department
  getDoctorsByDepartment(departmentId: string): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(`${this.baseUrl}/doctor/department/${departmentId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctors by department:', error);
        return throwError(() => new Error('Failed to fetch doctors.'));
      })
    );
  }
}

