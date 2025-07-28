using System.ComponentModel.DataAnnotations;

namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO para crear una nueva reserva
    /// </summary>
    public class CreateBookingDto
    {
        /// <summary>
        /// ID del cliente que realiza la reserva (se asigna internamente en el backend)
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// ID del profesional con quien se realiza la reserva
        /// </summary>
        [Required(ErrorMessage = "El ID del profesional es requerido")]
        public string ProfessionalId { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de la cita
        /// </summary>
        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Duración de la cita en minutos (default: 60)
        /// </summary>
        [Range(15, 480, ErrorMessage = "La duración debe estar entre 15 y 480 minutos")]
        public int Duration { get; set; } = 60;

        /// <summary>
        /// Tipo de consulta
        /// </summary>
        [Required(ErrorMessage = "El tipo de consulta es requerido")]
        public string ConsultationType { get; set; } = "VideoCall";

        /// <summary>
        /// Observaciones especiales del cliente (opcional)
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Las notas especiales no pueden exceder 1000 caracteres")]
        public string? Notes { get; set; }

        /// <summary>
        /// Teléfono del cliente (opcional)
        /// </summary>
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string? ClientPhone { get; set; }

        /// <summary>
        /// Email del cliente (opcional)
        /// </summary>
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? ClientEmail { get; set; }
    }
} 