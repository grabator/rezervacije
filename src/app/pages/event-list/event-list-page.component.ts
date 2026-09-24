import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReservationService } from '../../core/reservation.service';

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
  private slug = this.route.snapshot.paramMap.get('venue')!;
  venue = toSignal(this.api.getVenue(this.slug));
  events = toSignal(this.api.getEvents(this.slug), { initialValue: [] });
}
