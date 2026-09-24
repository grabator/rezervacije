import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'r/exclusive/rezervacije', pathMatch: 'full' },
  {
    path: 'r/:venue/rezervacije',
    loadComponent: () => import('./pages/event-list/event-list-page.component').then(m => m.EventListPageComponent),
  },
  {
    path: 'r/:venue/rezervacije/e/:eventId',
    loadComponent: () => import('./pages/event/event-page.component').then(m => m.EventPageComponent),
  },
  { path: '**', redirectTo: '' },
];
