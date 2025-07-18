// search.js - Lógica de búsqueda y renderizado de resultados
// Versión mejorada y corregida para la nueva estructura

class ProfessionalSearch {
    constructor() {
        this.apiUrl = '/api/search/professionals';
        this.currentPage = 1;
        this.totalPages = 1;
        this.lastQuery = '';
        this.isLoading = false;
        this.debounceTimer = null;
        
        this.initializeElements();
        this.bindEvents();
        this.initializeFilters();
        this.loadInitialResults();
    }

    initializeElements() {
        // Elementos principales
        this.resultsList = document.getElementById('results-list');
        this.statusDiv = document.getElementById('search-status');
        this.paginationNav = document.getElementById('pagination-nav');
        this.searchForm = document.getElementById('search-bar-form');
        this.searchQuery = document.getElementById('search-query');
        this.searchBtn = document.getElementById('search-btn');
        
        // Filtros
        this.filtersForm = document.getElementById('filters-form');
        this.specialtiesSelect = document.getElementById('specialties');
        this.locationInput = document.getElementById('location');
        this.minRating = document.getElementById('minRating');
        this.minRatingValue = document.getElementById('minRatingValue');
        this.minExperience = document.getElementById('minExperience');
        this.minExperienceValue = document.getElementById('minExperienceValue');
        this.minPrice = document.getElementById('minPrice');
        this.maxPrice = document.getElementById('maxPrice');
        this.virtualConsultation = document.getElementById('virtualConsultation');
        this.clearFiltersBtn = document.getElementById('clear-filters');
        
        // Categorías
        this.categoriesList = document.getElementById('categories-list');
        
        // Modal para móvil
        this.filtersModal = document.getElementById('filtersModal');
    }

    bindEvents() {
        // Búsqueda principal
        if (this.searchForm) {
            this.searchForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.performSearch();
            });
        }

        if (this.searchBtn) {
            this.searchBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.performSearch();
            });
        }

        // Búsqueda en tiempo real con debounce
        if (this.searchQuery) {
            this.searchQuery.addEventListener('input', (e) => {
                clearTimeout(this.debounceTimer);
                this.debounceTimer = setTimeout(() => {
                    this.performSearch();
                }, 500);
            });
        }

        // Filtros
        if (this.filtersForm) {
            // Aplicar filtros automáticamente al cambiar
            this.filtersForm.addEventListener('change', () => {
                this.currentPage = 1;
                this.performSearch();
            });
        }

        // Sliders
        if (this.minRating && this.minRatingValue) {
            this.minRating.addEventListener('input', (e) => {
                this.minRatingValue.textContent = e.target.value;
            });
        }

        if (this.minExperience && this.minExperienceValue) {
            this.minExperience.addEventListener('input', (e) => {
                this.minExperienceValue.textContent = e.target.value;
            });
        }

        // Limpiar filtros
        if (this.clearFiltersBtn) {
            this.clearFiltersBtn.addEventListener('click', () => {
                this.clearAllFilters();
            });
        }

        // Categorías
        if (this.categoriesList) {
            this.categoriesList.addEventListener('click', (e) => {
                if (e.target.closest('.category-item')) {
                    this.handleCategoryClick(e.target.closest('.category-item'));
                }
            });
        }

        // Paginación
        if (this.paginationNav) {
            this.paginationNav.addEventListener('click', (e) => {
                if (e.target.tagName === 'A' && e.target.dataset.page) {
                    e.preventDefault();
                    this.currentPage = parseInt(e.target.dataset.page);
                    this.performSearch();
                }
            });
        }

        // Responsive: botón de filtros para móvil
        this.addMobileFiltersButton();
    }

    initializeFilters() {
        // Cargar especialidades
        this.loadSpecialties();
        
        // Configurar valores iniciales de sliders
        if (this.minRating) {
            this.minRatingValue.textContent = this.minRating.value;
        }
        if (this.minExperience) {
            this.minExperienceValue.textContent = this.minExperience.value;
        }
    }

    loadSpecialties() {
        if (!this.specialtiesSelect) return;

        const specialties = [
            'Abogado', 'Contador', 'Consultor', 'Diseñador', 'Desarrollador',
            'Marketing', 'Fotógrafo', 'Traductor', 'Coach', 'Psicólogo',
            'Entrenador', 'Chef', 'Plomero', 'Electricista', 'Carpintero'
        ];

        // Limpiar opciones existentes
        this.specialtiesSelect.innerHTML = '';

        // Agregar nuevas opciones
        specialties.forEach(specialty => {
            const option = document.createElement('option');
            option.value = specialty.toLowerCase();
            option.textContent = specialty;
            this.specialtiesSelect.appendChild(option);
        });
    }

    async performSearch() {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.showLoadingState();

        try {
            const params = this.buildSearchParams();
            const url = `${this.apiUrl}?${params.toString()}`;
            
            console.log('Realizando búsqueda:', url);
            
            const response = await fetch(url);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Error HTTP:', response.status, errorText);
                throw new Error(`Error del servidor: ${response.status} - ${errorText}`);
            }
            
            const data = await response.json();
            console.log('Respuesta de búsqueda:', data);
            
            // Manejar diferentes formatos de respuesta
            const items = data.items || data || [];
            const totalCount = data.totalCount || 0;
            const totalPages = data.totalPages || Math.ceil(totalCount / 20);
            const currentPage = data.page || 1;
            
            this.renderResults(items, totalCount);
            this.renderPagination(totalPages, currentPage);
            
            if (items && items.length > 0) {
                this.hideStatus();
                console.log(`Se encontraron ${items.length} resultados de ${totalCount} totales`);
            } else {
                this.showNoResults();
                console.log('No se encontraron resultados');
            }
        } catch (error) {
            console.error('Error en la búsqueda:', error);
            this.showError(`Error al buscar profesionales: ${error.message}`);
        } finally {
            this.isLoading = false;
            this.hideLoadingState();
        }
    }

    buildSearchParams() {
        const params = new URLSearchParams();

        // Query principal
        if (this.searchQuery && this.searchQuery.value.trim()) {
            const query = this.searchQuery.value.trim();
            params.append('query', query);
            console.log('Query principal:', query);
        }

        // Especialidades
        if (this.specialtiesSelect) {
            const selectedSpecialties = Array.from(this.specialtiesSelect.selectedOptions)
                .map(option => option.value)
                .filter(value => value.trim() !== '');
            
            selectedSpecialties.forEach(specialty => {
                params.append('specialties', specialty);
            });
            
            if (selectedSpecialties.length > 0) {
                console.log('Especialidades seleccionadas:', selectedSpecialties);
            }
        }

        // Ubicación
        if (this.locationInput && this.locationInput.value.trim()) {
            const location = this.locationInput.value.trim();
            params.append('location', location);
            console.log('Ubicación:', location);
        }

        // Rango de precios
        if (this.minPrice && this.minPrice.value && this.minPrice.value > 0) {
            params.append('minHourlyRate', this.minPrice.value);
            console.log('Precio mínimo:', this.minPrice.value);
        }
        if (this.maxPrice && this.maxPrice.value && this.maxPrice.value > 0) {
            params.append('maxHourlyRate', this.maxPrice.value);
            console.log('Precio máximo:', this.maxPrice.value);
        }

        // Calificación mínima
        if (this.minRating && this.minRating.value > 0) {
            params.append('minRating', this.minRating.value);
            console.log('Calificación mínima:', this.minRating.value);
        }

        // Años de experiencia
        if (this.minExperience && this.minExperience.value > 0) {
            params.append('minExperienceYears', this.minExperience.value);
            console.log('Experiencia mínima:', this.minExperience.value);
        }

        // Consulta virtual
        if (this.virtualConsultation && this.virtualConsultation.checked) {
            params.append('virtualConsultation', 'true');
            console.log('Consulta virtual: habilitada');
        }

        // Paginación
        params.append('page', this.currentPage.toString());
        params.append('pageSize', '20');

        console.log('Parámetros de búsqueda construidos:', params.toString());
        return params;
    }

    renderResults(items, totalCount) {
        if (!this.resultsList) return;

        if (!items || items.length === 0) {
            this.resultsList.innerHTML = '';
            return;
        }

        const resultsHTML = items.map(item => this.createProfessionalCard(item)).join('');
        this.resultsList.innerHTML = resultsHTML;
    }

    createProfessionalCard(professional) {
        const rating = Math.round(professional.ratingAverage || 0);
        const stars = '★'.repeat(rating) + '☆'.repeat(5 - rating);
        const skills = professional.specialties?.slice(0, 3) || [];
        const skillsHTML = skills.map(skill => 
            `<span class="skill-tag">${skill}</span>`
        ).join('');

        return `
            <div class="professional-card ${professional.verified ? 'verified' : ''} ${professional.featured ? 'featured' : ''}">
                <div class="card-header">
                    <img src="/img/landing/image.png" 
                         alt="Foto de ${professional.fullName}" 
                         class="professional-avatar"
                         onerror="this.src='/img/default-avatar.png'">
                    <h5 class="professional-name">${professional.fullName || 'Profesional'}</h5>
                    <p class="professional-specialty">${professional.specialties?.join(', ') || 'Especialista'}</p>
                </div>
                <div class="card-body">
                    <div class="price-info">
                        <span class="price-amount">$${(professional.hourlyRate || 0).toLocaleString('es-CO')} COP</span>
                        <span class="price-unit">/hora</span>
                    </div>
                    
                    <div class="location-info">
                        <i class="fas fa-map-marker-alt location-icon"></i>
                        <span>${professional.location || 'Ubicación no especificada'}</span>
                    </div>
                    
                    <div class="rating-section">
                        <span class="rating-stars">${stars}</span>
                        <span class="rating-value">${professional.ratingAverage?.toFixed(1) || '0.0'}</span>
                        <span class="rating-count">(${professional.totalReviews || 0})</span>
                    </div>
                    
                    <div class="experience-info">
                        ${professional.experienceYears || 0} años de experiencia
                    </div>
                    
                    <div class="bio-section">
                        <p class="bio-text">${professional.bio || 'Sin descripción disponible'}</p>
                    </div>
                    
                    ${skillsHTML ? `
                        <div class="skills-section">
                            <div class="skills-list">
                                ${skillsHTML}
                            </div>
                        </div>
                    ` : ''}
                    
                    <div class="card-actions">
                        <a href="/profiles/Detail/${professional.id}" class="btn-view-profile">
                            <i class="fas fa-user me-1"></i>Ver Perfil
                        </a>
                        <button class="btn-contact" onclick="contactProfessional('${professional.id}')">
                            <i class="fas fa-envelope me-1"></i>Contactar
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    renderPagination(total, current) {
        if (!this.paginationNav) return;

        this.totalPages = total;
        
        if (total <= 1) {
            this.paginationNav.innerHTML = '';
            return;
        }

        let html = '<ul class="pagination">';
        
        // Botón anterior
        if (current > 1) {
            html += `<li class="page-item"><a class="page-link" href="#" data-page="${current - 1}">Anterior</a></li>`;
        }

        // Páginas numeradas
        const startPage = Math.max(1, current - 2);
        const endPage = Math.min(total, current + 2);

        for (let i = startPage; i <= endPage; i++) {
            html += `<li class="page-item${i === current ? ' active' : ''}">
                        <a class="page-link" href="#" data-page="${i}">${i}</a>
                     </li>`;
        }

        // Botón siguiente
        if (current < total) {
            html += `<li class="page-item"><a class="page-link" href="#" data-page="${current + 1}">Siguiente</a></li>`;
        }

        html += '</ul>';
        this.paginationNav.innerHTML = html;
    }

    handleCategoryClick(categoryElement) {
        // Remover clase active de todas las categorías
        document.querySelectorAll('.category-item').forEach(item => {
            item.classList.remove('active');
        });

        // Agregar clase active a la categoría seleccionada
        categoryElement.classList.add('active');

        // Aplicar filtro de categoría
        const category = categoryElement.dataset.category;
        this.applyCategoryFilter(category);
    }

    applyCategoryFilter(category) {
        // Mapear categorías a especialidades
        const categoryMap = {
            'diseño-gráfico': 'diseñador',
            'marketing-digital': 'marketing',
            'desarrollo-web': 'desarrollador',
            'consultoría': 'consultor',
            'fotografía': 'fotógrafo',
            'traducción': 'traductor'
        };

        const specialty = categoryMap[category];
        if (specialty && this.specialtiesSelect) {
            // Limpiar selecciones previas
            Array.from(this.specialtiesSelect.options).forEach(option => {
                option.selected = false;
            });

            // Seleccionar la especialidad correspondiente
            const targetOption = Array.from(this.specialtiesSelect.options)
                .find(option => option.value === specialty);
            if (targetOption) {
                targetOption.selected = true;
            }

            this.currentPage = 1;
            this.performSearch();
        }
    }

    clearAllFilters() {
        if (this.filtersForm) {
            this.filtersForm.reset();
        }

        if (this.searchQuery) {
            this.searchQuery.value = '';
        }

        // Resetear sliders
        if (this.minRating) {
            this.minRating.value = 1;
            this.minRatingValue.textContent = '1';
        }

        if (this.minExperience) {
            this.minExperience.value = 0;
            this.minExperienceValue.textContent = '0';
        }

        // Remover categorías activas
        document.querySelectorAll('.category-item').forEach(item => {
            item.classList.remove('active');
        });

        this.currentPage = 1;
        this.performSearch();
    }

    showLoadingState() {
        if (this.statusDiv) {
            this.statusDiv.innerHTML = '<div class="loading">Buscando profesionales...</div>';
        }
    }

    hideLoadingState() {
        // El estado se oculta automáticamente cuando se muestran los resultados
    }

    showNoResults() {
        if (this.statusDiv) {
            this.statusDiv.innerHTML = `
                <div class="no-results">
                    <div class="no-results-icon">🔍</div>
                    <h3 class="no-results-title">No se encontraron resultados</h3>
                    <p class="no-results-message">
                        Intenta ajustar tus filtros de búsqueda o usar términos diferentes.
                    </p>
                    <button class="btn btn-primary" onclick="this.clearAllFilters()">
                        Limpiar Filtros
                    </button>
                </div>
            `;
        }
    }

    showError(message) {
        if (this.statusDiv) {
            this.statusDiv.innerHTML = `
                <div class="alert alert-danger text-center">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    ${message}
                </div>
            `;
        }
    }

    hideStatus() {
        if (this.statusDiv) {
            this.statusDiv.innerHTML = '';
        }
    }

    addMobileFiltersButton() {
        // Solo agregar en móvil
        if (window.innerWidth <= 768) {
            const mobileBtn = document.createElement('button');
            mobileBtn.className = 'mobile-filters-btn';
            mobileBtn.innerHTML = '<i class="fas fa-filter"></i>';
            mobileBtn.title = 'Mostrar filtros';
            mobileBtn.onclick = () => this.showFiltersModal();
            
            document.body.appendChild(mobileBtn);
        }
    }

    showFiltersModal() {
        if (this.filtersModal) {
            const modalBody = this.filtersModal.querySelector('.modal-body');
            const filtersSidebar = document.getElementById('filters-sidebar');
            
            if (modalBody && filtersSidebar) {
                modalBody.innerHTML = filtersSidebar.outerHTML;
                const modal = new bootstrap.Modal(this.filtersModal);
                modal.show();
            }
        }
    }

    loadInitialResults() {
        // Cargar resultados iniciales
        this.performSearch();
    }
}

// Función global para contactar profesional
function contactProfessional(professionalId) {
    // Implementar lógica de contacto
    console.log('Contactando profesional:', professionalId);
    alert('Función de contacto en desarrollo');
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    // Verificar que estamos en la página de búsqueda
    if (document.getElementById('search-query')) {
        window.professionalSearch = new ProfessionalSearch();
    }
});

// Manejar cambios de tamaño de ventana para el botón móvil
window.addEventListener('resize', function() {
    const mobileBtn = document.querySelector('.mobile-filters-btn');
    if (window.innerWidth <= 768 && !mobileBtn) {
        if (window.professionalSearch) {
            window.professionalSearch.addMobileFiltersButton();
        }
    } else if (window.innerWidth > 768 && mobileBtn) {
        mobileBtn.remove();
    }
}); 