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

export interface AdminHospitalLocation {
  hospitalId: string;
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
  latitude?: number | null;
  longitude?: number | null;
}

export interface UpdateMyHospitalRequest {
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
  latitude?: number | null;
  longitude?: number | null;
}

export interface UpdateMyProfileRequest {
  username?: string;
  email?: string;
  password?: string;
}

export interface MyProfileResponse {
  userId: string;
  username: string;
  email: string;
  imageUrl?: string | null;
  role: string;
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

  getMyHospital(): Observable<AdminHospitalLocation> {
    return this.http.get<AdminHospitalLocation>(`${this.baseUrl}/my-hospital`);
  }

  updateMyHospital(payload: UpdateMyHospitalRequest): Observable<AdminHospitalLocation> {
    return this.http.put<AdminHospitalLocation>(`${this.baseUrl}/my-hospital`, payload);
  }

  getMyProfile(): Observable<MyProfileResponse> {
    return this.http.get<MyProfileResponse>(`${this.baseUrl}/my-profile`);
  }

  updateMyProfile(payload: UpdateMyProfileRequest): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/my-profile`, payload);
  }
}
