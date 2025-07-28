document.addEventListener('DOMContentLoaded', function () {
    // Inicializar sin estrellas seleccionadas
    const ratingInput = document.getElementById('ratingInput');
    const stars = document.querySelectorAll('.star');
    const comment = document.getElementById('comment');
    const charCount = document.getElementById('charCount');
    const form = document.getElementById('reviewForm');
    const message = document.getElementById('reviewMessage');
    if (ratingInput) {
        ratingInput.value = "";
    }
    highlightStars(0);

    // Interacción de estrellas
    stars.forEach(star => {
        star.addEventListener('mouseover', () => {
            highlightStars(star.dataset.value);
        });
        star.addEventListener('mouseout', () => {
            highlightStars(ratingInput.value);
        });
        star.addEventListener('click', () => {
            ratingInput.value = star.dataset.value;
            highlightStars(star.dataset.value);
        });
    });
    function highlightStars(val) {
        stars.forEach(star => {
            star.classList.toggle('selected', star.dataset.value <= val);
        });
    }
    // Contador de caracteres
    comment.addEventListener('input', function () {
        charCount.textContent = `${comment.value.length}/500`;
    });
    // Envío del formulario
    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        message.textContent = '';
        if (!ratingInput.value) {
            message.textContent = 'La calificación es obligatoria.';
            message.style.color = 'red';
            return;
        }
        if (comment.value.length > 500) {
            message.textContent = 'El comentario no puede superar los 500 caracteres.';
            message.style.color = 'red';
            return;
        }
        // Construir payload
        const payload = {
            professionalId: form.dataset.professionalId,
            bookingId: form.dataset.bookingId,
            rating: parseInt(ratingInput.value),
            comment: comment.value
        };
        try {
            const res = await fetch('/api/reviews', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin',
                body: JSON.stringify(payload)
            });
            if (!res.ok) {
                let errMsg = "Error al enviar la reseña.";
                try {
                    const err = await res.json();
                    errMsg = err.error || err.message || errMsg;
                } catch (e) {}
                message.textContent = errMsg;
                message.style.color = 'red';
                return;
            }
            message.textContent = '¡Reseña enviada con éxito!';
            message.style.color = '#28a745';
            form.reset();
            highlightStars(0);
            charCount.textContent = '0/500';
        } catch (error) {
            message.textContent = 'Error de red.';
            message.style.color = 'red';
        }
    });
});
