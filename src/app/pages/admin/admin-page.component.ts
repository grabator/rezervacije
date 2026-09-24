import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminReservation, AdminService } from '../../core/admin.service';

const STORAGE_KEY = 'rezervacije-admin-pw';

type StatusFilter = 'pending' | 'confirmed' | 'rejected' | 'all';

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
  filter = signal<StatusFilter>('pending');

  passwordInput = '';

  counts = computed(() => {
    const list = this.reservations();
    return {
      all: list.length,
      pending: list.filter(r => r.status === 'pending').length,
      confirmed: list.filter(r => r.status === 'confirmed').length,
      rejected: list.filter(r => r.status === 'rejected').length,
    };
  });

  filtered = computed(() => {
    const f = this.filter();
    const list = this.reservations();
    return f === 'all' ? list : list.filter(r => r.status === f);
  });

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
  return [...list].sort((a, b) => b.createdAt.localeCompare(a.createdAt));
}
