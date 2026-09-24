import { FloorPlan, FloorTable, Venue, VenueEvent } from './models';

export const VENUES: Venue[] = [
  {
    slug: 'exclusive',
    name: 'Exclusive Caffe Lounge',
    subtitle: 'Rezervacija stola',
    instagram: 'https://www.instagram.com/exclusivecaffe.lounge',
  },
];

/** Pomoćna funkcija: red stolova na istoj y-osi (horizontalni raspored sale) */
function row(
  prefix: string,
  start: number,
  y: number,
  xs: number[],
  statuses: Record<number, FloorTable['status']> = {},
  shapes: Record<number, FloorTable['shape']> = {},
): FloorTable[] {
  return xs.map((x, i) => {
    const n = start + i;
    const shape = shapes[n] ?? 'round';
    return { id: `${prefix}${n}`, label: `${prefix}${n}`, x, y, seats: shape === 'square' ? 2 : 4, shape, status: statuses[n] ?? 'free' };
  });
}

// Stvarni raspored sale (po skici vlasnika) – ulaz i bašta lijevo, šank/shisha uz ulaz,
// WC/igrice/stepenice u suprotnom uglu, stolovi popunjavaju salu između.
const PLAN: FloorPlan = {
  width: 760,
  height: 400,
  elements: [
    { kind: 'label', x: 40, y: 20, w: 120, h: 34, text: 'Shisha' },
    { kind: 'bar', x: 180, y: 20, w: 460, h: 34, text: 'Šank' },
    { kind: 'sofa', x: 4, y: 60, w: 14, h: 80 },
    { kind: 'entrance', x: 4, y: 170, w: 70, h: 20, text: 'Ulaz' },
    { kind: 'entrance', x: 4, y: 320, w: 70, h: 20, text: 'Bašta' },
    { kind: 'sofa', x: 750, y: 60, w: 14, h: 220 },
    { kind: 'label', x: 640, y: 230, w: 110, h: 60, text: 'Igrice' },
    { kind: 'label', x: 640, y: 300, w: 110, h: 76, text: 'WC' },
  ],
  tables: [
    ...row('S', 1, 100, [200, 280, 360, 440, 520, 600], { 2: 'taken', 6: 'pending' }, { 1: 'square', 2: 'square' }),
    ...row('S', 7, 180, [200, 280, 360, 440, 520, 600, 680], { 9: 'taken', 13: 'taken' }),
    ...row('S', 14, 260, [200, 280, 360, 440, 520, 600], { 14: 'taken', 17: 'unavailable', 19: 'taken' }),
    ...row('S', 20, 340, [200, 280, 360, 440]),
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
