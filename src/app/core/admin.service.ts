import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from './api-config';

export interface AdminReservation {
  id: string;
  eventId: string;
  eventTitle: string;
  tableLabel: string;
  fullName: string;
  phone: string;
  email: string;
  note?: string;
  status: 'pending' | 'confirmed' | 'rejected' | 'cancelled';
  createdAt: string;
}

export interface AdminEvent {
  id: string;
  title: string;
  subtitle: string;
  startsAt: string;
  imageUrl?: string;
}

export interface CreateEventRequest {
  venueSlug: string;
  title: string;
  subtitle?: string;
  startsAt: string;
  imageUrl?: string;
}

export interface UpdateEventRequest {
  title: string;
  subtitle?: string;
  startsAt: string;
  imageUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);

  private headers(password: string): HttpHeaders {
    return new HttpHeaders({ 'X-Admin-Password': password });
  }

  list(password: string): Observable<AdminReservation[]> {
    return this.http.get<AdminReservation[]>(`${API_BASE}/admin/reservations`, { headers: this.headers(password) });
  }

  confirm(password: string, id: string): Observable<object> {
    return this.http.post(`${API_BASE}/admin/reservations/${id}/confirm`, {}, { headers: this.headers(password) });
  }

  reject(password: string, id: string): Observable<object> {
    return this.http.post(`${API_BASE}/admin/reservations/${id}/reject`, {}, { headers: this.headers(password) });
  }

  cancel(password: string, id: string): Observable<object> {
    return this.http.post(`${API_BASE}/admin/reservations/${id}/cancel`, {}, { headers: this.headers(password) });
  }

  listEvents(password: string): Observable<AdminEvent[]> {
    return this.http.get<AdminEvent[]>(`${API_BASE}/admin/events`, { headers: this.headers(password) });
  }

  createEvent(password: string, req: CreateEventRequest): Observable<AdminEvent> {
    return this.http.post<AdminEvent>(`${API_BASE}/admin/events`, req, { headers: this.headers(password) });
  }

  updateEvent(password: string, id: string, req: UpdateEventRequest): Observable<AdminEvent> {
    return this.http.put<AdminEvent>(`${API_BASE}/admin/events/${id}`, req, { headers: this.headers(password) });
  }

  deleteEvent(password: string, id: string): Observable<object> {
    return this.http.delete(`${API_BASE}/admin/events/${id}`, { headers: this.headers(password) });
  }
}
