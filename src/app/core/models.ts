export type TableStatus = 'free' | 'pending' | 'taken' | 'unavailable';

export interface Venue {
  slug: string;
  name: string;
  subtitle: string;
  instagram?: string;
}

export interface FloorTable {
  id: string;
  label: string;
  x: number;           // centar stola u koordinatama mape (viewBox)
  y: number;
  size: number;         // precnik stola na mapi (default 48)
  seats: number;
  shape: 'round' | 'square';
  status: TableStatus;
}

/** Statični dijelovi sale: zidovi, šank, TV, ulaz, sjedalice uz zid */
export interface FloorElement {
  kind: 'bar' | 'tv' | 'sofa' | 'entrance' | 'label';
  x: number;
  y: number;
  w: number;
  h: number;
  text?: string;
}

export interface FloorPlan {
  width: number;
  height: number;
  elements: FloorElement[];
  tables: FloorTable[];
}

export interface VenueEvent {
  id: string;
  venueSlug: string;
  title: string;
  subtitle: string;
  startsAt: string;     // ISO datum
  imageUrl?: string;
  floorPlan: FloorPlan;
}

export interface ReservationRequest {
  eventId: string;
  tableId: string;
  fullName: string;
  phone: string;
  note?: string;
  hp?: string;
}

export interface ReservationResult {
  reservationId: string;
  status: 'pending';
}

export interface ReservationStatus {
  id: string;
  status: 'pending' | 'confirmed' | 'rejected' | 'cancelled';
  eventTitle: string;
  tableLabel: string;
  createdAt: string;
}
