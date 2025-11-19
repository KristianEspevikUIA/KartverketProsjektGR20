/// Handles drawing a line on the Leaflet map and syncing it with the form

document.addEventListener('DOMContentLoaded', function () {
    const mapElement = document.getElementById('map');
    if (!mapElement) {
        return;
    }

    // Initialize map
    const map = L.map('map').setView([59.91, 10.75], 5); // Start zoomed out
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);


    // Ensure map renders properly if its container was resized
    setTimeout(() => {
        map.invalidateSize();
    }, 100);

    const form = mapElement.closest('form');
    const submitButton = form ? form.querySelector('button[type="submit"]') : null;
    const lineInput = form ? form.querySelector('[name="LineGeoJson"]') : null;
    const latInput = form ? form.querySelector('[name="Latitude"]') : null;
    const lngInput = form ? form.querySelector('[name="Longitude"]') : null;

    const undoButton = document.getElementById('undoPoint');
    const clearButton = document.getElementById('clearLine');
    const vertexList = document.getElementById('vertexList');
    const lineSummary = document.getElementById('lineSummary');

    let vertices = [];
    let polyline = null;
    let markers = [];

    // --- Layer group for approved obstacles ---
    const approvedObstaclesLayer = L.featureGroup().addTo(map);

    function toFixed(value) {
        return Number(value.toFixed(6));
    }

    function updateSubmitState() {
        if (!submitButton) {
            return;
        }

        const ready = vertices.length >= 1;
        submitButton.disabled = !ready;
        submitButton.classList.toggle('opacity-50', !ready);
        submitButton.classList.toggle('cursor-not-allowed', !ready);
    }

    function updateCoordinateFields() {
        if (!latInput || !lngInput) {
            return;
        }

        if (vertices.length === 0) {
            latInput.value = '';
            lngInput.value = '';
        } else {
            const last = vertices[vertices.length - 1];
            latInput.value = toFixed(last.lat).toString();
            lngInput.value = toFixed(last.lng).toString();
        }
    }

    function updateLineInput() {
        if (!lineInput) {
            return;
        }

        if (vertices.length < 2) {
            lineInput.value = '';
            return;
        }

        const geoJson = {
            type: 'LineString',
            coordinates: vertices.map(point => [toFixed(point.lng), toFixed(point.lat)])
        };

        lineInput.value = JSON.stringify(geoJson);
    }

    function updateVertexList() {
        if (!vertexList) {
            return;
        }

        if (vertices.length === 0) {
            vertexList.innerHTML = '<p class="text-gray-500 text-center py-2">No points selected yet. Click on the map to start.</p>';
            return;
        }

        const rows = vertices.map((point, index) => `
            <tr class="border-b last:border-0">
                <td class="px-3 py-2 text-gray-700">${index + 1}</td>
                <td class="px-3 py-2 font-mono text-gray-900">${point.lat.toFixed(6)}</td>
                <td class="px-3 py-2 font-mono text-gray-900">${point.lng.toFixed(6)}</td>
            </tr>
        `).join('');

        vertexList.innerHTML = `
            <div class="overflow-x-auto">
                <table class="min-w-full text-sm">
                    <thead class="bg-gray-100">
                        <tr class="text-left text-gray-600">
                            <th class="px-3 py-2">#</th>
                            <th class="px-3 py-2">Latitude</th>
                            <th class="px-3 py-2">Longitude</th>
                        </tr>
                    </thead>
                    <tbody>${rows}</tbody>
                </table>
            </div>`;
    }

    function formatLength(meters) {
        if (meters >= 1000) {
            return `${(meters / 1000).toFixed(2)} km`;
        }
        return `${meters.toFixed(1)} m`;
    }

    function calculateLength() {
        if (vertices.length < 2) {
            return 0;
        }

        let total = 0;
        for (let i = 1; i < vertices.length; i += 1) {
            total += map.distance(vertices[i - 1], vertices[i]);
        }
        return total;
    }

    function updateLineSummary() {
        if (!lineSummary) {
            return;
        }

        if (vertices.length === 0) {
            lineSummary.innerHTML = '<i class="fa-solid fa-info-circle mr-2"></i>No geometry selected yet.';
            return;
        }

        if (vertices.length === 1) {
            const point = vertices[0];
            lineSummary.innerHTML = `<i class="fa-solid fa-map-marker-alt mr-2 text-indigo-600"></i>Single point at ${point.lat.toFixed(6)}, ${point.lng.toFixed(6)}`;
            return;
        }

        const length = calculateLength();
        lineSummary.innerHTML = `<i class="fa-solid fa-route mr-2 text-indigo-600"></i>Line with ${vertices.length} points · Length: ${formatLength(length)}`;
    }

    function clearMap() {
        // Remove polyline
        if (polyline) {
            map.removeLayer(polyline);
            polyline = null;
        }

        // Remove all markers
        markers.forEach(marker => {
            map.removeLayer(marker);
        });
        markers = [];
    }

    function redrawLine() {
        clearMap();

        if (vertices.length === 0) {
            return;
        }

        if (vertices.length === 1) {
            // Single point - show marker
            const marker = L.marker(vertices[0], {
                icon: L.icon({
                    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
                    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
                    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
                    iconSize: [25, 41],
                    iconAnchor: [12, 41],
                    popupAnchor: [1, -34],
                    shadowSize: [41, 41]
                })
            }).addTo(map);
            markers.push(marker);
            map.setView(vertices[0], Math.max(map.getZoom(), 14));
            return;
        }

        // Multiple points - draw line
        polyline = L.polyline(vertices, {
            color: '#4f46e5',
            weight: 4,
            opacity: 0.8
        }).addTo(map);

        // Add circle markers for each vertex
        vertices.forEach((point, index) => {
            const circleMarker = L.circleMarker(point, {
                radius: 6,
                color: '#4f46e5',
                fillColor: '#818cf8',
                fillOpacity: 0.9,
                weight: 2
            }).addTo(map);

            circleMarker.bindPopup(`Point ${index + 1}`);
            markers.push(circleMarker);
        });

        // Fit map to line bounds
        map.fitBounds(polyline.getBounds(), { padding: [50, 50] });
    }

    function syncFormState() {
        updateCoordinateFields();
        updateLineInput();
        updateVertexList();
        updateSubmitState();
        updateLineSummary();
    }

    function addVertex(latlng) {
        vertices.push(latlng);
        redrawLine();
        syncFormState();
    }

    function undoVertex() {
        if (vertices.length === 0) {
            return;
        }

        vertices.pop();
        redrawLine();
        syncFormState();
    }

    function clearVertices() {
        if (vertices.length === 0) {
            return;
        }

        if (confirm('Are you sure you want to clear all points?')) {
            vertices = [];
            redrawLine();
            syncFormState();
        }
    }

    function tryLoadExistingGeometry() {
        if (!lineInput || !lineInput.value) {
            return;
        }

        try {
            const parsed = JSON.parse(lineInput.value);
            if (parsed && parsed.type === 'LineString' && Array.isArray(parsed.coordinates)) {
                const loadedVertices = parsed.coordinates
                    .filter(coord => Array.isArray(coord) && coord.length >= 2)
                    .map(coord => L.latLng(coord[1], coord[0]));

                if (loadedVertices.length > 0) {
                    vertices = loadedVertices;
                    redrawLine();
                    syncFormState();
                }
            }
        } catch (error) {
            console.warn('Could not parse existing line geometry', error);
        }
    }

    function tryLoadExistingPoint() {
        if (!latInput || !lngInput) {
            return;
        }

        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);

        if (!Number.isNaN(lat) && !Number.isNaN(lng)) {
            vertices = [L.latLng(lat, lng)];
            redrawLine();
            syncFormState();
        }
    }

    function drawApprovedObstacles(layerGroup, obstacles) {
        console.log(`Found ${obstacles.length} approved obstacles to draw.`);

        if (!Array.isArray(obstacles)) {
            return;
        }

        obstacles.forEach(obstacle => {
            console.log('Processing obstacle:', obstacle);
            const popupContent = `
                <div style="font-family: sans-serif;">
                    <h4 style="margin: 0 0 5px 0; font-size: 1.1em;">${obstacle.ObstacleName || 'Approved Obstacle'}</h4>
                    <p style="margin: 0; color: #555;">Type: ${obstacle.ObstacleType || 'N/A'}</p>
                </div>`;

            if (obstacle.LineGeoJson) {
                try {
                    const geoJson = JSON.parse(obstacle.LineGeoJson);
                    if (geoJson.type === 'LineString' && Array.isArray(geoJson.coordinates) && geoJson.coordinates.length > 0) {
                        const latLngs = geoJson.coordinates
                            .filter(coord => Array.isArray(coord) && coord.length >= 2)
                            .map(coord => [coord[1], coord[0]]);

                        if (latLngs.length > 0) {
                            L.polyline(latLngs, {
                                color: '#16a34a',
                                weight: 3,
                                opacity: 0.6
                            }).bindPopup(popupContent).addTo(layerGroup);
                        }
                    }
                } catch (e) { console.error("Error parsing LineGeoJson:", e); }
            } else if (obstacle.Latitude && obstacle.Longitude) {
                L.marker([obstacle.Latitude, obstacle.Longitude], {
                    opacity: 0.7,
                    icon: L.icon({
                        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-green.png',
                        shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
                        iconSize: [25, 41],
                        iconAnchor: [12, 41],
                        popupAnchor: [1, -34],
                        shadowSize: [41, 41]
                    })
                }).bindPopup(popupContent).addTo(layerGroup);
            }
        });
    }

    // --- Main Execution ---
    map.whenReady(function () {
        console.log('Map is ready. Checking for approved obstacles data.');
        const obstaclesDataAttribute = mapElement.dataset.approvedObstacles;
        if (obstaclesDataAttribute) {
            try {
                const approvedObstaclesData = JSON.parse(obstaclesDataAttribute);
                if (approvedObstaclesData.length > 0) {
                    drawApprovedObstacles(approvedObstaclesLayer, approvedObstaclesData);
                }
            } catch (e) {
                console.error("Failed to parse approved obstacles JSON data:", e);
            }
        }

        // After drawing, decide where to center the map
        if (approvedObstaclesLayer.getLayers().length > 0) {
            map.fitBounds(approvedObstaclesLayer.getBounds(), { padding: [50, 50], maxZoom: 14 });
        } else {
            map.locate({ setView: true, maxZoom: 14 });
        }
    });

    // Event: Map click
    map.on('click', function (event) {
        addVertex(event.latlng);
    });

    // Event: Undo button
    if (undoButton) {
        undoButton.addEventListener('click', undoVertex);
    }

    // Event: Clear button
    if (clearButton) {
        clearButton.addEventListener('click', clearVertices);
    }

    // Load any geometry from the form if returning from a validation error
    tryLoadExistingGeometry();
    if (vertices.length === 0) {
        tryLoadExistingPoint();
    }

    syncFormState();

    // Geolocation fallbacks
    map.on('locationfound', function (event) {
        if (approvedObstaclesLayer.getLayers().length === 0 && vertices.length === 0) {
            map.setView(event.latlng, 14);
        }
    });

    map.on('locationerror', function (event) {
        console.warn('Geolocation error:', event.message);
    });
});