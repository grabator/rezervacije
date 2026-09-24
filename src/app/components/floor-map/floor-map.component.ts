import { Component, computed, input, output, signal } from '@angular/core';
import { FloorPlan, FloorTable } from '../../core/models';

@Component({
  selector: 'app-floor-map',
  standalone: true,
  templateUrl: './floor-map.component.html',
  styleUrl: './floor-map.component.scss',
})
export class FloorMapComponent {
  plan = input.required<FloorPlan>();
  selectedId = input<string | null>(null);
  select = output<FloorTable>();

  zoom = signal(1);
  width = computed(() => `${this.zoom() * 100}%`);
  showList = signal(false);

  zoomIn() { this.zoom.update(z => Math.min(2.5, +(z + 0.5).toFixed(1))); }
  zoomOut() { this.zoom.update(z => Math.max(1, +(z - 0.5).toFixed(1))); }

  stateOf(t: FloorTable): string {
    return t.id === this.selectedId() ? 'selected' : t.status;
  }

  onTable(t: FloorTable) {
    if (t.status === 'free') this.select.emit(t);
  }

  hitR(t: FloorTable): number { return (t.size || 48) / 2; }
  topR(t: FloorTable): number { return this.hitR(t) * 0.75; }
  chairOffset(t: FloorTable): number { return this.hitR(t) * 0.8; }
  chairR(t: FloorTable): number { return this.hitR(t) * 0.2; }
  fontSize(t: FloorTable): number { return Math.max(9, this.hitR(t) * 0.42); }

  ariaLabel(t: FloorTable): string {
    const map = { free: 'slobodan', pending: 'na čekanju', taken: 'zauzet', unavailable: 'nedostupan' } as const;
    return `Sto ${t.label}, ${t.seats} mjesta, ${t.id === this.selectedId() ? 'izabran' : map[t.status]}`;
  }
}
