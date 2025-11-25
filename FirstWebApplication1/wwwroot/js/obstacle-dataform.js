(() => {
    const formContainer = document.getElementById('obstacle-height-form');

    if (!formContainer) {
        return;
    }

    const usesFeet = formContainer.dataset.usesFeet === 'true';
    const isPilot = formContainer.dataset.isPilot === 'true';

    const METERS_TO_FEET = 3.28084;
    const FEET_TO_METERS = 1 / METERS_TO_FEET;
    const MIN_METERS = 15;
    const MAX_METERS = 300;
    const MIN_FEET = MIN_METERS * METERS_TO_FEET;
    const MAX_FEET = MAX_METERS * METERS_TO_FEET;

    let currentUnit = usesFeet ? 'feet' : 'meters';

    const heightSlider = document.getElementById('heightSlider');
    const heightInputDisplay = document.getElementById('heightInputDisplay');
    const heightInput = document.getElementById('ObstacleHeight');
    const unitToggle = document.getElementById('unitToggle');
    const currentUnitSpan = document.getElementById('currentUnit');
    const minLabel = document.getElementById('minLabel');
    const maxLabel = document.getElementById('maxLabel');
    const metersValueSpan = document.getElementById('feetValue');

    function updateHeightDisplay() {
        if (!heightSlider || !heightInputDisplay || !heightInput) {
            return;
        }

        let sliderValue = parseFloat(heightSlider.value);
        let metersValue;

        if (currentUnit === 'feet') {
            sliderValue = Math.round(sliderValue);
            heightInputDisplay.value = sliderValue.toFixed(0);
            metersValue = sliderValue * FEET_TO_METERS;
        } else {
            heightInputDisplay.value = sliderValue.toFixed(1);
            metersValue = sliderValue;
        }

        heightInput.value = metersValue.toFixed(2);

        if (metersValueSpan && isPilot) {
            metersValueSpan.textContent = metersValue.toFixed(2);
        }
    }

    if (heightInputDisplay) {
        heightInputDisplay.addEventListener('input', () => {
            let typedValue = parseFloat(heightInputDisplay.value);
            if (isNaN(typedValue) || !heightSlider || !heightInput) return;

            if (currentUnit === 'feet') {
                typedValue = Math.min(Math.max(typedValue, MIN_FEET), MAX_FEET);
                heightSlider.value = typedValue.toString();
                heightInput.value = (typedValue * FEET_TO_METERS).toFixed(2);

                if (metersValueSpan && isPilot) {
                    metersValueSpan.textContent = (typedValue * FEET_TO_METERS).toFixed(2);
                }
            } else {
                typedValue = Math.min(Math.max(typedValue, MIN_METERS), MAX_METERS);
                heightSlider.value = typedValue.toString();
                heightInput.value = typedValue.toFixed(2);

                if (metersValueSpan && isPilot) {
                    metersValueSpan.textContent = typedValue.toFixed(2);
                }
            }
        });
    }

    if (heightSlider) {
        heightSlider.addEventListener('input', updateHeightDisplay);
    }

    function toggleUnit() {
        if (!heightSlider || !heightInput || !minLabel || !maxLabel || !currentUnitSpan || !isPilot) return;

        const metersValueCurrent = parseFloat(heightInput.value) || MIN_METERS;

        if (currentUnit === 'meters') {
            currentUnit = 'feet';
            const feetValue = metersValueCurrent * METERS_TO_FEET;

            heightSlider.min = MIN_FEET.toFixed(0);
            heightSlider.max = MAX_FEET.toFixed(0);
            heightSlider.step = '1';
            heightSlider.value = Math.round(feetValue).toString();

            currentUnitSpan.textContent = 'Feet';
            minLabel.textContent = `${MIN_FEET.toFixed(0)} ft (Min)`;
            maxLabel.textContent = `${MAX_FEET.toFixed(0)} ft (Max)`;
        }
        else {
            currentUnit = 'meters';

            heightSlider.min = MIN_METERS.toFixed(1);
            heightSlider.max = MAX_METERS.toFixed(1);
            heightSlider.step = '0.5';
            heightSlider.value = metersValueCurrent.toFixed(1);

            currentUnitSpan.textContent = 'Meters';
            minLabel.textContent = `${MIN_METERS.toFixed(0)} m (Min)`;
            maxLabel.textContent = `${MAX_METERS.toFixed(0)} m (Max)`;
        }

        updateHeightDisplay();
    }

    if (unitToggle && isPilot) {
        unitToggle.addEventListener('click', (e) => {
            e.preventDefault();
            toggleUnit();
        });
    }

    updateHeightDisplay();
})();
