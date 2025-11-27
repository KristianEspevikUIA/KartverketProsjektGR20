document.addEventListener('DOMContentLoaded', async () => {

    // Start map
    const map = L.map('pilot-map').setView([65, 13], 5); // Norway view

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18
    }).addTo(map);

    // Fetch approved + pending obstacles from backend
    const response = await fetch('/Pilot/GetApprovedObstacles');

    if (!response.ok) {
        console.error('Failed to load obstacles:', response.status, response.statusText);
        return;
    }

    const obstacles = await response.json();

    const layers = [];

    obstacles.forEach(o => {
        if (!o.latitude || !o.longitude) return;

        const status = (o.status || '').toLowerCase();

        const statusColors = {
            approved: { marker: '#16a34a', line: '#0b7a3a' }, // green shades
            pending: { marker: '#facc15', line: '#e6b800' }    // yellow shades
        };

        const resolvedColors = statusColors[status] ?? { marker: '#f87171', line: '#b91c1c' }; // fallback red

        const marker = L.circleMarker([o.latitude, o.longitude], {
            radius: 10,
            color: resolvedColors.line,
            fillColor: resolvedColors.marker,
            fillOpacity: 0.9,
            weight: 2
        })
            .addTo(map)
            .bindPopup(`<b>${o.obstacleName}</b><br/>${o.obstacleHeight} m`);

        //  DRAW LINESTRING IF EXISTS
        if (o.lineGeoJson) {
            try {
                const geojson = JSON.parse(o.lineGeoJson);
                const lineColor = resolvedColors.line;

                const lineLayer = L.geoJSON(geojson, {
                    style: {
                        color: lineColor,
                        weight: 4
                    }
                }).addTo(map);

                layers.push(lineLayer);

            } catch (err) {
                console.error("Bad GeoJSON:", err);
            }
        }

        layers.push(marker);
    });

    // Zoom-to-fit
    if (layers.length > 0) {
        const group = L.featureGroup(layers);
        map.fitBounds(group.getBounds(), { padding: [40, 40] });
    }
});
