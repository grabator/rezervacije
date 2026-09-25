// Produkcija (ng build). Frontend i backend su odvojeni Railway servisi na
// razlicitim domenima, pa apiBase mora biti apsolutan URL backend servisa
// (CORS je podesen na backendu preko AllowedOrigins).
export const environment = {
  production: true,
  apiBase: 'https://backend-production-e7d3.up.railway.app/api',
};
