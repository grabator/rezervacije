import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReservationService } from '../../core/reservation.service';
import { ReservationStatus } from '../../core/models';

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
  private reservationId = this.route.snapshot.paramMap.get('id')!;

  venueSlug = this.route.snapshot.paramMap.get('venue')!;

  status = signal<ReservationStatus | null | undefined>(undefined);
  confirmingCancel = signal(false);
  cancelling = signal(false);
  cancelError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  private load() {
    this.api.getReservationStatus(this.reservationId).subscribe({
      next: (s) => this.status.set(s),
      error: () => this.status.set(null),
    });
  }

  statusLabel(status: string): string {
    return STATUS_LABELS[status] ?? status;
  }

  askCancel() {
    this.confirmingCancel.set(true);
  }

  dismissCancel() {
    this.confirmingCancel.set(false);
  }

  cancel() {
    this.cancelling.set(true);
    this.cancelError.set(null);
    this.api.cancelReservation(this.reservationId).subscribe({
      next: () => {
        this.cancelling.set(false);
        this.confirmingCancel.set(false);
        this.load();
      },
      error: (e: Error) => {
        this.cancelling.set(false);
        this.cancelError.set(e.message);
      },
    });
  }
}
