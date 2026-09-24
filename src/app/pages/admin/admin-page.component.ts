import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminReservation, AdminService } from '../../core/admin.service';

const STORAGE_KEY = 'rezervacije-admin-pw';

@Component({
  selector: 'app-admin-page',
  standalone: true,
  imports: [DatePipe, FormsModule],
  templateUrl: './admin-page.component.html',
  styleUrl: './admin-page.component.scss',
})
export class AdminPageComponent {
  private api = inject(AdminService);

  password = signal(sessionStorage.getItem(STORAGE_KEY) ?? '');
  authed = signal(false);
  authError = signal<string | null>(null);
  loading = signal(false);
  reservations = signal<AdminReservation[]>([]);
  actingOn = signal<string | null>(null);

  passwordInput = '';

  constructor() {
    if (this.password()) this.tryLoad(this.password());
  }

  submitPassword() {
    this.tryLoad(this.passwordInput.trim());
  }

  private tryLoad(pw: string) {
    if (!pw) return;
    this.loading.set(true);
    this.authError.set(null);
    this.api.list(pw).subscribe({
      next: (list) => {
        this.password.set(pw);
        sessionStorage.setItem(STORAGE_KEY, pw);
        this.authed.set(true);
        this.reservations.set(sortReservations(list));
        this.loading.set(false);
      },
      error: () => {
        sessionStorage.removeItem(STORAGE_KEY);
        this.authed.set(false);
        this.authError.set('Pogrešna lozinka.');
        this.loading.set(false);
      },
    });
  }

  refresh() {
    this.tryLoad(this.password());
  }

  logout() {
    sessionStorage.removeItem(STORAGE_KEY);
    this.password.set('');
    this.authed.set(false);
    this.reservations.set([]);
  }

  confirm(r: AdminReservation) {
    this.actingOn.set(r.id);
    this.api.confirm(this.password(), r.id).subscribe({
      next: () => this.refresh(),
      error: () => this.actingOn.set(null),
    });
  }

  reject(r: AdminReservation) {
    this.actingOn.set(r.id);
    this.api.reject(this.password(), r.id).subscribe({
      next: () => this.refresh(),
      error: () => this.actingOn.set(null),
    });
  }
}

function sortReservations(list: AdminReservation[]): AdminReservation[] {
  const order: Record<string, number> = { pending: 0, confirmed: 1, rejected: 2 };
  return [...list].sort((a, b) => order[a.status] - order[b.status] || b.createdAt.localeCompare(a.createdAt));
}
