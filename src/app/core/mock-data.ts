import { FloorPlan, FloorTable, Venue, VenueEvent } from './models';

export const VENUES: Venue[] = [
  {
    slug: 'exclusive',
    name: 'Exclusive Caffe Lounge',
    subtitle: 'Rezervacija stola',
    instagram: 'https://www.instagram.com/exclusivecaffe.lounge',
  },
];

// Stvarni raspored sale — složen ručno u editoru rasporeda od strane vlasnika (tacne koordinate).
const PLAN: FloorPlan = {
  width: 900,
  height: 620,
  elements: [
    { kind: 'bar', x: 10, y: 81, w: 140, h: 30, text: 'Šank' },
    { kind: 'label', x: 8, y: 7, w: 140, h: 30, text: 'Shisha' },
    { kind: 'label', x: 324, y: 105, w: 110, h: 80, text: 'WC' },
    { kind: 'tv', x: 48, y: 410, w: 50, h: 6 },
    { kind: 'tv', x: 880, y: 483, w: 6, h: 60 },
    { kind: 'entrance', x: 173, y: 420, w: 36, h: 20, text: 'Ulaz' },
    { kind: 'entrance', x: 176, y: 591, w: 36, h: 20, text: 'Bašta' },
    { kind: 'label', x: 207, y: 110, w: 90, h: 30, text: 'Igrice' },
    { kind: 'label', x: 772, y: 258, w: 90, h: 30, text: 'Igrice' },
    { kind: 'tv', x: 278, y: 356, w: 50, h: 6 },
    { kind: 'tv', x: 598, y: 356, w: 50, h: 6 },
  ],
  tables: [
    { id: 'S1', label: 'S1', x: 66, y: 165, seats: 4, shape: 'round', status: 'free' },
    { id: 'S2', label: 'S2', x: 67, y: 235, seats: 3, shape: 'round', status: 'free' },
    { id: 'S3', label: 'S3', x: 71, y: 373, seats: 3, shape: 'round', status: 'taken' },
    { id: 'S4', label: 'S4', x: 71, y: 305, seats: 3, shape: 'square', status: 'free' },
    { id: 'S5', label: 'S5', x: 252, y: 159, seats: 4, shape: 'round', status: 'free' },
    { id: 'S6', label: 'S6', x: 494, y: 235, seats: 4, shape: 'round', status: 'pending' },
    { id: 'S7', label: 'S7', x: 588, y: 235, seats: 4, shape: 'round', status: 'free' },
    { id: 'S8', label: 'S8', x: 667, y: 236, seats: 4, shape: 'round', status: 'free' },
    { id: 'S9', label: 'S9', x: 748, y: 237, seats: 4, shape: 'round', status: 'taken' },
    { id: 'S10', label: 'S10', x: 830, y: 236, seats: 4, shape: 'round', status: 'free' },
    { id: 'S11', label: 'S11', x: 382, y: 358, seats: 4, shape: 'round', status: 'free' },
    { id: 'S12', label: 'S12', x: 526, y: 360, seats: 4, shape: 'round', status: 'free' },
    { id: 'S13', label: 'S13', x: 681, y: 360, seats: 4, shape: 'round', status: 'taken' },
    { id: 'S14', label: 'S14', x: 830, y: 361, seats: 4, shape: 'round', status: 'free' },
    { id: 'S15', label: 'S15', x: 263, y: 239, seats: 4, shape: 'round', status: 'free' },
    { id: 'S16', label: 'S16', x: 73, y: 460, seats: 4, shape: 'round', status: 'free' },
    { id: 'S17', label: 'S17', x: 76, y: 562, seats: 4, shape: 'round', status: 'free' },
    { id: 'S18', label: 'S18', x: 311, y: 461, seats: 4, shape: 'round', status: 'free' },
    { id: 'S19', label: 'S19', x: 446, y: 460, seats: 4, shape: 'round', status: 'pending' },
    { id: 'S20', label: 'S20', x: 589, y: 460, seats: 4, shape: 'round', status: 'free' },
    { id: 'S21', label: 'S21', x: 727, y: 461, seats: 4, shape: 'round', status: 'free' },
    { id: 'S22', label: 'S22', x: 316, y: 571, seats: 4, shape: 'round', status: 'free' },
    { id: 'S23', label: 'S23', x: 455, y: 574, seats: 4, shape: 'round', status: 'free' },
    { id: 'S24', label: 'S24', x: 591, y: 571, seats: 4, shape: 'round', status: 'taken' },
    { id: 'S25', label: 'S25', x: 727, y: 574, seats: 4, shape: 'round', status: 'free' },
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
