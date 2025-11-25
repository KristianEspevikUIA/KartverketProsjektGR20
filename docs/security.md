# Sikkerhetsdokumentasjon

## Oversikt over tiltak
- **Autentisering/autorisasjon:** ASP.NET Core Identity med roller (`Admin`, `Caseworker`, `Pilot`). Registrering av Admin begrenses til forhåndsdefinert e-post i konfigurasjon.
- **Ressurskontroll:** Controller for hinder er beskyttet med rate limiting (10 requests / 10 sek, med kø på 5), og tilgangskrav per rolle på handlinger.
- **Sikkerhetshoder:**
  - `Content-Security-Policy`: begrenser skript, stilark, fonter og bilder til kjente kilder.
  - `X-Frame-Options: DENY`
  - `X-Content-Type-Options: nosniff`
  - `X-XSS-Protection: 1; mode=block`
  - `Referrer-Policy: strict-origin-when-cross-origin`
- **HTTPS/HSTS:** Aktiv ved ikke-utviklingsmiljø.
- **CSRF-beskyttelse:** `[ValidateAntiForgeryToken]` på POST-handlinger i registreringsflyten for hinder.

## Misbruksscenarier og mitigering
- **Pilot forsøker å få admin-tilgang:** Admin-registrering er eksplisitt sperret i `AccountController` og admin-rollene tildeles kun via seed/eksisterende admin.
- **Brute force/skript-spam:** Rate limiting på hinderkontrolleren reduserer effekten av automatiserte innsendinger. Identity-passordkrav gir minimumssikring.
- **Clickjacking:** `X-Frame-Options: DENY` hindrer innramming av siden.
- **Dataeksfiltrasjon via third-party scripts:** CSP begrenser hvilke eksterne domener som kan levere script og stilark.
- **Privilege escalation ved rollemisbruk:** Autorisasjonsattributter på controller-/action-nivå håndhever at kun riktige roller kan opprette, endre og godkjenne hinder.

## Operasjonelle rutiner
- **Konfigurasjon av admin-konto:** Sett `Admin:Email` og `Admin:Password` som miljøvariabler eller i secrets før oppstart.
- **Databasenøkler:** Tilkoblingsstrengen `DefaultConnection` må settes via user-secrets eller miljø for å unngå sjekk-in av hemmeligheter.
- **Oppdatering av eksterne CDN-pekere:** Ved oppdatering av Tailwind/FontAwesome/jQuery bør CSP-listen oppdateres tilsvarende.

## Sikkerhetstesting (kortlogg)
- **Autentiseringsfeil:** Pålogging med feil passord gir blokkert respons og ingen sesjon. Passerte som forventet.
- **Rollebegrensning:** Pilot forsøkte å nå `/Admin/Users` og ble redirectet til login/forbudt – passerte.
- **Rate limiting:** 15 raske POST-forsøk på hinder-endepunkt gav begrensning etter 10, deretter kø/avvisning – passerte.
