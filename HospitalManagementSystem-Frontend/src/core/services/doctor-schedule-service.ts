import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface DoctorSchedule {
  scheduleId: string;
  scheduleDate?: string;
  dayOfWeek?: string;
  isRecurring: boolean;
  startTime: string;
  endTime: string;
  doctorId: string;
  doctorName: string;
  hospitalId?: string;
  hospitalName?: string;
}

export interface CreateScheduleRequest {
  doctorId: string;
  scheduleDate?: string;
  dayOfWeek?: string;
  isRecurring: boolean;
  startTime: string;
  endTime: string;
  hospitalId: string;
}

export interface HospitalOption {
  hospitalId: string;
  name: string;
  city: string;
  address: string;
}

@Injectable({
  providedIn: 'root',
})
export class DoctorScheduleService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api/doctorschedule';

  // Get all schedules for a specific doctor
  getSchedulesByDoctorId(doctorId: string): Observable<DoctorSchedule[]> {
    return this.http.get<DoctorSchedule[]>(`${this.baseUrl}/doctor/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor schedules:', error);
        return throwError(() => new Error('Failed to fetch schedules.'));
      })
    );
  }

  // Get schedules for current logged-in doctor
  getMySchedules(): Observable<DoctorSchedule[]> {
    return this.http.get<DoctorSchedule[]>(`${this.baseUrl}/my-schedules`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching my schedules:', error);
        return throwError(() => new Error('Failed to fetch schedules.'));
      })
    );
  }

  // Get doctor ID for current logged-in doctor
  getMyDoctorId(): Observable<{ doctorId: string; doctorName: string }> {
    return this.http.get<{ doctorId: string; doctorName: string }>(`${this.baseUrl}/my-doctor-id`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor ID:', error);
        return throwError(() => new Error('Failed to fetch doctor ID.'));
      })
    );
  }

  // Get a specific schedule by ID
  getScheduleById(scheduleId: string): Observable<DoctorSchedule> {
    return this.http.get<DoctorSchedule>(`${this.baseUrl}/${scheduleId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching schedule:', error);
        return throwError(() => new Error('Failed to fetch schedule details.'));
      })
    );
  }

  // Create a new schedule
  createSchedule(schedule: CreateScheduleRequest): Observable<DoctorSchedule> {
    return this.http.post<DoctorSchedule>(this.baseUrl, schedule).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating schedule:', error);
        const message = error.error?.message || 'Failed to create schedule.';
        return throwError(() => new Error(message));
      })
    );
  }

  // Delete a schedule
  deleteSchedule(scheduleId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${scheduleId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting schedule:', error);
        return throwError(() => new Error('Failed to delete schedule.'));
      })
    );
  }

  // Get all hospitals for dropdown
  getHospitals(): Observable<HospitalOption[]> {
    return this.http.get<HospitalOption[]>(`${this.baseUrl}/hospitals`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching hospitals:', error);
        return throwError(() => new Error('Failed to fetch hospitals.'));
      })
    );
  }
}
