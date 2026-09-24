import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { ReservationService } from '../../core/reservation.service';

const STATUS_LABELS: Record<string, string> = {
  pending: 'Na čekanju',
  confirmed: 'Potvrđeno',
  rejected: 'Odbijeno',
  cancelled: 'Otkazano',
};

@Component({
  selector: 'app-reservation-status-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './reservation-status-page.component.html',
  styleUrl: './reservation-status-page.component.scss',
})
export class ReservationStatusPageComponent {
  private route = inject(ActivatedRoute);
  private api = inject(ReservationService);

  venueSlug = this.route.snapshot.paramMap.get('venue')!;

  status = toSignal(
    this.api.getReservationStatus(this.route.snapshot.paramMap.get('id')!).pipe(
      catchError(() => of(null)),
    ),
    { initialValue: undefined },
  );

  statusLabel(status: string): string {
    return STATUS_LABELS[status] ?? status;
  }
}
