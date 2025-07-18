// JavaScript para la página de resultados de profesionales

document.addEventListener('DOMContentLoaded', function() {
    initializeViewControls();
    initializeSortControls();
    initializeContactModal();
    initializeLazyLoading();
    initializeAccessibility();
});

// Control de vistas (lista/grilla)
function initializeViewControls() {
    const gridViewBtn = document.getElementById('grid-view');
    const listViewBtn = document.getElementById('list-view');
    const gridContainer = document.getElementById('grid-view-container');
    const listContainer = document.getElementById('list-view-container');

    if (!gridViewBtn || !listViewBtn) return;

    gridViewBtn.addEventListener('click', function() {
        setActiveView('grid');
        showGridView();
    });

    listViewBtn.addEventListener('click', function() {
        setActiveView('list');
        showListView();
    });

    // Guardar preferencia en localStorage
    const savedView = localStorage.getItem('professionals-view') || 'grid';
    setActiveView(savedView);
}

function setActiveView(view) {
    const gridViewBtn = document.getElementById('grid-view');
    const listViewBtn = document.getElementById('list-view');

    if (view === 'grid') {
        gridViewBtn.classList.add('active');
        listViewBtn.classList.remove('active');
        showGridView();
    } else {
        listViewBtn.classList.add('active');
        gridViewBtn.classList.remove('active');
        showListView();
    }

    localStorage.setItem('professionals-view', view);
}

function showGridView() {
    const gridContainer = document.getElementById('grid-view-container');
    const listContainer = document.getElementById('list-view-container');

    if (gridContainer) gridContainer.classList.remove('d-none');
    if (listContainer) listContainer.classList.add('d-none');

    // Animar las tarjetas
    animateCards();
}

function showListView() {
    const gridContainer = document.getElementById('grid-view-container');
    const listContainer = document.getElementById('list-view-container');

    if (gridContainer) gridContainer.classList.add('d-none');
    if (listContainer) listContainer.classList.remove('d-none');

    // Animar los elementos de lista
    animateListItems();
}

// Control de ordenamiento
function initializeSortControls() {
    const sortSelect = document.getElementById('sort-select');
    if (!sortSelect) return;

    sortSelect.addEventListener('change', function() {
        const sortValue = this.value;
        applySorting(sortValue);
    });

    // Aplicar ordenamiento inicial si hay parámetro en URL
    const urlParams = new URLSearchParams(window.location.search);
    const initialSort = urlParams.get('sort');
    if (initialSort) {
        sortSelect.value = initialSort;
        applySorting(initialSort);
    }
}

function applySorting(sortType) {
    const cards = document.querySelectorAll('.professional-card');
    const listItems = document.querySelectorAll('.professional-list-item');
    
    const elements = cards.length > 0 ? cards : listItems;
    const container = cards.length > 0 ? 
        document.getElementById('grid-view-container') : 
        document.getElementById('list-view-container');

    if (!container) return;

    // Agregar clase de carga
    container.classList.add('loading');

    // Convertir a array para ordenar
    const elementsArray = Array.from(elements);

    // Ordenar elementos
    elementsArray.sort((a, b) => {
        switch (sortType) {
            case 'rating':
                return getRating(b) - getRating(a);
            case 'price-low':
                return getPrice(a) - getPrice(b);
            case 'price-high':
                return getPrice(b) - getPrice(a);
            case 'experience':
                return getExperience(b) - getExperience(a);
            default: // relevance
                return 0; // Mantener orden original
        }
    });

    // Reorganizar elementos en el DOM
    elementsArray.forEach(element => {
        container.appendChild(element);
    });

    // Remover clase de carga después de un breve delay
    setTimeout(() => {
        container.classList.remove('loading');
        animateCards();
    }, 300);

    // Actualizar URL sin recargar la página
    updateURLParameter('sort', sortType);
}

function getRating(element) {
    const ratingText = element.querySelector('.text-muted')?.textContent;
    if (ratingText) {
        const match = ratingText.match(/(\d+\.?\d*)/);
        return match ? parseFloat(match[1]) : 0;
    }
    return 0;
}

function getPrice(element) {
    const priceText = element.querySelector('.text-muted b')?.textContent;
    if (priceText) {
        const match = priceText.match(/\$([\d,]+)/);
        return match ? parseInt(match[1].replace(/,/g, '')) : 0;
    }
    return 0;
}

function getExperience(element) {
    const expText = element.querySelector('.text-muted b')?.textContent;
    if (expText) {
        const match = expText.match(/(\d+)\s*años/);
        return match ? parseInt(match[1]) : 0;
    }
    return 0;
}

// Modal de contacto
function initializeContactModal() {
    const modal = document.getElementById('contactModal');
    const sendEmailBtn = document.getElementById('sendEmailBtn');
    
    if (!modal || !sendEmailBtn) return;

    sendEmailBtn.addEventListener('click', function() {
        const professionalName = document.getElementById('professionalName').textContent;
        const professionalId = this.dataset.professionalId;
        
        sendContactEmail(professionalName, professionalId);
        
        // Cerrar modal
        const modalInstance = bootstrap.Modal.getInstance(modal);
        if (modalInstance) {
            modalInstance.hide();
        }
    });
}

function contactProfessional(professionalId, professionalName) {
    const modal = document.getElementById('contactModal');
    const nameElement = document.getElementById('professionalName');
    const sendBtn = document.getElementById('sendEmailBtn');
    
    if (!modal || !nameElement || !sendBtn) return;

    nameElement.textContent = professionalName;
    sendBtn.dataset.professionalId = professionalId;
    
    const modalInstance = new bootstrap.Modal(modal);
    modalInstance.show();
}

function sendContactEmail(professionalName, professionalId) {
    const subject = encodeURIComponent(`Consulta sobre servicios - ${professionalName}`);
    const body = encodeURIComponent(
        `Hola ${professionalName},\n\n` +
        `Me interesa conocer más sobre tus servicios profesionales.\n\n` +
        `Por favor, comparte conmigo:\n` +
        `- Información sobre tu experiencia\n` +
        `- Tarifas y disponibilidad\n` +
        `- Proceso de trabajo\n\n` +
        `Saludos cordiales,\n` +
        `[Tu nombre]`
    );
    
    const mailtoLink = `mailto:?subject=${subject}&body=${body}`;
    window.open(mailtoLink, '_blank');
}

// Lazy loading para imágenes
function initializeLazyLoading() {
    const images = document.querySelectorAll('img[src="/img/landing/image.png"]');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.classList.add('fade-in');
                    observer.unobserve(img);
                }
            });
        });

        images.forEach(img => imageObserver.observe(img));
    }
}

// Mejoras de accesibilidad
function initializeAccessibility() {
    // Navegación por teclado en tarjetas
    const cards = document.querySelectorAll('.professional-card, .professional-list-item');
    
    cards.forEach(card => {
        card.setAttribute('tabindex', '0');
        card.setAttribute('role', 'button');
        
        card.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                const profileLink = this.querySelector('a[href*="/profiles/Detail/"]');
                if (profileLink) {
                    profileLink.click();
                }
            }
        });
    });

    // ARIA labels para botones
    const viewButtons = document.querySelectorAll('.view-options .btn');
    viewButtons.forEach(btn => {
        const view = btn.dataset.view;
        btn.setAttribute('aria-label', `Cambiar a vista de ${view === 'grid' ? 'cuadrícula' : 'lista'}`);
    });
}

// Animaciones
function animateCards() {
    const cards = document.querySelectorAll('.professional-card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('animate-card');
    });
}

function animateListItems() {
    const items = document.querySelectorAll('.professional-list-item');
    items.forEach((item, index) => {
        item.style.animationDelay = `${index * 0.1}s`;
        item.classList.add('animate-list-item');
    });
}

// Utilidades
function updateURLParameter(key, value) {
    const url = new URL(window.location);
    if (value) {
        url.searchParams.set(key, value);
    } else {
        url.searchParams.delete(key);
    }
    window.history.replaceState({}, '', url);
}

// Función para mostrar notificaciones
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remover después de 5 segundos
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Función para manejar errores
function handleError(error, context = '') {
    console.error(`Error en ${context}:`, error);
    showNotification(`Ha ocurrido un error: ${error.message}`, 'danger');
}

// Event listeners adicionales
document.addEventListener('click', function(e) {
    // Mejorar UX para botones de contacto
    if (e.target.matches('.btn-outline-primary')) {
        e.target.style.transform = 'scale(0.95)';
        setTimeout(() => {
            e.target.style.transform = '';
        }, 150);
    }
});

// Optimización de performance
let resizeTimeout;
window.addEventListener('resize', function() {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(() => {
        // Reajustar layout si es necesario
        const cards = document.querySelectorAll('.professional-card');
        cards.forEach(card => {
            card.style.height = 'auto';
        });
    }, 250);
});

// Exportar funciones para uso global
window.professionalsResults = {
    contactProfessional,
    showNotification,
    handleError
}; 