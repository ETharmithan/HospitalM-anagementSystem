import { HttpClient } from '@angular/common/http';
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

  // Get appointments by patient ID (filtering on client side until backend endpoint is added)
  getAppointmentsByPatientId(patientId: string): Observable<Appointment[]> {
    return this.getAllAppointments().pipe(
      map(appointments => appointments.filter(apt => apt.patientId === patientId)),
      timeout(10000),
      catchError(error => {
        console.error('Error fetching patient appointments:', error);
        return throwError(() => new Error('Failed to fetch your appointments.'));
      })
    );
  }

  // Get appointments by doctor ID (filtering on client side until backend endpoint is added)
  getAppointmentsByDoctorId(doctorId: string): Observable<Appointment[]> {
    return this.getAllAppointments().pipe(
      map(appointments => appointments.filter(apt => apt.doctorId === doctorId)),
      timeout(10000),
      catchError(error => {
        console.error('Error fetching doctor appointments:', error);
        return throwError(() => new Error('Failed to fetch appointments.'));
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
        return throwError(() => new Error('Failed to update appointment.'));
      })
    );
  }

  // Cancel appointment (update status)
  cancelAppointment(appointmentId: string): Observable<Appointment> {
    return this.http.put<Appointment>(`${this.baseUrl}/doctorappointment/${appointmentId}/cancel`, {}).pipe(
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

