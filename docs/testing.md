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
3. **Visning av godkjente og ventende hinder (Pilot)**
   - Når pilot åpner kartet, returneres hindre med status `Approved` eller `Pending` i JSON som konsumeres av Leaflet.
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
9. **Pilot-begrensning (redigere kun egne hindre)**
Pilot forsøker å redigere et hinder noen andre har registrert. Blir vist til access denied siden. 

## Testlogg og resultater
| Dato | Scenario | Rolle | Resultat |
| --- | --- | --- | --- |
| 01/12/2025 | Pilot registrerer hinder og ser kvittering | Pilot | OK |
| 01/12/2025 | Caseworker endrer status til Approved | Caseworker | OK |
| 01/12/2025 | Pilot åpner kart og ser Approved + Pending | Pilot | OK |
| 01/12/2025 | Ugyldig skjema viser valideringsfeil | Pilot | OK |
| 01/12/2025 | Pilot prøver Admin-side | Pilot | Avvist |
| 01/12/2025 | Rate limiting etter 10 forespørsler | Pilot | OK |
| 01/12/2025 | Feil passord ved innlogging | Pilot | Avvist |
| 04/12/2025 | Pilot prøver å redigere andre sitt hinder | Pilot | Avvist |

## Videre testarbeid
- Automatisere scenariene over i integrasjonstester for hinderflyt og rollebegrensninger.
- Legge til lastertest mot rate limiter og database under realistisk volum.
- UX-test med 1-2 brukere for navigasjon, spesielt mobilnav og kartinteraksjon.

# UX-test gjennomført av pilot fra luftambulansen
Resultat:
- legge til pending obstacles på kartet også, for å få bedre oversikt.
- legg til WCAG-godkjente primærknapper på forsiden/landingssider for tydelig fokus og kontrast (implementert etter testen).

