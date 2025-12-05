# Systemarkitektur

## Komponenter
- **Webapplikasjon (ASP.NET Core MVC)**
  - Razor Views med Tailwind CSS og noen få JavaScript-biblioteker (Leaflet for kart, jQuery for validering).
  - MVC-kontrollere for pålogging/registrering, hinderregistrering og rolleadministrasjon.
- **Autentisering og autorisasjon**
  - ASP.NET Core Identity med rollestyring (`Admin`, `Caseworker`, `Pilot`).
  - Seed av roller og admin-bruker i oppstart (`Program.cs`).
- **Data- og peristeringslag**
  - Entity Framework Core med MariaDB (Pomelo-driver).
  - Migrasjoner opprettes/kjøres ved oppstart av webapplikasjonen.
- **Presentasjon/klient**
  - Responsiv layout levert via Tailwind CDN, med font- og ikonstøtte fra Font Awesome.
  - Leaflet-kart for å vise registrerte hindre (punkt og linje). Pilot-kartet laster både godkjente og ventende hindre, mens registreringsskjemaet viser godkjente hindre for referanse.
- **Sikkerhetslag**
  - Rate limiting (fast vindu) på kontrolleren for hinder (`[EnableRateLimiting("Fixed")]`).
  - Standard antiforgery på POST-handlinger; HTTPS/HSTS aktivt utenfor utvikling. Ingen ekstra sikkerhetshoder er konfigurert i koden utover rammeverkets default.

## Flyt gjennom systemet
1. **Innlogging/registrering**
   - Brukere registrerer seg med ønsket rolle (pilot eller saksbehandler). Admin opprettes via konfigurasjon.
   - Identitet lagres i databasen via EF Core.
2. **Rapportering av hinder**
   - Brukeren velger hindertype, fyller ut skjemaet (inkludert karttegning) og sender inn. Data valideres server-side og lagres som `Pending`.
   - Et oversiktsbilde viser registrert hinder og kvittering.
3. **Behandling**
   - Autoriserte roller (`Caseworker`/`Admin`) kan filtrere og oppdatere status på hindre, inkludert historikk for sist endret og godkjent/avslått.
4. **Visning**
   - Pilot-kartet viser godkjente og ventende hindre som JSON for Leaflet. Listevisningen filtrerer og sorterer alle hindre med server-side querying.

## Database
- MariaDB kjører i egen container via Docker Compose.
- EF Core-migrasjoner lager Identity-tabeller, hinder-tabeller og statuslogg.
- Tilkoblingsstreng `DefaultConnection` leses fra konfigurasjon (`appsettings.json`/miljøvariabler).

## Roller og tilgang
- **Admin**: Bruker- og rolleadministrasjon, full tilgang til hinder og statusoppdateringer.
- **Caseworker**: Behandler/oppdaterer hinder, kan endre status og se oversikter.
- **Pilot**: Kan registrere og se egne/godkjente hinder, få kartoversikt.

## Distribusjon og drift
- Lokalt utviklingsoppsett via Visual Studio: Docker Compose starter database og webapp.
- Miljøvariabler for admin-oppstart lagres i `appsettings.json` eller secrets/store.
- Standard port i lokal utvikling: `http://localhost:5010`.

