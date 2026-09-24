// Produkcija (ng build). Pretpostavlja da su frontend i backend na ISTOM domenu
// (npr. Nginx/reverse proxy prosljeđuje /api na .NET backend) - tako nema CORS
// glavobolje. Ako backend bude na drugom domenu, promijeni apiBase ovdje i
// napravi novi build.
export const environment = {
  production: true,
  apiBase: '/api',
};
