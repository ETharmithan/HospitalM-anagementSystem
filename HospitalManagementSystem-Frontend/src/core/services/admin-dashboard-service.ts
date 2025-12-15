import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminOverview } from '../../types/admin-overview';

export interface HospitalInfo {
  hospitalId: string;
  hospitalName: string;
  hospitalAddress: string;
  hospitalCity: string;
  hospitalEmail: string;
  hospitalPhone: string;
}

@Injectable({
  providedIn: 'root',
})
export class AdminDashboardService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5245/api/admindashboard';

  getOverview(): Observable<AdminOverview> {
    return this.http.get<AdminOverview>(`${this.baseUrl}/overview`);
  }

  getHospitalInfo(): Observable<HospitalInfo> {
    return this.http.get<HospitalInfo>(`${this.baseUrl}/hospital-info`);
  }

  getAppointments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/appointments`);
  }

  approveCancellation(appointmentId: string, note?: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/appointments/${appointmentId}/approve-cancellation`, { note });
  }

  rejectCancellation(appointmentId: string, reason?: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/appointments/${appointmentId}/reject-cancellation`, { reason });
  }
}
