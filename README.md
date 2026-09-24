# Rezervacije stolova (Angular)

Mobilna web stranica za rezervaciju stolova za evente, npr. link iz Instagram bio-a:
`https://tvoj-domen.ba/r/exclusive/rezervacije`

## Pokretanje
```bash
npm install
npx ng serve
```
Otvori http://localhost:4200 (u Chrome DevTools uključi mobilni prikaz, Ctrl+Shift+M).

## Rute
- `/r/:venue/rezervacije` lista eventova lokala
- `/r/:venue/rezervacije/e/:eventId` event: mapa stolova, paketi, forma

## Struktura
- `src/app/core/models.ts` modeli (Venue, VenueEvent, FloorPlan, FloorTable, TablePackage...)
- `src/app/core/mock-data.ts` probni podaci i raspored sale (x/y koordinate stolova)
- `src/app/core/reservation.service.ts` servis; trenutno mock, kasnije HttpClient prema API-ju
- `src/app/components/floor-map` SVG mapa sale sa statusima i zoomom
- `src/app/pages/event` stranica eventa i forma
- `src/app/pages/event-list` lista eventova

## Sljedeći koraci
1. Backend (.NET Web API + SQL Server): Venues, Events, Tables, Packages, Reservations
2. Na serveru zaključavanje stola (status Pending) da nema duple rezervacije
3. Admin panel: potvrda/odbijanje i email gostu
4. Editor rasporeda sale za admina (drag & drop stolova)
5. Deploy (Netlify/Vercel/Azure) i link u Instagram bio
