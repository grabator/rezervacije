import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReservationService } from '../../core/reservation.service';
import { VenueEvent } from '../../core/models';

@Component({
  selector: 'app-event-list-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './event-list-page.component.html',
  styleUrl: './event-list-page.component.scss',
})
export class EventListPageComponent {
  private route = inject(ActivatedRoute);
  private api = inject(ReservationService);
  venueSlug = this.route.snapshot.paramMap.get('venue')!;
  venue = toSignal(this.api.getVenue(this.venueSlug));
  events = toSignal(this.api.getEvents(this.venueSlug), { initialValue: [] });

  lastReservationId = this.readLastReservationId();

  freeCount(e: VenueEvent): number {
    return e.floorPlan.tables.filter(t => t.status === 'free').length;
  }

  totalCount(e: VenueEvent): number {
    return e.floorPlan.tables.length;
  }

  private readLastReservationId(): string | null {
    try {
      const raw = localStorage.getItem('rezervacije-last');
      if (!raw) return null;
      const parsed = JSON.parse(raw) as { venueSlug: string; id: string };
      return parsed.venueSlug === this.venueSlug ? parsed.id : null;
    } catch {
      return null;
    }
  }
}
