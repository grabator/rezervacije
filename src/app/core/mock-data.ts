import { FloorPlan, FloorTable, Venue, VenueEvent } from './models';

export const VENUES: Venue[] = [
  {
    slug: 'exclusive',
    name: 'Exclusive Caffe Lounge',
    subtitle: 'Rezervacija stola',
    instagram: 'https://www.instagram.com/exclusivecaffe.lounge',
  },
];

// Stvarni raspored sale (po opisu i skici vlasnika):
// - gore-lijevo šank, shisha odmah ispod (iza) njega; WC desno, iznad prostora S5/S6
// - kod ulaza 3 stola u koloni (jedan za 8 osoba)
// - oba ulaza (u lokal i u baštu) su dolje lijevo, na početku sale, jedan pa drugi
// - prvi red stolova (S4-S10), pa prolaz kroz salu, pa jos 4 stola tacno preko puta S5-S8
const PLAN: FloorPlan = {
  width: 900,
  height: 340,
  elements: [
    { kind: 'bar', x: 10, y: 10, w: 140, h: 30, text: 'Šank' },
    { kind: 'label', x: 10, y: 45, w: 140, h: 30, text: 'Shisha' },
    { kind: 'label', x: 450, y: 10, w: 110, h: 80, text: 'WC' },
    { kind: 'tv', x: 300, y: 105, w: 50, h: 6 },
    { kind: 'tv', x: 650, y: 105, w: 50, h: 6 },
    { kind: 'tv', x: 866, y: 230, w: 6, h: 60 },
    { kind: 'entrance', x: 4, y: 180, w: 36, h: 20, text: 'Ulaz' },
    { kind: 'entrance', x: 4, y: 280, w: 36, h: 20, text: 'Bašta' },
  ],
  tables: [
    { id: 'S1', label: 'S1', x: 70, y: 80, seats: 4, shape: 'round', status: 'free' },
    { id: 'S2', label: 'S2', x: 70, y: 150, seats: 8, shape: 'round', status: 'free' },
    { id: 'S3', label: 'S3', x: 70, y: 220, seats: 4, shape: 'round', status: 'taken' },
    { id: 'S4', label: 'S4', x: 340, y: 140, seats: 4, shape: 'square', status: 'free' },
    { id: 'S5', label: 'S5', x: 450, y: 140, seats: 4, shape: 'round', status: 'free' },
    { id: 'S6', label: 'S6', x: 520, y: 140, seats: 4, shape: 'round', status: 'pending' },
    { id: 'S7', label: 'S7', x: 590, y: 140, seats: 4, shape: 'round', status: 'free' },
    { id: 'S8', label: 'S8', x: 660, y: 140, seats: 4, shape: 'round', status: 'free' },
    { id: 'S9', label: 'S9', x: 730, y: 140, seats: 4, shape: 'round', status: 'taken' },
    { id: 'S10', label: 'S10', x: 800, y: 140, seats: 4, shape: 'round', status: 'free' },
    { id: 'S11', label: 'S11', x: 450, y: 260, seats: 4, shape: 'round', status: 'free' },
    { id: 'S12', label: 'S12', x: 520, y: 260, seats: 4, shape: 'round', status: 'free' },
    { id: 'S13', label: 'S13', x: 590, y: 260, seats: 4, shape: 'round', status: 'taken' },
    { id: 'S14', label: 'S14', x: 660, y: 260, seats: 4, shape: 'round', status: 'free' },
  ],
};

export const EVENTS: VenueEvent[] = [
  {
    id: 'poljska-bih',
    venueSlug: 'exclusive',
    title: 'Poljska vs Bosna i Hercegovina',
    subtitle: 'Utakmica na velikim ekranima',
    startsAt: '2026-09-25T20:45:00',
    packages: [
      { id: 'kibla-tuborg', name: 'Kibla Tuborg', persons: 4, price: 50, description: '6 × Tuborg, 1 × shisha, grickalice' },
      { id: 'kibla-somersby', name: 'Kibla Somersby', persons: 4, price: 50, description: '6 × Somersby bezalkoholni, 1 × shisha, grickalice' },
      { id: 'kibla-redbull', name: 'Kibla Red Bull', persons: 4, price: 60, description: '6 × Red Bull, 1 × shisha, grickalice' },
      { id: 'kibla-fast', name: 'Kibla Fast', persons: 4, price: 45, description: '6 × Fast, 1 × shisha, grickalice' },
    ],
    floorPlan: PLAN,
  },
];
