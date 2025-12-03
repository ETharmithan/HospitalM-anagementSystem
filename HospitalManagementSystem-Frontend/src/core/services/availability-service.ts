import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';

export interface TimeSlot {
  time: string;
  available: boolean;
  reason?: string;
}

export interface AvailabilityResponse {
  date: string;
  doctorId: string;
  doctorName: string;
  appointmentDurationMinutes: number;
  availableSlots: TimeSlot[];
  isFullyBooked: boolean;
  hasSchedule: boolean;
  isOnLeave: boolean;
  unavailableReason?: string;
}

export interface AvailableDatesResponse {
  doctorId: string;
  startDate: string;
  endDate: string;
  availableDates: string[];
  fullyBookedDates: string[];
  unavailableDates: string[];
}

@Injectable({
  providedIn: 'root',
})
export class AvailabilityService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api';

  // Get available time slots for a specific date
  getAvailability(doctorId: string, date: Date): Observable<AvailabilityResponse> {
    const dateStr = date.toISOString().split('T')[0]; // Format: YYYY-MM-DD
    return this.http.get<AvailabilityResponse>(`${this.baseUrl}/availability/doctor/${doctorId}/date/${dateStr}`).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching availability:', error);
        return throwError(() => new Error('Failed to fetch availability. Please try again.'));
      })
    );
  }

  // Get available dates for calendar (3 months range)
  getAvailableDates(doctorId: string, startDate?: Date, endDate?: Date): Observable<AvailableDatesResponse> {
    const start = startDate || new Date();
    const end = endDate || new Date();
    end.setMonth(end.getMonth() + 3); // 3 months ahead

    const params = new HttpParams()
      .set('startDate', start.toISOString().split('T')[0])
      .set('endDate', end.toISOString().split('T')[0]);

    return this.http.get<AvailableDatesResponse>(`${this.baseUrl}/availability/doctor/${doctorId}/dates`, { params }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching available dates:', error);
        return throwError(() => new Error('Failed to fetch available dates.'));
      })
    );
  }

  // Check if a specific slot is available
  checkSlotAvailability(doctorId: string, date: Date, time: string): Observable<{ available: boolean }> {
    return this.http.post<{ available: boolean }>(`${this.baseUrl}/availability/doctor/${doctorId}/check`, {
      date: date.toISOString().split('T')[0],
      time: time
    }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error checking slot availability:', error);
        return throwError(() => new Error('Failed to check availability.'));
      })
    );
  }
}

