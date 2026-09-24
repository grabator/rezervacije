import { FloorPlan, FloorTable, Venue, VenueEvent } from './models';

export const VENUES: Venue[] = [
  {
    slug: 'exclusive',
    name: 'Exclusive Caffe Lounge',
    subtitle: 'Rezervacija stola',
    instagram: 'https://www.instagram.com/exclusivecaffe.lounge',
  },
];

/** Pomoćna funkcija: red stolova na istoj x-osi */
function column(prefix: string, start: number, x: number, ys: number[], statuses: Record<number, FloorTable['status']> = {}): FloorTable[] {
  return ys.map((y, i) => {
    const n = start + i;
    return { id: `${prefix}${n}`, label: `${prefix}${n}`, x, y, seats: 4, shape: 'round', status: statuses[n] ?? 'free' };
  });
}

// Primjer rasporeda (izmišljen) – zamijeni stvarnim rasporedom lokala
const PLAN: FloorPlan = {
  width: 400,
  height: 560,
  elements: [
    { kind: 'bar', x: 110, y: 20, w: 270, h: 34, text: 'Šank' },
    { kind: 'label', x: 20, y: 20, w: 80, h: 34, text: 'Shisha' },
    { kind: 'tv', x: 30, y: 66, w: 70, h: 6 },
    { kind: 'tv', x: 165, y: 66, w: 70, h: 6 },
    { kind: 'tv', x: 300, y: 66, w: 70, h: 6 },
    { kind: 'sofa', x: 14, y: 90, w: 18, h: 220 },
    { kind: 'sofa', x: 368, y: 90, w: 18, h: 440 },
    { kind: 'sofa', x: 190, y: 120, w: 20, h: 150 },
    { kind: 'entrance', x: 150, y: 540, w: 100, h: 14, text: 'Ulaz' },
  ],
  tables: [
    ...column('S', 1, 62, [110, 170, 230, 290], { 2: 'taken' }),
    ...column('S', 5, 150, [110, 170, 230], { 6: 'pending' }),
    ...column('S', 8, 250, [130, 190, 250], { 9: 'taken' }),
    ...column('S', 11, 335, [110, 170, 230, 290, 350, 410, 470], { 13: 'taken', 14: 'taken', 17: 'unavailable' }),
    ...column('S', 18, 110, [370, 430, 490], { 19: 'taken' }),
    ...column('S', 21, 230, [340, 410, 480]),
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
