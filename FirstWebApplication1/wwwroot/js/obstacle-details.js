(() => {
    const mapElement = document.getElementById('map');

    if (!mapElement) {
        return;
    }

    const hasLine = mapElement.dataset.hasLine === 'true';
    const obstacleName = mapElement.dataset.obstacleName ?? '';
    const lat = parseFloat(mapElement.dataset.lat || '0');
    const lng = parseFloat(mapElement.dataset.lng || '0');

    const lineDataScript = document.getElementById('line-coordinates-data');
    let lineCoordinates = [];

    if (lineDataScript && hasLine) {
        try {
            lineCoordinates = JSON.parse(lineDataScript.textContent || '[]');
        } catch (err) {
            console.error('Could not parse line coordinates', err);
            lineCoordinates = [];
        }
    }

    const map = L.map('map');
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

    if (hasLine && Array.isArray(lineCoordinates) && lineCoordinates.length > 0) {
        const coords = lineCoordinates.map(c => [c.latitude, c.longitude]);
        const poly = L.polyline(coords, { color: 'red' }).addTo(map);
        map.fitBounds(poly.getBounds());
        map.setZoom(map.getZoom() - 1);
    } else {
        map.setView([lat, lng], 13);
        L.marker([lat, lng]).addTo(map).bindPopup(obstacleName).openPopup();
    }

    const showBtn = document.getElementById('showReasonBtn');
    const reasonBox = document.getElementById('reasonContainer');
    const declineTextarea = document.querySelector("textarea[name='declineReason']");
    const declineCounter = document.getElementById('declineCount');

    if (declineTextarea && declineCounter) {
        declineTextarea.addEventListener('input', () => {
            declineCounter.textContent = declineTextarea.value.length.toString();
        });
    }

    if (showBtn && reasonBox) {
        showBtn.addEventListener('click', () => {
            reasonBox.classList.remove('hidden');
            showBtn.classList.add('hidden');
        });
    }
})();
