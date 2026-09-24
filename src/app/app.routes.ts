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
  {
    path: 'r/:venue/rezervacije/status/:id',
    loadComponent: () => import('./pages/reservation-status/reservation-status-page.component').then(m => m.ReservationStatusPageComponent),
  },
  {
    path: 'r/:venue/admin',
    loadComponent: () => import('./pages/admin/admin-page.component').then(m => m.AdminPageComponent),
  },
  {
    path: 'privatnost',
    loadComponent: () => import('./pages/privacy/privacy-page.component').then(m => m.PrivacyPageComponent),
  },
  { path: '**', redirectTo: '' },
];
