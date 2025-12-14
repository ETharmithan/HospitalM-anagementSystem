import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

export interface LocationSearchResult {
  displayName: string;
  lat: number;
  lng: number;
  className?: string;
  type?: string;
  address?: {
    houseNumber?: string;
    road?: string;
    neighbourhood?: string;
    suburb?: string;
    city?: string;
    town?: string;
    village?: string;
    state?: string;
    country?: string;
    postcode?: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class LocationService {
  private http = inject(HttpClient);

  private defaultCountryCode = 'lk';

  search(
    query: string,
    limit = 5,
    countryCode: string = this.defaultCountryCode,
    onlyHospitalsAndClinics = false
  ): Observable<LocationSearchResult[]> {
    const trimmed = (query || '').trim();
    if (!trimmed) return new Observable<LocationSearchResult[]>(subscriber => {
      subscriber.next([]);
      subscriber.complete();
    });

    const params = new HttpParams()
      .set('q', trimmed)
      .set('format', 'json')
      .set('addressdetails', '1')
      .set('countrycodes', countryCode)
      .set('bounded', '0')
      // Sri Lanka bounding box: left,bottom,right,top
      .set('viewbox', '79.5214,5.9167,81.8790,9.8353')
      .set('limit', String(limit));

    return this.http
      .get<any[]>(`https://nominatim.openstreetmap.org/search`, {
        params,
        headers: {
          'Accept': 'application/json'
        }
      })
      .pipe(
        map((rows) => (rows || []).map(this.mapNominatimResult)),
        map((results) => {
          if (!onlyHospitalsAndClinics) return results;
          return results.filter(this.isHospitalOrClinicResult);
        })
      );
  }

  reverseGeocode(lat: number, lng: number): Observable<LocationSearchResult | null> {
    const params = new HttpParams()
      .set('lat', String(lat))
      .set('lon', String(lng))
      .set('format', 'json')
      .set('addressdetails', '1');

    return this.http
      .get<any>(`https://nominatim.openstreetmap.org/reverse`, {
        params,
        headers: {
          'Accept': 'application/json'
        }
      })
      .pipe(map((row) => (row ? this.mapNominatimResult(row) : null)));
  }

  private mapNominatimResult = (row: any): LocationSearchResult => {
    const address = row?.address || {};

    return {
      displayName: row?.display_name || '',
      lat: Number(row?.lat),
      lng: Number(row?.lon),
      className: row?.class,
      type: row?.type,
      address: {
        houseNumber: address?.house_number,
        road: address?.road,
        neighbourhood: address?.neighbourhood,
        suburb: address?.suburb,
        city: address?.city,
        town: address?.town,
        village: address?.village,
        state: address?.state,
        country: address?.country,
        postcode: address?.postcode,
      },
    };
  };

  private isHospitalOrClinicResult = (r: LocationSearchResult): boolean => {
    const name = (r.displayName || '').toLowerCase();
    const className = (r.className || '').toLowerCase();
    const type = (r.type || '').toLowerCase();

    const keywordMatch =
      name.includes('hospital') ||
      name.includes('clinic') ||
      name.includes('medical centre') ||
      name.includes('medical center') ||
      name.includes('health centre') ||
      name.includes('health center') ||
      name.includes('nursing home');

    if (className === 'amenity' && (type === 'hospital' || type === 'clinic')) return true;

    return keywordMatch;
  };
}
