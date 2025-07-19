using System.ComponentModel.DataAnnotations;

namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar una reserva existente
    /// </summary>
    public class UpdateBookingDto
    {
        /// <summary>
        /// Nueva fecha y hora de la cita (opcional)
        /// </summary>
        public DateTime? AppointmentDate { get; set; }

        /// <summary>
        /// Nueva duración de la cita en minutos (opcional)
        /// </summary>
        [Range(15, 480, ErrorMessage = "La duración debe estar entre 15 y 480 minutos")]
        public int? AppointmentDuration { get; set; }

        /// <summary>
        /// Nuevo tipo de consulta (opcional)
        /// </summary>
        public string? ConsultationType { get; set; }

        /// <summary>
        /// Nuevas observaciones especiales (opcional)
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Las notas especiales no pueden exceder 1000 caracteres")]
        public string? SpecialNotes { get; set; }

        /// <summary>
        /// Nuevo estado de la reserva (opcional)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Razón de cancelación (solo si se cancela)
        /// </summary>
        [MaxLength(500, ErrorMessage = "La razón de cancelación no puede exceder 500 caracteres")]
        public string? CancellationReason { get; set; }
    }
} 