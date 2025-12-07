import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map, timeout } from 'rxjs/operators';
import { Appointment, CreateAppointmentRequest } from '../../types/doctor';

@Injectable({
  providedIn: 'root',
})
export class AppointmentService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api';

  // Get all appointments
  getAllAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.baseUrl}/doctorappointment`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching appointments:', error);
        return throwError(() => new Error('Failed to fetch appointments.'));
      })
    );
  }

  // Get appointment by ID
  getAppointmentById(appointmentId: string): Observable<Appointment> {
    return this.http.get<Appointment>(`${this.baseUrl}/doctorappointment/${appointmentId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching appointment:', error);
        return throwError(() => new Error('Failed to fetch appointment details.'));
      })
    );
  }

  // Get appointments by patient ID (using backend endpoint)
  getAppointmentsByPatientId(patientId: string): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.baseUrl}/doctorappointment/patient/${patientId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching patient appointments:', error);
        return throwError(() => new Error('Failed to fetch your appointments.'));
      })
    );
  }

  // Get appointments by doctor ID (using backend endpoint)
  getAppointmentsByDoctorId(doctorId: string): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.baseUrl}/doctorappointment/doctor/${doctorId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor appointments:', error);
        return throwError(() => new Error('Failed to fetch appointments.'));
      })
    );
  }

  // Get available time slots for a doctor on a specific date
  getAvailableSlots(doctorId: string, date: Date, hospitalId?: string): Observable<string[]> {
    let params = new HttpParams().set('date', date.toISOString());
    if (hospitalId) {
      params = params.set('hospitalId', hospitalId);
    }
    return this.http.get<string[]>(`${this.baseUrl}/doctorappointment/available-slots/${doctorId}`, { params }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching available slots:', error);
        return throwError(() => new Error('Failed to fetch available time slots.'));
      })
    );
  }

  // Get fully booked dates for calendar disabling
  getFullyBookedDates(doctorId: string, startDate: Date, endDate: Date, hospitalId?: string): Observable<Date[]> {
    let params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    if (hospitalId) {
      params = params.set('hospitalId', hospitalId);
    }
    return this.http.get<string[]>(`${this.baseUrl}/doctorappointment/fully-booked-dates/${doctorId}`, { params }).pipe(
      map(dates => dates.map(d => new Date(d))),
      timeout(10000),
      catchError(error => {
        console.error('Error fetching fully booked dates:', error);
        return throwError(() => new Error('Failed to fetch booked dates.'));
      })
    );
  }

  // Check if a specific slot is available
  checkSlotAvailability(doctorId: string, date: Date, time: string): Observable<{ isAvailable: boolean }> {
    const params = new HttpParams()
      .set('date', date.toISOString())
      .set('time', time);
    return this.http.get<{ isAvailable: boolean }>(`${this.baseUrl}/doctorappointment/check-availability/${doctorId}`, { params }).pipe(
      timeout(5000),
      catchError(error => {
        console.error('Error checking slot availability:', error);
        return throwError(() => new Error('Failed to check slot availability.'));
      })
    );
  }

  // Create new appointment
  createAppointment(appointment: CreateAppointmentRequest): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.baseUrl}/doctorappointment`, appointment).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error creating appointment:', error);
        const errorMessage = error.error?.message || 'Failed to book appointment. Please try again.';
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  // Update appointment
  updateAppointment(appointmentId: string, appointment: CreateAppointmentRequest): Observable<Appointment> {
    return this.http.put<Appointment>(`${this.baseUrl}/doctorappointment/${appointmentId}`, appointment).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error updating appointment:', error);
        const errorMessage = error.error?.message || 'Failed to update appointment.';
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  // Cancel appointment
  cancelAppointment(appointmentId: string): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/doctorappointment/cancel/${appointmentId}`, {}).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error canceling appointment:', error);
        return throwError(() => new Error('Failed to cancel appointment.'));
      })
    );
  }

  // Delete appointment
  deleteAppointment(appointmentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/doctorappointment/${appointmentId}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error deleting appointment:', error);
        return throwError(() => new Error('Failed to delete appointment.'));
      })
    );
  }
}

