/**
 * BOOKING MODULE - MAIN JAVASCRIPT
 * Funcionalidad principal para el sistema de reservas
 */

class BookingManager {
    constructor() {
        this.selectedDate = null;
        this.selectedTimeSlot = null;
        this.professionalId = null;
        this.calendar = null;
        this.availabilityData = {};
        this.isLoading = false;
        
        this.init();
    }

    /**
     * Inicializa el gestor de reservas
     */
    init() {
        this.professionalId = this.getProfessionalIdFromUrl();
        this.initializeCalendar();
        this.bindEvents();
        this.loadInitialData();
    }

    /**
     * Obtiene el ID del profesional desde la URL
     */
    getProfessionalIdFromUrl() {
        const pathParts = window.location.pathname.split('/');
        return pathParts[pathParts.length - 1];
    }

    /**
     * Inicializa el calendario FullCalendar
     */
    initializeCalendar() {
        const calendarEl = document.getElementById('calendar');
        if (!calendarEl) return;

        this.calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            locale: 'es',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth'
            },
            selectable: true,
            selectMirror: true,
            dayMaxEvents: true,
            weekends: true,
            height: 'auto',
            
            // Eventos del calendario
            dateClick: (info) => this.handleDateClick(info),
            datesSet: (info) => this.handleDatesSet(info),
            
            // Personalización de días
            dayCellClassNames: (info) => this.getDayCellClasses(info),
            dayCellContent: (info) => this.getDayCellContent(info)
        });

        this.calendar.render();
    }

    /**
     * Vincula eventos del DOM
     */
    bindEvents() {
        // Formulario de reserva
        const bookingForm = document.getElementById('booking-form');
        if (bookingForm) {
            bookingForm.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }

        // Modal de confirmación
        const confirmBtn = document.getElementById('confirm-booking-btn');
        const cancelBtn = document.getElementById('cancel-booking-btn');
        
        if (confirmBtn) {
            confirmBtn.addEventListener('click', () => this.confirmBooking());
        }
        
        if (cancelBtn) {
            cancelBtn.addEventListener('click', () => this.hideConfirmationModal());
        }

        // Cerrar modal con escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.hideConfirmationModal();
            }
        });

        // Validación en tiempo real
        this.bindFormValidation();
    }

    /**
     * Carga datos iniciales
     */
    async loadInitialData() {
        try {
            this.showLoading('calendar-section');
            await this.loadAvailability();
            this.hideLoading('calendar-section');
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showError('Error al cargar la disponibilidad');
            this.hideLoading('calendar-section');
        }
    }

    /**
     * Carga la disponibilidad del profesional
     */
    async loadAvailability(startDate = null, endDate = null) {
        if (!startDate) {
            const now = new Date();
            startDate = new Date(now.getFullYear(), now.getMonth(), 1);
            endDate = new Date(now.getFullYear(), now.getMonth() + 2, 0);
        }

        try {
            const response = await axios.get(`/api/professionals/${this.professionalId}/availability`, {
                params: {
                    startDate: startDate.toISOString().split('T')[0],
                    endDate: endDate.toISOString().split('T')[0]
                }
            });

            this.availabilityData = response.data;
            this.updateCalendarDisplay();
        } catch (error) {
            console.error('Error loading availability:', error);
            throw error;
        }
    }

    /**
     * Maneja el clic en una fecha del calendario
     */
    async handleDateClick(info) {
        // Usar fecha local para evitar desfases por zona horaria
        const dateStr = info.dateStr;
        const [year, month, day] = dateStr.split('-');
        const clickedDate = new Date(year, month - 1, day);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // No permitir fechas pasadas
        if (clickedDate < today) {
            this.showError('No puedes seleccionar fechas pasadas');
            return;
        }

        // Verificar disponibilidad
        const dayAvailability = this.availabilityData[dateStr];
        if (!dayAvailability || dayAvailability.length === 0) {
            this.showError('No hay horarios disponibles para esta fecha');
            return;
        }

        // Actualizar fecha seleccionada
        this.selectedDate = dateStr;
        this.selectedTimeSlot = null;
        // Actualizar UI
        this.updateSelectedDateDisplay();
        await this.loadTimeSlots(dateStr);
        this.updateCalendarSelection();
    }

    /**
     * Maneja el cambio de vista del calendario
     */
    async handleDatesSet(info) {
        try {
            await this.loadAvailability(info.start, info.end);
        } catch (error) {
            console.error('Error loading availability for date range:', error);
        }
    }

    /**
     * Obtiene las clases CSS para las celdas del calendario
     */
    getDayCellClasses(info) {
        const dateStr = info.date.toISOString().split('T')[0];
        const dayAvailability = this.availabilityData[dateStr];
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        const classes = [];
        
        if (info.date < today) {
            classes.push('fc-day-past');
        } else if (dayAvailability && dayAvailability.length > 0) {
            const availableSlots = dayAvailability.filter(slot => slot.isAvailable);
            if (availableSlots.length === dayAvailability.length) {
                classes.push('available');
            } else if (availableSlots.length > 0) {
                classes.push('partially-available');
            } else {
                classes.push('unavailable');
            }
        } else {
            classes.push('unavailable');
        }
        
        if (dateStr === this.selectedDate) {
            classes.push('selected');
        }
        
        return classes;
    }

    /**
     * Obtiene el contenido personalizado para las celdas del calendario
     */
    getDayCellContent(info) {
        const dateStr = info.date.toISOString().split('T')[0];
        const dayAvailability = this.availabilityData[dateStr];
        
        let indicatorClass = 'unavailable';
        if (dayAvailability && dayAvailability.length > 0) {
            const availableSlots = dayAvailability.filter(slot => slot.isAvailable);
            if (availableSlots.length === dayAvailability.length) {
                indicatorClass = 'available';
            } else if (availableSlots.length > 0) {
                indicatorClass = 'partially-available';
            }
        }
        
        return {
            html: `
                <div class="fc-daygrid-day-number">${info.dayNumberText}</div>
                <div class="availability-indicator ${indicatorClass}"></div>
            `
        };
    }

    async loadTimeSlots(dateStr) {
    const timeSlotsContainer = document.getElementById('time-slots-grid');
    const selectedDateDisplay = document.getElementById('selected-date-display');
    
    if (!timeSlotsContainer) return;

    try {
        this.showLoading('time-slots-section');
        
        // Formatear la fecha seleccionada de manera consistente (usando formatDate)
        if (selectedDateDisplay) {
            selectedDateDisplay.textContent = this.formatDate(dateStr);
        }
        
        const dayAvailability = this.availabilityData[dateStr];
        if (!dayAvailability || dayAvailability.length === 0) {
            this.showNoTimeSlots('No hay horarios disponibles para esta fecha');
            return;
        }

        // Generate time slots in 30-minute intervals based on availability
        const timeSlotsHtml = this.generateTimeSlots(dayAvailability);
        
        timeSlotsContainer.innerHTML = timeSlotsHtml;
        
        // Bind click events to time slots
        this.bindTimeSlotEvents();
        
        this.hideLoading('time-slots-section');
    } catch (error) {
        console.error('Error loading time slots:', error);
        this.showError('Error al cargar los horarios');
        this.hideLoading('time-slots-section');
    }
}

/**
 * Generates time slots in 30-minute intervals based on availability
 */
generateTimeSlots(dayAvailability) {
    // First, sort the availability by start time
    const sortedSlots = [...dayAvailability].sort((a, b) => 
        a.startTime.localeCompare(b.startTime)
    );

    let slotsHtml = '';

    // Process each available time slot
    sortedSlots.forEach(slot => {
        if (!slot.isAvailable) return;

        const startTime = new Date(`2000-01-01T${slot.startTime}`);
        const endTime = new Date(`2000-01-01T${slot.endTime || '23:59'}`);
        
        // Create 30-minute intervals within the available time slot
        let currentTime = new Date(startTime);
        
        while (currentTime < endTime) {
            const slotEndTime = new Date(currentTime.getTime() + 30 * 60000); // Add 30 minutes
            
            // Format time for display (HH:MM)
            const timeStr = currentTime.toTimeString().substring(0, 5);
            
            // Add the time slot if it fits within the available time
            if (slotEndTime <= endTime) {
                slotsHtml += `
                    <div class="time-slot available" 
                         data-time="${timeStr}" 
                         data-duration="30">
                        ${timeStr}
                    </div>
                `;
            }
            
            // Move to next 30-minute slot
            currentTime = slotEndTime;
        }
    });

    // If no slots were generated, show a message
    if (!slotsHtml) {
        return `
            <div class="no-time-slots">
                <i class="fas fa-calendar-times"></i>
                <h3>No hay horarios disponibles</h3>
                <p>No hay horarios disponibles para la fecha seleccionada</p>
            </div>
        `;
    }

    return slotsHtml;
}

/**
 * Updates the UI when no time slots are available
 */
showNoTimeSlots(message = 'No hay horarios disponibles para esta fecha') {
    const timeSlotsContainer = document.getElementById('time-slots-grid');
    if (!timeSlotsContainer) return;
    
    timeSlotsContainer.innerHTML = `
        <div class="no-time-slots">
            <i class="fas fa-calendar-times"></i>
            <h3>Sin disponibilidad</h3>
            <p>${message}</p>
        </div>
    `;
}

    /**
     * Obtiene la clase CSS para un slot de tiempo
     */
    getTimeSlotClass(slot) {
        if (!slot.isAvailable) {
            return slot.isOccupied ? 'occupied' : 'unavailable';
        }
        return 'available';
    }

    /**
     * Vincula eventos a los slots de tiempo
     */
    bindTimeSlotEvents() {
        const timeSlots = document.querySelectorAll('.time-slot.available');
        timeSlots.forEach(slot => {
            slot.addEventListener('click', () => this.selectTimeSlot(slot));
        });
    }

    /**
     * Selecciona un slot de tiempo
     */
    selectTimeSlot(slotElement) {
        // Remover selección anterior
        document.querySelectorAll('.time-slot.selected').forEach(slot => {
            slot.classList.remove('selected');
        });

        // Seleccionar nuevo slot
        slotElement.classList.add('selected');
        
        this.selectedTimeSlot = {
            time: slotElement.dataset.time,
            duration: parseInt(slotElement.dataset.duration)
        };

        // Actualizar formulario
        this.updateBookingForm();
        
        // Mostrar formulario si estaba oculto
        this.showBookingForm();
    }

    /**
     * Actualiza el formulario de reserva con los datos seleccionados
     */
    updateBookingForm() {
        if (!this.selectedDate || !this.selectedTimeSlot) return;

        // Actualizar campos del formulario
        const dateInput = document.getElementById('appointment-date');
        const timeInput = document.getElementById('appointment-time');
        
        if (dateInput) dateInput.value = this.selectedDate;
        if (timeInput) timeInput.value = this.selectedTimeSlot.time;

        // Actualizar resumen
        this.updateBookingSummary();
    }

    /**
     * Actualiza el resumen de la reserva
     */
    updateBookingSummary() {
        const summaryDate = document.getElementById('summary-date');
        const summaryTime = document.getElementById('summary-time');
        const summaryDuration = document.getElementById('summary-duration');
        
        if (summaryDate && this.selectedDate) {
            summaryDate.textContent = this.formatDate(this.selectedDate);
        }
        
        if (summaryTime && this.selectedTimeSlot) {
            summaryTime.textContent = this.formatTime(this.selectedTimeSlot.time);
        }
        
        if (summaryDuration && this.selectedTimeSlot) {
            summaryDuration.textContent = `${this.selectedTimeSlot.duration} minutos`;
        }
    }

    /**
     * Maneja el envío del formulario
     */
    async handleFormSubmit(e) {
        e.preventDefault();
        
        if (!this.validateForm()) {
            return;
        }

        // Mostrar modal de confirmación
        this.showConfirmationModal();
    }

    /**
     * Valida el formulario de reserva
     */
    validateForm() {
        const form = document.getElementById('booking-form');
        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                this.showFieldError(field, 'Este campo es obligatorio');
                isValid = false;
            } else {
                this.clearFieldError(field);
            }
        });

        if (!this.selectedDate || !this.selectedTimeSlot) {
            this.showError('Por favor selecciona una fecha y horario');
            isValid = false;
        }

        return isValid;
    }

    /**
     * Confirma la reserva
     */
    async confirmBooking() {
        try {
            this.showLoadingOverlay('Procesando reserva...');
            const formData = this.getFormData();
            const response = await axios.post('/api/Booking', formData);

            // Considerar éxito si status 201/200 y hay id de reserva
            if ((response.status === 201 || response.status === 200) && response.data && response.data.id) {
                window.location.href = `/booking/confirmation/${response.data.id}`;
            } else if (response.data && response.data.error) {
                // Mostrar mensaje de error específico del backend
                throw new Error(response.data.error);
            } else {
                throw new Error(response.data?.message || 'Error al procesar la reserva');
            }
        } catch (error) {
            console.error('Error confirming booking:', error);
            this.hideLoadingOverlay();
            // Mostrar mensaje de error claro al usuario
            const mensaje = error.response?.data?.error || error.response?.data?.message || error.message || 'Error al procesar la reserva';
            this.showError(mensaje);
        }
    }

    getFormData() {
        const form = document.getElementById('booking-form');
        const formData = new FormData(form);
        // Unir fecha y hora seleccionada en formato ISO
        const date = this.selectedDate; // yyyy-MM-dd
        const time = this.selectedTimeSlot.time; // HH:mm
        const appointmentDateTime = new Date(`${date}T${time}`).toISOString();

        return {
            ProfessionalId: this.professionalId,
            AppointmentDate: appointmentDateTime,
            Duration: this.selectedTimeSlot.duration,
            ConsultationType: formData.get('consultation-type'),
            Notes: formData.get('special-notes'),
            ClientPhone: formData.get('client-phone'),
            ClientEmail: formData.get('client-email')
        };
    }

    // Métodos de utilidad UI
    showLoading(sectionId) {
        const section = document.getElementById(sectionId);
        if (section) {
            section.classList.add('loading-state');
        }
    }

    hideLoading(sectionId) {
        const section = document.getElementById(sectionId);
        if (section) {
            section.classList.remove('loading-state');
        }
    }

    showLoadingOverlay(message = 'Cargando...') {
        const overlay = document.getElementById('loading-overlay');
        const messageEl = document.getElementById('loading-message');
        
        if (overlay) {
            if (messageEl) messageEl.textContent = message;
            overlay.style.display = 'flex';
        }
    }

    hideLoadingOverlay() {
        const overlay = document.getElementById('loading-overlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
    }

    showConfirmationModal() {
        const modal = document.getElementById('confirmation-modal');
        if (modal) {
            this.updateConfirmationModal();
            modal.classList.add('show');
        }
    }

    hideConfirmationModal() {
        const modal = document.getElementById('confirmation-modal');
        if (modal) {
            modal.classList.remove('show');
        }
    }

    updateConfirmationModal() {
        // Actualizar detalles en el modal
        const formData = this.getFormData();
        
        document.getElementById('confirm-date').textContent = this.formatDate(this.selectedDate);
        document.getElementById('confirm-time').textContent = this.formatTime(this.selectedTimeSlot.time);
        document.getElementById('confirm-duration').textContent = `${this.selectedTimeSlot.duration} minutos`;
        document.getElementById('confirm-type').textContent = formData.consultationType;
        document.getElementById('confirm-notes').textContent = formData.specialNotes || 'Ninguna';
    }

    showError(message) {
        // Implementar sistema de notificaciones toast
        console.error(message);
        alert(message); // Temporal
    }

    showFieldError(field, message) {
        field.classList.add('has-error');
        let errorEl = field.parentNode.querySelector('.error-message');
        if (!errorEl) {
            errorEl = document.createElement('span');
            errorEl.className = 'error-message';
            field.parentNode.appendChild(errorEl);
        }
        errorEl.textContent = message;
    }

    clearFieldError(field) {
        field.classList.remove('has-error');
        const errorEl = field.parentNode.querySelector('.error-message');
        if (errorEl) {
            errorEl.remove();
        }
    }

    // Métodos de formato
    formatDate(dateStr) {
        // Forzar a UTC para evitar desfase por zona horaria local
        const [year, month, day] = dateStr.split('-');
        const date = new Date(Date.UTC(year, month - 1, day));
        return date.toLocaleDateString('es-ES', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            timeZone: 'UTC'
        });
    }

    formatTime(timeStr) {
        const [hours, minutes] = timeStr.split(':');
        const date = new Date();
        date.setHours(parseInt(hours), parseInt(minutes));
        return date.toLocaleTimeString('es-ES', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    updateSelectedDateDisplay() {
        const display = document.getElementById('selected-date-display');
        if (display && this.selectedDate) {
            display.textContent = this.formatDate(this.selectedDate);
        }
    }

    updateCalendarDisplay() {
        if (this.calendar) {
            this.calendar.refetchEvents();
        }
    }

    updateCalendarSelection() {
        // Forzar re-render del calendario para actualizar clases
        if (this.calendar) {
            this.calendar.render();
        }
    }

    showBookingForm() {
        const formSection = document.getElementById('booking-form-section');
        if (formSection) {
            formSection.style.display = 'block';
            formSection.scrollIntoView({ behavior: 'smooth' });
        }
    }

    showNoTimeSlots() {
        const container = document.getElementById('time-slots-grid');
        if (container) {
            container.innerHTML = `
                <div class="no-time-slots">
                    <i class="fas fa-calendar-times"></i>
                    <h3>No hay horarios disponibles</h3>
                    <p>Por favor selecciona otra fecha</p>
                </div>
            `;
        }
    }

    bindFormValidation() {
        const form = document.getElementById('booking-form');
        if (!form) return;

        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });
    }

    validateField(field) {
        if (field.hasAttribute('required') && !field.value.trim()) {
            this.showFieldError(field, 'Este campo es obligatorio');
            return false;
        }

        if (field.type === 'email' && field.value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(field.value)) {
                this.showFieldError(field, 'Por favor ingresa un email válido');
                return false;
            }
        }

        if (field.type === 'tel' && field.value) {
            const phoneRegex = /^[\d\s\-\+\(\)]+$/;
            if (!phoneRegex.test(field.value)) {
                this.showFieldError(field, 'Por favor ingresa un teléfono válido');
                return false;
            }
        }

        this.clearFieldError(field);
        return true;
    }
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('calendar')) {
        window.bookingManager = new BookingManager();
    }
});
