import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { ReservationRequest, ReservationResult, ReservationStatus, Venue, VenueEvent } from './models';
import { API_BASE } from './api-config';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private http = inject(HttpClient);

  getVenue(slug: string): Observable<Venue> {
    return this.http.get<Venue>(`${API_BASE}/venues/${slug}`);
  }

  getEvents(slug: string): Observable<VenueEvent[]> {
    return this.http.get<VenueEvent[]>(`${API_BASE}/venues/${slug}/events`);
  }

  getEvent(id: string): Observable<VenueEvent> {
    return this.http.get<VenueEvent>(`${API_BASE}/events/${id}`);
  }

  reserve(req: ReservationRequest): Observable<ReservationResult> {
    return this.http.post<ReservationResult>(`${API_BASE}/reservations`, req).pipe(
      catchError((err: HttpErrorResponse) => {
        const msg = err.error?.message ?? 'Greška pri slanju zahtjeva. Pokušaj ponovo.';
        return throwError(() => new Error(msg));
      }),
    );
  }

  getReservationStatus(id: string): Observable<ReservationStatus> {
    return this.http.get<ReservationStatus>(`${API_BASE}/reservations/${id}/status`);
  }

  cancelReservation(id: string): Observable<object> {
    return this.http.post(`${API_BASE}/reservations/${id}/cancel`, {}).pipe(
      catchError((err: HttpErrorResponse) => {
        const msg = err.error?.message ?? 'Greška pri otkazivanju. Pokušaj ponovo.';
        return throwError(() => new Error(msg));
      }),
    );
  }
}
