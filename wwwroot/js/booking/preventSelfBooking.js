// Este script previene que un profesional reserve una cita consigo mismo desde el frontend.
// Debe ser incluido en la página de booking.

(function() {
    // El id del profesional y del usuario deben estar disponibles en variables globales o como data-attributes.
    // Suponemos que existen variables globales: window.currentUserId y window.professionalId
    if (typeof window.currentUserId !== 'undefined' && typeof window.professionalId !== 'undefined') {
        if (window.currentUserId === window.professionalId) {
            // Oculta el formulario y muestra un mensaje de error
            var formSection = document.getElementById('booking-form-section');
            if (formSection) {
                formSection.style.display = 'none';
            }
            var bookingContent = document.querySelector('.booking-content');
            if (bookingContent) {
                var alert = document.createElement('div');
                alert.className = 'alert alert-danger';
                alert.innerHTML = '<i class="fas fa-ban me-2"></i>No puedes reservar una cita contigo mismo.';
                bookingContent.prepend(alert);
            }
        }
    }
})();
