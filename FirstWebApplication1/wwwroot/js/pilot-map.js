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

    const statusStyles = {
        Approved: {
            color: '#0b7a3a',
            fillColor: '#0b7a3a'
        },
        Pending: {
            color: '#d97706',
            fillColor: '#facc15'
        }
    };

    obstacles.forEach(o => {
        if (!o.latitude || !o.longitude) return;

        const markerStyle = statusStyles[o.status] ?? {
            color: '#dc2626',
            fillColor: '#fca5a5'
        };

        const marker = L.circleMarker([o.latitude, o.longitude], {
            ...markerStyle,
            radius: 9,
            weight: 3,
            fillOpacity: 0.95
        })
            .addTo(map)
            .bindPopup(`<b>${o.obstacleName}</b><br/>${o.obstacleHeight} m (${o.status})`);

        //  DRAW LINESTRING IF EXISTS
        if (o.lineGeoJson) {
            try {
                const geojson = JSON.parse(o.lineGeoJson);
                const lineColor = statusStyles[o.status]?.color ?? '#dc2626';

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
