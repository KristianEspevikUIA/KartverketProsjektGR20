// --- geolocation.js ---
// This file handles the Leaflet map + geolocation + click-to-select position
// Utvidet: linje-/frihåndstegning med GeoJSON-eksport (LineString)

document.addEventListener("DOMContentLoaded", function () {

    // Finn kart-elementet
    var mapContainer = document.getElementById('map');
    if (!mapContainer) return; // Ikke kjør scriptet hvis siden ikke har kart

    // Lag kart med standardvisning
    var map = L.map('map').setView([59.91, 10.75], 12);

    // Legg til OpenStreetMap-lag
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    // --- Skjema-referanser (valgfri: hvis finnes i DOM oppdateres de) ---
    var latInput = document.querySelector('[name="Latitude"]');
    var lngInput = document.querySelector('[name="Longitude"]');
    var lineInput = document.getElementById('LineGeoJson'); // <input type="hidden" id="LineGeoJson">

    function toFixed6(x) { return Number(x).toFixed(6); }
    function setLatLngInputs(latlng) {
        if (latInput) latInput.value = toFixed6(latlng.lat);
        if (lngInput) lngInput.value = toFixed6(latlng.lng);
    }

    // --- Punktmarkør (BEHOLDER original Leaflet-marker) ---
    var marker;

    // --- Linje-/frihåndstegning ---
    var lineLayer = null;     // L.Polyline
    var linePoints = [];      // [[lat,lng], ...]
    var isMouseDown = false;  // frihånd aktiv

    function lineMidpoint() {
        if (linePoints.length === 0) return null;
        var sLat = 0, sLng = 0;
        for (var i = 0; i < linePoints.length; i++) {
            sLat += linePoints[i][0];
            sLng += linePoints[i][1];
        }
        return L.latLng(sLat / linePoints.length, sLng / linePoints.length);
    }

    function updateHiddenGeoJson() {
        if (!lineInput) return;
        if (lineLayer && linePoints.length >= 2) {
            // Eksporter kun geometrien (LineString) for enkel lagring
            var gj = lineLayer.toGeoJSON();
            lineInput.value = JSON.stringify(gj.geometry);
        } else {
            lineInput.value = "";
        }
    }

    function syncLine() {
        if (linePoints.length >= 2) {
            if (!lineLayer) {
                lineLayer = L.polyline(linePoints, {
                    color: '#0055ff',
                    weight: 4,
                    opacity: 0.95
                }).addTo(map);
            } else {
                lineLayer.setLatLngs(linePoints);
            }
            // Oppdater skjema med midtpunktet av linja (samme logikk som i “oppdatert”)
            var mid = lineMidpoint();
            if (mid) setLatLngInputs(mid);

            // Når linje vises, skjul punktmarkøren (som i den “oppdaterte” logikken)
            if (marker && map.hasLayer(marker)) map.removeLayer(marker);
        } else {
            if (lineLayer) { map.removeLayer(lineLayer); lineLayer = null; }
        }
        updateHiddenGeoJson();
    }

    function clearLine() {
        linePoints = [];
        if (lineLayer) { map.removeLayer(lineLayer); lineLayer = null; }
        updateHiddenGeoJson();
    }

    // Når bruker klikker på kartet:
    // - Første klikk: sett markør og start potensiell linje (første punkt).
    // - Neste klikk: legg til nytt vertex i linja.
    map.on('click', function (e) {
        if (linePoints.length >= 1) {
            // Vi er i "linjemodus": legg til vertex
            linePoints.push([e.latlng.lat, e.latlng.lng]);
            syncLine();
            return;
        }

        // Første klikk: sett markør (original stil)
        if (marker) {
            marker.setLatLng(e.latlng).addTo(map);
        } else {
            marker = L.marker(e.latlng).addTo(map);
        }
        setLatLngInputs(e.latlng);

        // Start potensiell linje med første punkt
        linePoints = [[e.latlng.lat, e.latlng.lng]];
        updateHiddenGeoJson();
    });

    // Frihånd: hold venstre mus nede og beveg for å tegne linje
    map.on('mousedown', function (e) {
        if (e.originalEvent.button !== 0) return; // kun venstre knapp
        isMouseDown = true;
        map.dragging.disable();
        if (linePoints.length === 0) {
            linePoints.push([e.latlng.lat, e.latlng.lng]);
        }
        syncLine();
    });

    map.on('mousemove', function (e) {
        if (!isMouseDown) return;
        var last = linePoints[linePoints.length - 1];
        var dLat = Math.abs(e.latlng.lat - last[0]);
        var dLng = Math.abs(e.latlng.lng - last[1]);
        // Terskel for å unngå altfor tett punktsky
        if (dLat > 0.00005 || dLng > 0.00005) {
            linePoints.push([e.latlng.lat, e.latlng.lng]);
            syncLine();
        }
    });

    map.on('mouseup', function () {
        if (!isMouseDown) return;
        isMouseDown = false;
        map.dragging.enable();
        syncLine();
    });

    // Dobbeltklikk: bare “avslutt tegning” (ingen ekstra handling)
    map.on('dblclick', function () {
        isMouseDown = false;
        map.dragging.enable();
    });

    // Høyreklikk: tøm linje (behold markør hvis du klikker nytt punkt etterpå)
    map.on('contextmenu', function (e) {
        e.originalEvent.preventDefault();
        e.originalEvent.stopPropagation();
        clearLine();
    });

    // ESC tømmer linje
    document.addEventListener('keydown', function (ev) {
        if (ev.key === 'Escape') clearLine();
    });

    // --- GEOLOCATION ---
    map.locate({ setView: true, maxZoom: 16 });

    // Hvis posisjonen ble funnet
    map.on('locationfound', function (e) {
        var radius = e.accuracy;

        if (linePoints.length === 0) {
            // Bare oppdater markør hvis vi ikke viser linje (samme “prioritet” som i oppdatert logikk)
            if (marker) {
                marker.setLatLng(e.latlng).addTo(map);
            } else {
                marker = L.marker(e.latlng).addTo(map);
            }
            setLatLngInputs(e.latlng);
        }

        // Behold nøyaktighetssirkel (stor sirkel – ikke en mini-dot)
        L.circle(e.latlng, radius).addTo(map)
            .bindPopup("Du er innenfor " + Math.round(radius) + " meter fra dette punktet").openPopup();
    });

    // Hvis geolokasjon feilet
    map.on('locationerror', function (e) {
        console.warn("Geolocation error:", e.message);
        alert("Geolocation feilet: " + e.message);
    });
});
