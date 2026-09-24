import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReservationService } from '../../core/reservation.service';

@Component({
  selector: 'app-event-list-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <header class="head">
      <img class="mark" src="logo.jpg" [alt]="venue()?.name ?? 'Logo'" />
      <div><h1>{{ venue()?.name }}</h1><p>Izaberi event i rezerviši sto.</p></div>
    </header>
    <main class="list">
      @for (e of events(); track e.id) {
        <a class="ev" [routerLink]="['e', e.id]">
          <div class="d"><span>{{ e.startsAt | date: 'MMM' }}</span><b>{{ e.startsAt | date: 'd' }}</b></div>
          <div class="t"><b>{{ e.title }}</b><small>{{ e.startsAt | date: 'EEEE, HH:mm' }}</small></div>
          <span class="go" aria-hidden="true">›</span>
        </a>
      } @empty {
        <p class="empty">Trenutno nema eventova za rezervaciju. Zaprati nas na Instagramu za najave.</p>
      }
    </main>
  `,
  styles: [`
    .head { display:flex; gap:14px; align-items:center; padding:28px 16px 10px; max-width:640px; margin:0 auto; }
    .mark { width:56px; height:56px; border-radius:16px; object-fit:cover; flex:none; box-shadow:0 0 0 2px var(--surface), 0 0 0 4px var(--brass), 0 6px 16px -6px rgba(0,0,0,.6); }
    h1 { margin:0; font-size:22px; letter-spacing:-.02em; } p { margin:2px 0 0; color:var(--ink-soft); }
    .list { max-width:640px; margin:0 auto; padding:16px; display:grid; gap:12px; }
    .ev { display:flex; gap:14px; align-items:center; padding:14px; border-radius:18px; background:var(--surface); border:1px solid var(--line); color:inherit; text-decoration:none;
      box-shadow:inset 0 1px 0 rgba(255,255,255,.04); transition:transform .15s ease, box-shadow .15s ease, border-color .15s ease; }
    .ev:hover { transform:translateY(-2px); box-shadow:0 10px 24px -12px rgba(0,0,0,.6); border-color:var(--velvet); }
    .ev:focus-visible { outline:3px solid var(--velvet); outline-offset:2px; }
    .d { width:54px; padding:6px 0; border-radius:12px; background:var(--velvet); color:var(--ink-on-accent); text-align:center; line-height:1; flex:none; }
    .d span { display:block; font-size:12px; color:var(--ink-on-accent); opacity:.65; text-transform:capitalize; } .d b { font-size:22px; }
    .t { flex:1; display:flex; flex-direction:column; gap:3px; min-width:0; } .t small { color:var(--ink-soft); } .t small::first-letter { text-transform:uppercase; }
    .go { color:var(--muted); font-size:22px; flex:none; }
    .empty { color:var(--ink-soft); text-align:center; padding:40px 0; }
  `],
})
export class EventListPageComponent {
  private route = inject(ActivatedRoute);
  private api = inject(ReservationService);
  private slug = this.route.snapshot.paramMap.get('venue')!;
  venue = toSignal(this.api.getVenue(this.slug));
  events = toSignal(this.api.getEvents(this.slug), { initialValue: [] });
}
