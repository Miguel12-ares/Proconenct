// JavaScript para la página de perfil detallado

document.addEventListener('DOMContentLoaded', function() {
    initializePortfolioModal();
    initializeContactModal();
    initializeShareFunctionality();
    initializeAnimations();
    initializeAccessibility();
});

// Modal de portafolio
function initializePortfolioModal() {
    const modal = document.getElementById('portfolioModal');
    const modalContent = document.getElementById('portfolioContent');
    const downloadBtn = document.getElementById('downloadPortfolio');
    
    if (!modal || !modalContent || !downloadBtn) return;

    // Cerrar modal con Escape
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                modalInstance.hide();
            }
        }
    });
}

function openPortfolioModal(fileName, fileUrl) {
    const modal = document.getElementById('portfolioModal');
    const modalContent = document.getElementById('portfolioContent');
    const downloadBtn = document.getElementById('downloadPortfolio');
    const modalTitle = document.getElementById('portfolioModalLabel');
    
    if (!modal || !modalContent || !downloadBtn || !modalTitle) return;

    // Configurar título del modal
    modalTitle.textContent = `Portafolio - ${fileName}`;
    
    // Configurar enlace de descarga
    downloadBtn.href = fileUrl;
    downloadBtn.download = fileName;
    
    // Mostrar contenido según el tipo de archivo
    const fileExtension = fileName.toLowerCase().split('.').pop();
    
    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(fileExtension)) {
        // Imagen
        modalContent.innerHTML = `
            <div class="portfolio-image-container">
                <img src="${fileUrl}" alt="${fileName}" class="img-fluid rounded" style="max-height: 70vh;" />
            </div>
        `;
    } else if (fileExtension === 'pdf') {
        // PDF
        modalContent.innerHTML = `
            <div class="portfolio-pdf-container">
                <embed src="${fileUrl}" type="application/pdf" width="100%" height="600px" />
            </div>
        `;
    } else {
        // Otros tipos de archivo
        modalContent.innerHTML = `
            <div class="portfolio-file-container text-center">
                <i class="fas fa-file fa-4x text-primary mb-3"></i>
                <h5>${fileName}</h5>
                <p class="text-muted">Archivo no previsualizable</p>
                <a href="${fileUrl}" class="btn btn-primary" download="${fileName}">
                    <i class="fas fa-download me-2"></i>Descargar archivo
                </a>
            </div>
        `;
    }
    
    // Mostrar modal
    const modalInstance = new bootstrap.Modal(modal);
    modalInstance.show();
}

// Modal de reserva
function openBookingModal() {
    const modal = document.getElementById('bookingModal');
    if (!modal) return;
    
    const modalInstance = new bootstrap.Modal(modal);
    modalInstance.show();
}

// Funcionalidad de contacto
function contactProfessional(professionalId, professionalName) {
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

// Funcionalidad de compartir
function shareProfile() {
    const currentUrl = window.location.href;
    const professionalName = document.querySelector('.profile-name')?.textContent || 'Profesional';
    
    if (navigator.share) {
        // API nativa de compartir (móviles)
        navigator.share({
            title: `${professionalName} - Perfil Profesional`,
            text: `Mira el perfil de ${professionalName} en ProConnect`,
            url: currentUrl
        }).catch(console.error);
    } else {
        // Fallback para desktop
        showShareOptions(currentUrl, professionalName);
    }
}

function showShareOptions(url, professionalName) {
    const shareData = {
        title: `${professionalName} - Perfil Profesional`,
        text: `Mira el perfil de ${professionalName} en ProConnect`,
        url: url
    };
    
    // Crear modal de opciones de compartir
    const shareModal = document.createElement('div');
    shareModal.className = 'modal fade';
    shareModal.id = 'shareModal';
    shareModal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Compartir perfil</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="share-options">
                        <button class="btn btn-outline-primary w-100 mb-2" onclick="shareOnWhatsApp('${url}', '${professionalName}')">
                            <i class="fab fa-whatsapp me-2"></i>WhatsApp
                        </button>
                        <button class="btn btn-outline-primary w-100 mb-2" onclick="shareOnFacebook('${url}', '${professionalName}')">
                            <i class="fab fa-facebook me-2"></i>Facebook
                        </button>
                        <button class="btn btn-outline-primary w-100 mb-2" onclick="shareOnTwitter('${url}', '${professionalName}')">
                            <i class="fab fa-twitter me-2"></i>Twitter
                        </button>
                        <button class="btn btn-outline-primary w-100 mb-2" onclick="shareOnLinkedIn('${url}', '${professionalName}')">
                            <i class="fab fa-linkedin me-2"></i>LinkedIn
                        </button>
                        <button class="btn btn-outline-secondary w-100" onclick="copyToClipboard('${url}')">
                            <i class="fas fa-link me-2"></i>Copiar enlace
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(shareModal);
    
    const modalInstance = new bootstrap.Modal(shareModal);
    modalInstance.show();
    
    // Remover modal del DOM después de cerrar
    shareModal.addEventListener('hidden.bs.modal', function() {
        document.body.removeChild(shareModal);
    });
}

// Funciones de compartir en redes sociales
function shareOnWhatsApp(url, professionalName) {
    const text = encodeURIComponent(`Mira el perfil de ${professionalName} en ProConnect: ${url}`);
    window.open(`https://wa.me/?text=${text}`, '_blank');
}

function shareOnFacebook(url, professionalName) {
    const shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`;
    window.open(shareUrl, '_blank', 'width=600,height=400');
}

function shareOnTwitter(url, professionalName) {
    const text = encodeURIComponent(`Mira el perfil de ${professionalName} en ProConnect`);
    const shareUrl = `https://twitter.com/intent/tweet?text=${text}&url=${encodeURIComponent(url)}`;
    window.open(shareUrl, '_blank', 'width=600,height=400');
}

function shareOnLinkedIn(url, professionalName) {
    const shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}`;
    window.open(shareUrl, '_blank', 'width=600,height=400');
}

function copyToClipboard(text) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(text).then(() => {
            showNotification('Enlace copiado al portapapeles', 'success');
        }).catch(() => {
            fallbackCopyToClipboard(text);
        });
    } else {
        fallbackCopyToClipboard(text);
    }
}

function fallbackCopyToClipboard(text) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    
    try {
        document.execCommand('copy');
        showNotification('Enlace copiado al portapapeles', 'success');
    } catch (err) {
        showNotification('Error al copiar el enlace', 'error');
    }
    
    document.body.removeChild(textArea);
}

// Animaciones
function initializeAnimations() {
    // Animar elementos al hacer scroll
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
    
    // Observar secciones de contenido
    const sections = document.querySelectorAll('.content-section, .sidebar-section');
    sections.forEach(section => observer.observe(section));
    
    // Animar estadísticas
    animateStats();
}

function animateStats() {
    const stats = document.querySelectorAll('.stat-number');
    
    stats.forEach(stat => {
        const finalValue = parseInt(stat.textContent);
        const duration = 2000; // 2 segundos
        const increment = finalValue / (duration / 16); // 60fps
        let currentValue = 0;
        
        const timer = setInterval(() => {
            currentValue += increment;
            if (currentValue >= finalValue) {
                currentValue = finalValue;
                clearInterval(timer);
            }
            stat.textContent = Math.floor(currentValue);
        }, 16);
    });
}

// Mejoras de accesibilidad
function initializeAccessibility() {
    // Navegación por teclado
    const interactiveElements = document.querySelectorAll('.portfolio-item, .service-item, .review-item');
    
    interactiveElements.forEach(element => {
        element.setAttribute('tabindex', '0');
        element.setAttribute('role', 'button');
        
        element.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });
    
    // ARIA labels
    const shareBtn = document.querySelector('button[onclick="shareProfile()"]');
    if (shareBtn) {
        shareBtn.setAttribute('aria-label', 'Compartir perfil profesional');
    }
    
    const contactBtn = document.querySelector('button[onclick*="contactProfessional"]');
    if (contactBtn) {
        contactBtn.setAttribute('aria-label', 'Contactar profesional');
    }
    
    const bookingBtn = document.querySelector('button[onclick="openBookingModal()"]');
    if (bookingBtn) {
        bookingBtn.setAttribute('aria-label', 'Reservar consulta con el profesional');
    }
}

// Notificaciones
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remover después de 3 segundos
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 3000);
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

// Optimización de performance
let resizeTimeout;
window.addEventListener('resize', function() {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(() => {
        // Reajustar layout si es necesario
        const portfolioGallery = document.querySelector('.portfolio-gallery');
        if (portfolioGallery) {
            portfolioGallery.style.height = 'auto';
        }
    }, 250);
});

// Event listeners adicionales
document.addEventListener('click', function(e) {
    // Mejorar UX para botones
    if (e.target.matches('.btn')) {
        e.target.style.transform = 'scale(0.95)';
        setTimeout(() => {
            e.target.style.transform = '';
        }, 150);
    }
});

// Exportar funciones para uso global
window.profileDetail = {
    openPortfolioModal,
    openBookingModal,
    contactProfessional,
    shareProfile,
    showNotification
}; 