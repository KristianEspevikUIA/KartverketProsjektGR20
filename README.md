# FirstWebApplication1 – IS-202 Programmeringsprosjekt (Høst 2025)

FirstWebApplication1 er en ASP.NET Core MVC-applikasjon utviklet som del av IS-202 Programmeringsprosjekt (Høst 2025) ved Universitetet i Agder.

Applikasjonen lar innloggede brukere registrere luftfartshindre gjennom et totrinnsskjema (velg type → registrer detaljer med kartdatainput) og se innsendte registreringer i en sorterbar/filtrerbar liste. Piloter kan også åpne et Leaflet-kart som viser både ventende og godkjente innsendelser for bedre situasjonsforståelse.

Løsningen er **utelukkende tiltenkt kjøring i Visual Studio** via det medfølgende Docker Compose-oppsettet, som starter både ASP.NET Core 9-appen og MariaDB-databasen fra nedtrekket for debug-profiler. Kjøring via CLI eller andre IDE-er er ikke støttet.

## Mappe- og prosjektstruktur
- `FirstWebApplication1/` – ASP.NET Core MVC-app (kontrollere, modeller, Razor-views, statiske ressurser).
- `FirstWebApplication1.Tests/` – testprosjekt for enhets-/integrasjonstester.
- `docs/` – utfyllende dokumentasjon (arkitektur, sikkerhet, testing, mobil).
- Docker Compose-filer – starter MariaDB- og app-containere i utvikling.

## Teknologi og nøkkelfunksjoner
- ASP.NET Core 9 MVC med Identity og EF Core (Pomelo MariaDB-driver)
- Tailwind CSS via CDN og Leaflet-kart (viser godkjente **og** ventende hindre)
- Rollebasert tilgang (`Admin`, `Caseworker`, `Pilot`) med rate limiting på `ObstacleController`
- HTTPS/HSTS utenfor utvikling, standard antiforgery på POST-aksjoner, Razor-encoding mot XSS og EF Core-parameterisering mot SQL-injection; ingen ekstra sikkerhetshoder er satt i koden
- WCAG-godkjente primærknapper på landingssider etter brukertesting (tilstrekkelig kontrast, fokusmarkering og god treffflate)

## Slik kjører du prosjektet i Visual Studio (kun støttet modus)
Bruk Visual Studio med Docker Compose for å starte hele løsningen fra `.sln`-fila. Installer følgende først:

- Visual Studio (arbeidsbelastning for ASP.NET og webutvikling)
- .NET 9 SDK
- Docker Desktop

Trinn:
1. Klon repoet: `git clone https://github.com/KristianEspevikUIA/KartVerketProsjektGR20.git` og åpne `FirstWebApplication1.sln` i Visual Studio.
2. I verktøylinja i Visual Studio velger du **Docker Compose** fra nedtrekksmenyen for kjøreprofiler.
3. Trykk **F5** (eller den grønne **Start**-knappen). Visual Studio bygger containerne og starter appen sammen med MariaDB-tjenesten definert i `docker-compose.dcproj`.
4. Når containerne er ferdig startet, er appen tilgjengelig på http://localhost:5010.

> **Ikke støttet:** Vi tilbyr ikke CLI-basert oppstart (`dotnet run`/`docker compose up`) eller kjøring fra andre IDE-er. Eventuelle avvik fra Visual Studio-arbeidsflyten er på egen risiko og dokumenteres ikke.
 
## Kjøring og deploy (fremtidig dokumentasjon)
Vi har forsøkt å beskrive en CLI-basert «Kjøring og deploy»-flyt (f.eks. `dotnet ef database update`, `docker compose up`, miljøvariabler og helse-sjekker, samt en kort produksjonsguide med reverse proxy, logging og HSTS/CSP-konfigurasjon). Dette ble satt på vent fordi kombinasjonen av Leaflet via CDN og strengere sikkerhetshoder skapte kompatibilitetsutfordringer som vi ikke rakk å løse. Seksjonen legges til ved en senere oppdatering når vi har avklart anbefalt oppstart utenfor Visual Studio og kan gi verifiserte kommandoer og sikkerhetsoppsett.

## Prosjektoppsett
### Docker-bakgrunnstjenester
- En MariaDB-container for lagring av hinderdata
- En ASP.NET Core 9-container for å kjøre applikasjonen (starter automatisk via Visual Studio/Docker Compose)

### Admin-kontooppsett
Roller (`Admin`, `Pilot`, `Caseworker`) seedes ved oppstart. En admin-bruker opprettes kun når følgende konfigurasjonsverdier er satt:

- `Admin:Email` – admin-brukernavn (`admin@kartverket.no`)
- `Admin:Password` – admin-passord (`Admin123`)

Hvis verdiene mangler, logger applikasjonen et varsel og ingen admin-bruker opprettes. Kun den konfigurerte admin-e-posten kan bli administrator; den offentlige registreringen eksponerer kun Pilot- og Caseworker-roller.

## Hvordan systemet fungerer (høyt nivå)
- Brukere registrerer/logger inn via ASP.NET Identity. Kun forhåndskonfigurert e-post kan bli Admin; andre brukere velger Pilot/Caseworker.
- Hinderflyt: velg hindertype → fyll ut skjema (inkludert karttegning) → innsending lagres som `Pending` → kvittering vises.
- Behandling: Caseworker/Admin filtrerer hindre, oppdaterer status (Approved/Declined/Pending) og ser endringsmetadata.
- Synlighet: Godkjente og ventende hindre eksponeres som JSON til pilotkartet, mens listevisningen er rollebeskyttet (Pilot/Caseworker/Admin) med filtrering på status, datoperiode, høyde, type og organisasjon.

## Midlertidig håndtering av passord i repoet (kun for sensur)
- Vi har **bevisst sjekket inn databasepassord og admin-passord** i Git for å forenkle oppsettet under sensur.
- Dette er **ikke en anbefalt praksis** og bryter med våre egne retningslinjer om å bruke miljøvariabler/User Secrets for hemmeligheter.
- Etter at prosjektet er ferdig vurdert vil passordene roteres, flyttes til secrets og slettes fra historikken for å gjenopprette sikkerhetsnivået.

## Funksjoner i applikasjonen
- Et ryddig hinderegistreringsskjema som fanger høyde, lokasjon, koordinater, kategori og metadata
- En resultat-/kvitteringsvisning som viser innsendt informasjon
- Et tabelloversiktsbilde av rapporterte hindre med filtre for status, datoer, høyde, type, organisasjon og fritekst
- Et Leaflet-basert interaktivt kart som viser posisjoner (punkt eller tegnet linje) for godkjente og ventende hindre
- Støtte for konvertering mellom feet/meter basert på brukerrolle
- Tilgjengelighetsjusterte (WCAG) primærknapper på landingssider med tydelig fokuslinje og fargekontrast
- En enkel og utvidbar arkitektur for videre utvikling gjennom IS-202-kurset

## Pilot-oversikt over hindre
Piloter har to dedikerte innganger for situasjonsforståelse:

- **Pilotkart (`/Pilot/Map`)** – laster Leaflet med godkjente og ventende hindre fra `PilotController.GetApprovedObstacles`, og viser både punkter og valgfri linjegeometri. En flytende knapp linker direkte til registreringsflyten slik at piloter kan rapportere nye funn.
- **Hinderliste (`/Obstacle/List`)** – rollebeskyttet for Pilot, Caseworker og Admin, med filtrering på status, type, fritekst, datointervall, høyde og organisasjon. Hver rad lenker til detalj- og endrehandlinger og gir piloter en tydelig, filtrerbar oversikt over alle lagrede hindre.


## Prosjektformål og kontekst
Prosjektet ble utviklet for IS-202 Programmeringsprosjekt, der studentene skal bygge en fungerende programvareløsning basert på gitte krav. Gruppen implementerte et hinder-rapporteringssystem inspirert av prosesser hos Kartverket og Norsk Luftambulanse. Applikasjonen støtter opprettelse av nye hinderrapporter, forvaltning av dem og visning av data i dynamiske grensesnitt som tabeller og kart.

Alle funksjoner er utviklet i fellesskap av gruppen, inkludert skjemavalidering, datahåndtering, UI-tilpasninger, statushistorikk og kartintegrasjon. Tilgjengelighetsforbedringene på landingssider ble lagt til etter brukertesting med pilotbruker.

## 📄 Dokumentasjon
- [Systemarkitektur](docs/architecture.md)
- [Mobiltilpasning](docs/mobile.md)
- [Testing og testresultater](docs/testing.md)
- [Vurderingskriterier](docs/vurderingskriterier.md)

## Team
Dette prosjektet ble utviklet av Gruppe 20:
- Nicolai Stephansen
- Brage Kristoffersen
- Endi Muriqi
- Kristian Espevik
- Rune Kvame
- Victor Ziadpour
