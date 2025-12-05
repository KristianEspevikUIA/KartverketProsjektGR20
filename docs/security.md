# Sikkerhetsdokumentasjon

## Oversikt over tiltak
- **Autentisering/autorisasjon:** ASP.NET Core Identity med roller (`Admin`, `Caseworker`, `Pilot`). Registrering av Admin begrenses til forhåndsdefinert e-post i konfigurasjon.
- **Ressurskontroll:** Rate limiting (10 requests / 10 sek, med kø på 5), og tilgangskrav per rolle på handlinger.
- **HTTPS/HSTS:** Aktiv ved ikke-utviklingsmiljø.
- **CSRF-beskyttelse:** `[ValidateAntiForgeryToken]` på POST-handlinger i registreringsflyten for hinder og brukerinnlogging.
- **XSS-beskyttelse:** Razor view-engine HTML-encoder utdata som standard. Kart- og tabellvisninger bruker ikke `Html.Raw` eller user-generert scriptinnstikk. Vi forsøkte å innføre en streng Content Security Policy, men regelene blokkerte Leaflet-lasten fra CDN og brøt kartvisningen. På grunn av tidsmangel rakk vi ikke å finjustere eller whitelist’e Leaflet-kildene, men vi erkjenner at en korrekt konfigurert CSP er svært viktig å få på plass.
- **SQL-injection-beskyttelse:** Databasetilgang går gjennom Entity Framework Core (parameteriserte queries). Ingen manuelle SQL-strenger brukes i koden.

## Misbruksscenarier og mitigering
- **Pilot forsøker å få admin-tilgang:** Admin-registrering er eksplisitt sperret i `AccountController` og admin-rollene tildeles kun via seed/eksisterende admin.
- **Brute force/skript-spam:** Rate limiting på hinderkontrolleren reduserer effekten av automatiserte innsendinger. Identity-passordkrav gir minimumssikring.
- **Clickjacking:** Ingen egendefinerte anti-iframe-headere er satt; vurder å legge til `X-Frame-Options`/`Frame-Options` ved produksjonssetting.
- **Dataeksfiltrasjon via third-party scripts:** Ingen CSP er konfigurert i koden per nå. Tailwind/Leaflet/Font Awesome lastes via CDN, så CSP bør vurderes før produksjon.
- **Privilege escalation ved rollemisbruk:** Autorisasjonsattributter på controller-/action-nivå håndhever at kun riktige roller kan opprette, endre og godkjenne hinder.
- **XSS via uventet renderingsvei:** Razor encoding reduserer risikoen, men mangel på CSP og tillit til CDN-innhold betyr at en supply-chain hendelse kan få injisert script. Innfør CSP og vurder subresource integrity.
- **SQL-injection via manuelle spørringer:** Nåværende kodebase bruker EF Core; dersom rå SQL introduseres må parameterisering og `FromSqlRaw`-guardrails følges.

## Operasjonelle rutiner
- **Konfigurasjon av admin-konto:** Sett `Admin:Email` og `Admin:Password` som miljøvariabler eller i secrets før oppstart.
- **Databasenøkler:** Tilkoblingsstrengen `DefaultConnection` må settes via user-secrets eller miljø for å unngå sjekk-in av hemmeligheter.
- **Oppdatering av eksterne CDN-pekere:** Ved oppdatering av Tailwind/FontAwesome/jQuery bør CSP-listen oppdateres tilsvarende.

## Sikkerhetstesting (kortlogg)
- **Autentiseringsfeil:** Pålogging med feil passord gir blokkert respons og ingen sesjon. Passerte som forventet.
- **Rollebegrensning:** Pilot forsøkte å nå `/Admin/Users` og ble redirectet til login/forbudt – passerte.
- **Rate limiting:** 15 raske POST-forsøk på hinder-endepunkt gav begrensning etter 10, deretter kø/avvisning – passerte.
