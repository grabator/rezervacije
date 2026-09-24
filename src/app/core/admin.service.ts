import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from './api-config';

export interface AdminReservation {
  id: string;
  eventId: string;
  eventTitle: string;
  tableLabel: string;
  packageName: string;
  fullName: string;
  phone: string;
  email: string;
  note?: string;
  status: 'pending' | 'confirmed' | 'rejected';
  createdAt: string;
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
}
