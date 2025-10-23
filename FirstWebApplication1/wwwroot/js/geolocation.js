// Handles drawing a line on the Leaflet map and syncing it with the form

document.addEventListener('DOMContentLoaded', function () {
    const mapElement = document.getElementById('map');
    if (!mapElement) {
        return;
    }

    const map = L.map('map').setView([59.91, 10.75], 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

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
    const markerLayer = L.layerGroup().addTo(map);

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
            vertexList.innerHTML = '<p class="text-gray-500">No points selected yet. Click on the map to start drawing.</p>';
            return;
        }

        const rows = vertices.map((point, index) => `
            <tr>
                <td class="px-3 py-1 text-gray-700">${index + 1}</td>
                <td class="px-3 py-1 font-mono text-gray-900">${point.lat.toFixed(6)}</td>
                <td class="px-3 py-1 font-mono text-gray-900">${point.lng.toFixed(6)}</td>
            </tr>
        `).join('');

        vertexList.innerHTML = `
            <div class="overflow-x-auto">
                <table class="min-w-full text-sm">
                    <thead>
                        <tr class="text-left text-gray-500">
                            <th class="px-3 py-1">#</th>
                            <th class="px-3 py-1">Latitude</th>
                            <th class="px-3 py-1">Longitude</th>
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
            lineSummary.textContent = 'No geometry selected yet.';
            return;
        }

        if (vertices.length === 1) {
            const point = vertices[0];
            lineSummary.textContent = `Single point at ${point.lat.toFixed(6)}, ${point.lng.toFixed(6)}.`;
            return;
        }

        const length = calculateLength();
        lineSummary.textContent = `Line with ${vertices.length} points · Length ${formatLength(length)}.`;
    }

    function redrawLine() {
        markerLayer.clearLayers();

        if (polyline) {
            map.removeLayer(polyline);
            polyline = null;
        }

        if (vertices.length === 0) {
            return;
        }

        if (vertices.length === 1) {
            L.marker(vertices[0]).addTo(markerLayer);
            map.setView(vertices[0], Math.max(map.getZoom(), 14));
            return;
        }

        polyline = L.polyline(vertices, { color: '#1d4ed8', weight: 4 }).addTo(map);

        vertices.forEach(point => {
            L.circleMarker(point, {
                radius: 5,
                color: '#1d4ed8',
                fillColor: '#3b82f6',
                fillOpacity: 0.8
            }).addTo(markerLayer);
        });

        map.fitBounds(polyline.getBounds(), { padding: [20, 20] });
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

        vertices = [];
        redrawLine();
        syncFormState();
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

    map.on('click', function (event) {
        addVertex(event.latlng);
    });

    if (undoButton) {
        undoButton.addEventListener('click', undoVertex);
    }

    if (clearButton) {
        clearButton.addEventListener('click', clearVertices);
    }

    tryLoadExistingGeometry();

    if (vertices.length === 0) {
        tryLoadExistingPoint();
    }

    syncFormState();

    map.locate({ setView: true, maxZoom: 16 });

    map.on('locationfound', function (event) {
        if (vertices.length === 0) {
            map.setView(event.latlng, 14);
        }
    });

    map.on('locationerror', function (event) {
        console.warn('Geolocation error:', event.message);
    });
});
