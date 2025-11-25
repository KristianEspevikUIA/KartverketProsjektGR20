(() => {
    const mapElement = document.getElementById('map');

    if (!mapElement) {
        return;
    }

    const fallbackLat = parseFloat(mapElement.dataset.fallbackLat || '0');
    const fallbackLng = parseFloat(mapElement.dataset.fallbackLng || '0');
    const hasPointGeometry = mapElement.dataset.hasPointGeometry === 'true';

    const map = L.map('map').setView([fallbackLat, fallbackLng], 14);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    const lineGeoJsonElement = document.getElementById('line-geojson-data');
    const rawLineGeoJson = lineGeoJsonElement?.textContent?.trim();
    let lineGeoJson = null;

    if (rawLineGeoJson) {
        try {
            lineGeoJson = JSON.parse(rawLineGeoJson);
        } catch (err) {
            console.error('Could not parse obstacle line GeoJSON', err);
        }
    }

    if (lineGeoJson && lineGeoJson.type === 'LineString' && Array.isArray(lineGeoJson.coordinates)) {
        const latLngs = lineGeoJson.coordinates
            .filter(coord => Array.isArray(coord) && coord.length >= 2)
            .map(coord => [coord[1], coord[0]]);

        if (latLngs.length > 0) {
            const polyline = L.polyline(latLngs, { color: '#4f46e5', weight: 4 }).addTo(map);

            latLngs.forEach((point, index) => {
                L.circleMarker(point, {
                    radius: 6,
                    color: '#4f46e5',
                    fillColor: '#818cf8',
                    fillOpacity: 0.9,
                    weight: 2
                }).addTo(map).bindPopup(`Point ${index + 1}`);
            });

            if (latLngs.length > 1) {
                map.fitBounds(polyline.getBounds(), { padding: [20, 20] });
            } else {
                map.setView(latLngs[0], 14);
            }
        }
    } else if (hasPointGeometry) {
        L.marker([fallbackLat, fallbackLng]).addTo(map).bindPopup(mapElement.dataset.obstacleName || '').openPopup();
    }

    setTimeout(() => {
        map.invalidateSize();
    }, 100);
})();
