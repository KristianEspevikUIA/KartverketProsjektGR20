# Tablet- og responsivitetsdokumentasjon

## Strategi
- **Responsivt rammeverk:** Tailwind CSS via CDN med mobile-first klasser (`flex`, `grid`, `px-4`, `sm:px-6`, `md:flex` osv.). Layouten bruker `md`- og `lg`-breakpoints for å utnytte bredden på nettbrett.
- **Kart og tabeller:** Leaflet-kartet skalerer til 100 % bredde og ligger i fleks-/grid-containere som justerer seg til to kolonner på nettbrett. Tabeller har horisontal scroll (`overflow-x`) aktivert under `md`-brekkpunktet.
- **Formelementer:** Skjemaer bruker blokkoppsett på små skjermer og deler felt i to kolonner fra `md`-brekkpunktet for bedre utnyttelse av skjermflaten.
- **Navigasjon:** Navigasjonen brytes til én kolonne under `md`; på nettbrett vises horisontal navigasjon med tilpassede marger/padding.

## Test på nettbrett
- **iPad (10,9", Safari 1024x768, landscape/portrait):**
  - Login- og registreringsskjemaer fyller bredden uten horisontal scroll.
  - Kart og tabeller deler plass i to kolonner i landscape; i portrait legges de under hverandre uten overlapp.
- **Galaxy Tab S7 (Android/Chrome 1280x800, landscape/portrait):**
  - Kartet skalerer og beholder interaktive kontroller i synlig område.
  - Tabeller kan scrolles horisontalt i portrait for å se alle kolonner.

Vi har ikke testet dette direkte på enhetene, men gjennom developer tools i chrome. Etter tilbakemelding fra piloter testet vi også for portrett modus, da vi fikk beskjed at det var dette pilotene bruker. 

## Videre forbedringer
- Legge til eksplisitte `lg`-tilpasninger for avanserte rapporttabeller for å utnytte bredde på store nettbrett.
- Vurdere tilpasset zoomnivå og kontrollstørrelse i Leaflet for nettbrett for bedre ergonomi.
- Dokumentere kort brukertest med 1–2 nettbrettbrukere for å bekrefte flyt og tydelighet.
- Legge til eventuelle skjermbilder 
