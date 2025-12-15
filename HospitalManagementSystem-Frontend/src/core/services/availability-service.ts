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
  getAvailability(doctorId: string, date: Date, hospitalId?: string): Observable<AvailabilityResponse> {
    // Format date as YYYY-MM-DD in local timezone to avoid UTC issues
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const localDateString = `${year}-${month}-${day}`;

    let params = new HttpParams();
    if (hospitalId) {
      params = params.set('hospitalId', hospitalId);
    }

    return this.http.get<AvailabilityResponse>(`${this.baseUrl}/availability/doctor/${doctorId}/date/${localDateString}`, { params }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching availability:', error);
        return throwError(() => new Error('Failed to fetch availability. Please try again.'));
      })
    );
  }

  // Get available dates for calendar (3 months range)
  getAvailableDates(doctorId: string, startDate?: Date, endDate?: Date, hospitalId?: string): Observable<AvailableDatesResponse> {
    const start = startDate || new Date();
    const end = endDate || new Date();
    end.setMonth(end.getMonth() + 3); // 3 months ahead

    // Format dates as YYYY-MM-DD in local timezone to avoid UTC issues
    const formatDateLocal = (date: Date) => {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    };

    let params = new HttpParams()
      .set('startDate', formatDateLocal(start))
      .set('endDate', formatDateLocal(end));

    if (hospitalId) {
      params = params.set('hospitalId', hospitalId);
    }

    return this.http.get<AvailableDatesResponse>(`${this.baseUrl}/availability/doctor/${doctorId}/dates`, { params }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error fetching available dates:', error);
        return throwError(() => new Error('Failed to fetch available dates.'));
      })
    );
  }

  // Check if a specific slot is available
  checkSlotAvailability(doctorId: string, date: Date, time: string, hospitalId?: string): Observable<{ available: boolean }> {
    // Format date as YYYY-MM-DD in local timezone to avoid UTC issues
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const localDateString = `${year}-${month}-${day}`;
    
    return this.http.post<{ available: boolean }>(`${this.baseUrl}/availability/doctor/${doctorId}/check`, {
      date: localDateString,
      time: time,
      hospitalId: hospitalId || null
    }).pipe(
      timeout(10000),
      catchError(error => {
        console.error('Error checking slot availability:', error);
        return throwError(() => new Error('Failed to check availability.'));
      })
    );
  }
}

