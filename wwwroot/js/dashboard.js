// Dashboard JavaScript functionality
document.addEventListener('DOMContentLoaded', function() {
    
    // Inicializar tooltips de Bootstrap si están disponibles
    if (typeof bootstrap !== 'undefined') {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // Hacer las tarjetas de reserva clickeables
    const bookingCards = document.querySelectorAll('.booking-card');
    bookingCards.forEach(card => {
        card.addEventListener('click', function() {
            const bookingId = this.dataset.bookingId;
            if (bookingId) {
                window.location.href = `/booking/details/${bookingId}`;
            }
        });
    });
    
    // Animación de contadores para las estadísticas
    const statValues = document.querySelectorAll('.stat-value');
    statValues.forEach(stat => {
        const finalValue = parseInt(stat.textContent);
        if (!isNaN(finalValue)) {
            animateCounter(stat, 0, finalValue, 1000);
        }
    });
    
    // Función para animar contadores
    function animateCounter(element, start, end, duration) {
        const range = end - start;
        const increment = range / (duration / 16);
        let current = start;
        
        const timer = setInterval(() => {
            current += increment;
            if (current >= end) {
                current = end;
                clearInterval(timer);
            }
            element.textContent = Math.floor(current);
        }, 16);
    }
    
    // Actualizar estado de navegación activa
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.dashboard-nav-list a');
    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
    
    // Función para actualizar datos en tiempo real (opcional)
    function updateDashboardData() {
        // Aquí se pueden agregar llamadas AJAX para actualizar datos en tiempo real
        console.log('Actualizando datos del dashboard...');
    }
    
    // Actualizar datos cada 30 segundos (opcional)
    // setInterval(updateDashboardData, 30000);
    
    // Manejo de notificaciones toast (si se implementan)
    function showNotification(message, type = 'info') {
        const toastContainer = document.getElementById('toast-container');
        if (toastContainer) {
            const toast = document.createElement('div');
            toast.className = `toast toast-${type}`;
            toast.innerHTML = `
                <div class="toast-header">
                    <strong class="me-auto">ProConnect</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            `;
            toastContainer.appendChild(toast);
            
            if (typeof bootstrap !== 'undefined') {
                const bsToast = new bootstrap.Toast(toast);
                bsToast.show();
            }
        }
    }
    
    // Exportar funciones para uso global
    window.DashboardUtils = {
        showNotification: showNotification,
        updateDashboardData: updateDashboardData
    };
    
    // Manejo de filtros de reservas (si se implementan)
    const filterButtons = document.querySelectorAll('.booking-filter');
    filterButtons.forEach(button => {
        button.addEventListener('click', function() {
            const filter = this.dataset.filter;
            filterBookings(filter);
        });
    });
    
    function filterBookings(filter) {
        const bookingCards = document.querySelectorAll('.booking-card');
        bookingCards.forEach(card => {
            const status = card.dataset.status;
            if (filter === 'all' || status === filter) {
                card.style.display = 'block';
            } else {
                card.style.display = 'none';
            }
        });
        
        // Actualizar botones activos
        filterButtons.forEach(btn => btn.classList.remove('active'));
        event.target.classList.add('active');
    }
    
    // Lazy loading para imágenes (si se implementan)
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
    
    // Manejo de formularios de búsqueda rápida
    const quickSearchForm = document.getElementById('quick-search-form');
    if (quickSearchForm) {
        quickSearchForm.addEventListener('submit', function(e) {
            e.preventDefault();
            const searchTerm = this.querySelector('input[name="search"]').value;
            if (searchTerm.trim()) {
                window.location.href = `/search?q=${encodeURIComponent(searchTerm)}`;
            }
        });
    }
    
    // Animaciones de entrada para elementos del dashboard
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, observerOptions);
    
    // Observar elementos para animaciones
    const animateElements = document.querySelectorAll('.stat-card, .content-section, .quick-action-card');
    animateElements.forEach(el => {
        observer.observe(el);
    });
    
    console.log('Dashboard JavaScript inicializado correctamente');
}); 