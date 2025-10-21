// --- geolocation.js ---
// This file handles the Leaflet map + geolocation + click-to-select position

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

    var marker;

    // Når bruker klikker på kartet → plasser markør og oppdater felter
    map.on('click', function (e) {
        if (marker) {
            marker.setLatLng(e.latlng);
        } else {
            marker = L.marker(e.latlng).addTo(map);
        }

        document.querySelector('[name="Latitude"]').value = e.latlng.lat.toFixed(6);
        document.querySelector('[name="Longitude"]').value = e.latlng.lng.toFixed(6);
    });

    // --- GEOLOCATION ---
    map.locate({ setView: true, maxZoom: 16 });

    // Hvis posisjonen ble funnet
    map.on('locationfound', function (e) {
        var radius = e.accuracy;

        if (marker) {
            marker.setLatLng(e.latlng);
        } else {
            marker = L.marker(e.latlng).addTo(map);
        }

        document.querySelector('[name="Latitude"]').value = e.latlng.lat.toFixed(6);
        document.querySelector('[name="Longitude"]').value = e.latlng.lng.toFixed(6);

        L.circle(e.latlng, radius).addTo(map)
            .bindPopup("Du er innenfor " + Math.round(radius) + " meter fra dette punktet").openPopup();
    });

    // Hvis geolokasjon feilet
    map.on('locationerror', function (e) {
        console.warn("Geolocation error:", e.message);
        alert("Geolocation feilet: " + e.message);
    });
});
