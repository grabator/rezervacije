# Rezervacije stolova

Mobilna web stranica za rezervaciju stolova za evente, npr. link iz Instagram bio-a:
`https://tvoj-domen.ba/r/exclusive/rezervacije`

Frontend (Angular) + backend (.NET Web API + SQLite) sa pravom bazom i admin panelom za potvrdu/odbijanje rezervacija.

## Pokretanje (treba oba servera istovremeno)

**1. Backend** (u jednom terminalu):
```bash
cd api/Rezervacije.Api
dotnet run --urls http://localhost:5080
```
Prvi put kad se pokrene sam napravi bazu (`rezervacije.db`) i ubaci probne podatke (lokal, event, raspored sale, paketi).

**2. Frontend** (u drugom terminalu):
```bash
npm install
npx ng serve
```
Otvori http://localhost:4200 (u Chrome DevTools uključi mobilni prikaz, Ctrl+Shift+M).

## Rute

- `/r/:venue/rezervacije` — lista eventova lokala
- `/r/:venue/rezervacije/e/:eventId` — event: mapa stolova, paketi, forma
- `/r/:venue/admin` — admin panel (lozinka: vidi `api/Rezervacije.Api/appsettings.Development.json` → `AdminPassword`)

## Struktura

**Frontend** (`src/app/`)
- `core/models.ts` — modeli (Venue, VenueEvent, FloorPlan, FloorTable, TablePackage...)
- `core/reservation.service.ts` — zove pravi API (`core/api-config.ts` ima baznu adresu)
- `core/admin.service.ts` — admin pozivi (lista, potvrdi, odbij) sa lozinkom u headeru
- `components/floor-map` — SVG mapa sale sa statusima i zoomom
- `pages/event`, `pages/event-list` — stranice za gosta
- `pages/admin` — admin panel (login lozinkom + lista zahtjeva)

**Backend** (`api/Rezervacije.Api/`)
- `Models/Entities.cs` — EF Core entiteti (Venue, VenueEvent, TablePackage, FloorTableEntity, FloorElementEntity, Reservation)
- `Data/AppDbContext.cs`, `Data/SeedData.cs` — baza (SQLite) i probni podaci (stvarni raspored sale)
- `Program.cs` — svi API endpoint-i (minimal API):
  - `GET /api/venues/{slug}`
  - `GET /api/venues/{slug}/events`
  - `GET /api/events/{id}`
  - `POST /api/reservations` — server provjerava da je sto slobodan prije nego ga zaključa (status `pending`), zaštita od duple rezervacije
  - `GET /api/admin/reservations`, `POST /api/admin/reservations/{id}/confirm|reject` — zaštićeno headerom `X-Admin-Password`

## Editor rasporeda sale

Ako treba mijenjati raspored sale (stolovi, šank, WC, TV...), postoji interaktivni alat gdje se stolovi/zone prevlače na tačno mjesto i izvezu koordinate — javi pa se ponovo objavi link, ili direktno izmijeni `Data/SeedData.cs` (i obriši `rezervacije.db` da se baza ponovo napravi sa novim podacima).

## Sljedeći koraci

1. ~~Backend (.NET Web API + baza): Venues, Events, Tables, Packages, Reservations~~ ✅
2. ~~Na serveru zaključavanje stola (status Pending) da nema duple rezervacije~~ ✅
3. ~~Admin panel: potvrda/odbijanje~~ ✅ (email gostu o potvrdi — nije još dodano)
4. Deploy (backend + frontend na pravi domen) i link u Instagram bio
5. Upravljanje eventima bez izmjene koda (trenutno je jedan event hardkodiran u `SeedData.cs`)
6. Editor rasporeda sale ugrađen u admin panel (trenutno je poseban alat)
