# IS-202 vurderingsbevis - teknisk dokumentasjon

Dette dokumentet supplerer README og øvrige fagnotater for å dekke alle vurderingskriterier i IS-202-prosjektet. Innholdet peker direkte til kode (kontrollere, modeller, views), Docker-oppsettet og testene slik at sensor kan verifisere implementasjonen.

## 1. ASP.NET MVC i Docker

| Del | Detaljer |
| --- | --- |
| Prosjektstruktur | `Controllers/` (MVC-endepunkter), `Models/` (domenemodeller og viewmodeller), `Views/` (Razor med Tailwind/Leaflet) - se f.eks. `ObstacleController` som bruker `ObstacleData` og rendrer `Dataform.cshtml`. |
| Docker-komposisjon | `docker-compose.yml` starter `mariadb` (port 3308→3306, vedvarende volum `mariadb_data`) og webappen `firstwebapplication1` (port 5010→8080) med miljøvariabel for connection string. MariaDB og appen ligger på samme bridge-nettverk `appnet`. |
| Kjøring | **Visual Studio:** velg profilen *Docker Compose* og trykk F5 - VS bygger containerne og kjører appen på http://localhost:5010. |

## 2. God praksis, MVC-logikk og bruk av GET/POST

- **GET-eksempel:** `ObstacleController.SelectType()` returnerer et view med `ObstacleTypeViewModel` og krever innlogging (`[Authorize]`).
- **POST med validering/antiforgery/PRG:** `ObstacleController.DataForm(ObstacleData obstacledata, bool? useFeet)` bruker `[HttpPost]`, `[ValidateAntiForgeryToken]`, ModelState-validering, lagrer data og `RedirectToAction(nameof(Overview), …)` for å følge PRG-mønsteret.
- **Modellbinding:** `[Bind("Id, ObstacleName, ObstacleHeight, ObstacleDescription, Longitude, Latitude, LineGeoJson")]` på `Edit`-POST begrenser hvilke felt som hentes fra skjemaet og kopieres inn i `obstacledata`-instansen.
- **Rate limiting:** `[EnableRateLimiting("Fixed")]` på `ObstacleController` peker til policyen konfigurert i `Program.cs` (10 forespørsler/10 sek med kølimit 5).

## 3. Kart og skjema (Leaflet)

- **Henting av kartdata:** `DataForm`-GET henter godkjente hindre med LINQ (`Where(o => o.Status == "Approved")`) og serialiserer dem til JSON for kartet (`ViewBag.ApprovedObstaclesJson`). `PilotController.GetApprovedObstacles` eksponerer både `Approved` og `Pending` som JSON-API for pilotkartet.
- **JSON til view:** `Dataform.cshtml` setter `data-approved-obstacles='@(ViewBag.ApprovedObstaclesJson ?? "[]")'` på kart-diven slik at klientskriptet kan lese GeoJSON/koordinater.
- **Totrinns flyt:** 1) `SelectType` (velg hindertype og lagre i TempData) → 2) `DataForm` (registrer koordinater med Leaflet og høyde) → 3) `Overview` (kvittering). Pilotkartet (`/Pilot/Map`) er en parallell leseflyt.

## 4. Database: EF Core + MariaDB

- **Modeller:** `ObstacleData` definerer felter for høyde, geometri (`Longitude`, `Latitude`, `LineGeoJson`), status og metadata (`SubmittedBy`, `ApprovedBy`, etc.).
- **Migrations:** `Program.cs` kjører `db.Database.MigrateAsync()` ved oppstart. En migrasjon som `20251104163957_AddObstacleStatusTracking` legger til statushistorikk-kolonner.
- **GeoJSON-lagring:** `LineGeoJson` er konfigurert som `longtext` i modellen og migrasjoner; `LineCoordinates`/`ParseLine` parser GeoJSON for å validere linjer.
- **Connection string:** `appsettings.json` definerer `ConnectionStrings:DefaultConnection` med MariaDB-host `mariadb`, port 3308 (host), database `ObstacleDb`, root-passord og timeouter.

## 5. SQL-spørringer i koden

- **LINQ-filter i listevisning:** `ObstacleController.List` bygger et `obstaclesQuery` med `Where`-filtre for status, organisasjon, type, fritekst, høydeintervall og datointervall før `OrderByDescending(o => o.SubmittedDate).ToListAsync()`. Dette gir server-side filtrering uten rå SQL og understøtter søk i hinderlisten.

## 6. Tester

- **Dekning:** `FirstWebApplication1.Tests` bruker xUnit/Moq og et InMemory-DbContext for å verifisere controllerlogikk: flyt mellom SelectType → DataForm, valideringsfeil, godkjenning/avslag og sletting.
- **Eksempeltest:** `Approve_ExistingObstacle_ChangesStatusAndRedirectsToList` oppretter et hinder i minnedatabasen, kaller `Approve`, og bekrefter at status settes til `Approved`, `ApprovedBy` matcher bruker og at resultatet er `RedirectToAction("List")`.

## 7. Sikkerhet (SQL injection, XSS, CSRF)

- **CSRF:** POST-metoder bruker `[ValidateAntiForgeryToken]`, og skjemaene har `@Html.AntiForgeryToken()` i Razor-views (f.eks. `Dataform.cshtml`).
- **SQL injection:** EF Core bruker parameteriserte kommandoer når LINQ-spørringer oversettes til SQL; connection string peker til MariaDB-driveren (Pomelo) og unngår manuell strengkonkatenering.
- **XSS:** Razor HTML-encoder variabler som standard; verdier som `@Model.ObstacleDescription` eller `@notificationMessage` renderes trygt med automatisk encoding.
- **Innlogging/Identity:** `Program.cs` konfigurerer `AddIdentity<IdentityUser, IdentityRole>()` med passordpolicy og token providers. Identity håndterer brukere/roller, og autentiseringsmiddleware aktiveres via `app.UseAuthentication()`/`UseAuthorization()`.

## 8. Autentisering og autorisasjon

- **Roller:** `Admin`, `Caseworker`, `Pilot` seedes ved oppstart i `Program.cs` (roles array). Admin-bruker opprettes hvis `Admin:Email`/`Admin:Password` er satt.
- **[Authorize]-eksempler:**
  - `[Authorize(Roles = "Pilot,Caseworker,Admin")]` på `ObstacleController.List/Details/Edit` beskytter hinderoversikt og endring.
  - `[Authorize(Roles = "Pilot")]` på `PilotController` begrenser kart-API-et til piloter.
- **Seeding:** Etter migrasjoner henter koden `RoleManager` og oppretter roller dersom de mangler; deretter opprettes/roller-tilordnes admin-bruker basert på konfigurasjon.

## 9. Frontend og rammeverk

- **Tailwind via CDN:** `_Layout.cshtml` laster `https://cdn.tailwindcss.com` og definerer nav/knapper med Tailwind-klasser.
- **Leaflet-integrasjon:** `Dataform.cshtml` og `Pilot/Map.cshtml` inkluderer Leaflet CSS/JS fra CDN og initialiserer kartet via lokale skript (`~/js/geolocation.js`, `~/js/pilot-map.js`).
- **MVC-views:** Razor bruker modellbinding (`@model ObstacleData`), `ViewBag` for rolleinfo og `asp-`-taghelpers for lenker/validering, i tråd med MVC-strukturen.

## 10. Database i Docker

- **Compose-utdrag:**
  - MariaDB: port `3308:3306`, volum `mariadb_data:/var/lib/mysql`, miljøvariabler for root-passord og database.
  - Webapp: port `5010:8080`, avhengig av `mariadb`, connection string sendt via miljøvariabel.
- **Nettverk:** Begge tjenester ligger på `appnet` (bridge) slik at connection string bruker host `mariadb` og port 3306 internt, mens 3308 eksponeres til verten.
