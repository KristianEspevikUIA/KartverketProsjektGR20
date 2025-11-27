document.addEventListener('DOMContentLoaded', async () => {

    // Start map
    const map = L.map('pilot-map').setView([65, 13], 5); // Norway view

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18
    }).addTo(map);

    // Fetch approved obstacles from backend
    const response = await fetch('/Pilot/GetPilotObstacles');
    const obstacles = await response.json();

    const layers = [];

    obstacles.forEach(o => {
        if (!o.latitude || !o.longitude) return;

        let iconUrl = '';

        if (o.status === "Approved") {
            iconUrl = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-green.png';
        }
        else if (o.status === "Pending") {
            iconUrl = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-yellow.png';
        }

        const marker = L.marker([o.latitude, o.longitude], {
            icon: L.icon({
                iconUrl: iconUrl,
                shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
                iconSize: [25, 41],
                iconAnchor: [12, 41]
            })
        })
            .addTo(map)
            .bindPopup(`<b>${o.obstacleName}</b><br/>${o.obstacleHeight} m`);

        //  DRAW LINESTRING IF EXISTS
        if (o.lineGeoJson) {
            try {
                const geojson = JSON.parse(o.lineGeoJson);
                const lineColor = o.status === "Approved" ? "#0b7a3a" : "#e6b800"; 

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