// Initialize the map
var map = L.map('map').setView([60.472, 8.4689], 5); // Default center (Norway)

// Add a base tile layer
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors'
}).addTo(map);

// Try to locate the user and set the view
map.locate({ setView: true, maxZoom: 16 });

// When location is found
function onLocationFound(e) {
    var radius = e.accuracy;

    // Add marker
    L.marker(e.latlng).addTo(map)
        .bindPopup("You are within " + Math.round(radius) + " meters from this point")
        .openPopup();

    // Draw accuracy circle
    L.circle(e.latlng, radius).addTo(map);
}

// When location fails
function onLocationError(e) {
    alert(e.message);
}

// Register event listeners
map.on('locationfound', onLocationFound);
map.on('locationerror', onLocationError);
