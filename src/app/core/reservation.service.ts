import { Injectable } from '@angular/core';
import { Observable, delay, of, throwError } from 'rxjs';
import { EVENTS, VENUES } from './mock-data';
import { ReservationRequest, ReservationResult, Venue, VenueEvent } from './models';

/**
 * Za sada radi nad mock podacima u memoriji.
 * Kad backend bude spreman (npr. .NET Web API), zamijeni tijela metoda sa HttpClient pozivima:
 *   GET  /api/venues/{slug}
 *   GET  /api/venues/{slug}/events
 *   GET  /api/events/{id}
 *   POST /api/reservations
 * Backend MORA sam provjeriti da je sto slobodan (zaštita od duple rezervacije).
 */
@Injectable({ providedIn: 'root' })
export class ReservationService {
  getVenue(slug: string): Observable<Venue | undefined> {
    return of(VENUES.find(v => v.slug === slug)).pipe(delay(150));
  }

  getEvents(slug: string): Observable<VenueEvent[]> {
    return of(EVENTS.filter(e => e.venueSlug === slug)).pipe(delay(150));
  }

  getEvent(id: string): Observable<VenueEvent | undefined> {
    return of(EVENTS.find(e => e.id === id)).pipe(delay(150));
  }

  reserve(req: ReservationRequest): Observable<ReservationResult> {
    const event = EVENTS.find(e => e.id === req.eventId);
    const table = event?.floorPlan.tables.find(t => t.id === req.tableId);
    if (!table || table.status !== 'free') {
      return throwError(() => new Error('Ovaj sto je u međuvremenu zauzet. Izaberi drugi.'));
    }
    table.status = 'pending';
    return of<ReservationResult>({ reservationId: crypto.randomUUID(), status: 'pending' }).pipe(delay(400));
  }
}
