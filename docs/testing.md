# Testdokumentasjon

## Testplan
- **Mål:** Verifisere funksjonell flyt for innlogging, hinderregistrering, saksbehandling og kartvisning. Bekrefte rollebegrensninger og grunnleggende sikkerhetstiltak.
- **Miljø:** Lokal kjøring via Visual Studio/Docker Compose (MariaDB + ASP.NET Core). Standard kultur `en-US` for koordinater.
- **Roller testet:** Admin, Caseworker, Pilot.
- **Data:** Testkontoer med seedet admin, manuelt opprettet pilot og saksbehandler.

## Testscenarier
1. **Innlogging → rapport → kvittering (Pilot)**
   - Gitt pålogget pilot, når pilot velger hindertype og fyller ut skjema, så lagres hinder med status `Pending` og oversikt vises.
2. **Saksbehandler oppdaterer status**
   - Gitt pending hinder, når Caseworker endrer status til `Approved`, så reflekteres oppdateringen i liste og kart-JSON.
3. **Visning av godkjente hinder (Pilot/Caseworker/Admin)**
   - Når bruker åpner kart/oversikt, returneres kun hindre med status `Approved` i JSON som konsumeres av Leaflet.
4. **Feilhåndtering**
   - Ugyldige felter i registreringsskjema gir model state-feil og vises som valideringsmeldinger.
5. **Rollebeskyttede ruter**
   - Pilot forsøker å nå Admin-endepunkt og får avslag.
6. **Sikkerhetsbelastning**
   - 15 raske forespørsler mot hinder-endepunkt utløser rate limiting etter 10 (fast vindu).
7. **Autentiseringsfeil**
   - Feil passord ved innlogging gir avvist forsøk, uten sesjonsopprettelse.
8. **Kart og koordinater**
   - Godkjente hinder vises med korrekte koordinater og GeoJSON-linje når tilgjengelig.

## Testlogg og resultater
| Dato | Scenario | Rolle | Resultat |
| --- | --- | --- | --- |
| 2025-01-15 | Pilot registrerer hinder og ser kvittering | Pilot | OK |
| 2025-01-15 | Caseworker endrer status til Approved | Caseworker | OK |
| 2025-01-15 | Pilot åpner kart og ser kun Approved | Pilot | OK |
| 2025-01-15 | Ugyldig skjema viser valideringsfeil | Pilot | OK |
| 2025-01-15 | Pilot prøver Admin-side | Pilot | Avvist |
| 2025-01-15 | Rate limiting etter 10 forespørsler | Pilot | OK |
| 2025-01-15 | Feil passord ved innlogging | Pilot | Avvist |

## Videre testarbeid
- Automatisere scenariene over i integrasjonstester for hinderflyt og rollebegrensninger.
- Legge til lastertest mot rate limiter og database under realistisk volum.
- UX-test med 1–2 brukere for navigasjon, spesielt mobilnav og kartinteraksjon.
