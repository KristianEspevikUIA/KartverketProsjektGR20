(() => {
    const toast = document.getElementById('notification-toast');

    if (!toast) {
        return;
    }

    window.addEventListener('load', () => {
        requestAnimationFrame(() => {
            toast.classList.remove('opacity-0');
        });

        setTimeout(() => {
            toast.classList.add('opacity-0');
            setTimeout(() => toast.remove(), 300);
        }, 3500);
    });
})();
