using System.ComponentModel.DataAnnotations;
using ProConnect.Application.DTOs.Shared;

namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo perfil profesional.
    /// </summary>
    public class CreateProfessionalProfileDto
    {
        /// <summary>
        /// Lista de especialidades del profesional.
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar al menos una especialidad")]
        [MinLength(1, ErrorMessage = "Debe tener al menos una especialidad")]
        public List<string> Specialties { get; set; } = new();

        /// <summary>
        /// Biografía profesional.
        /// </summary>
        [Required(ErrorMessage = "La biografía es obligatoria")]
        [MinLength(100, ErrorMessage = "La biografía debe tener al menos 100 caracteres")]
        [MaxLength(2000, ErrorMessage = "La biografía no puede exceder 2000 caracteres")]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// Años de experiencia profesional.
        /// </summary>
        [Range(0, 50, ErrorMessage = "Los años de experiencia deben estar entre 0 y 50")]
        public int ExperienceYears { get; set; }

        /// <summary>
        /// Tarifa por hora (USD).
        /// </summary>
        [Required(ErrorMessage = "La tarifa por hora es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La tarifa por hora debe estar entre $1 y $10,000")]
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Lista de credenciales.
        /// </summary>
        public List<string> Credentials { get; set; } = new();

        /// <summary>
        /// Ubicación principal del profesional.
        /// </summary>
        [Required(ErrorMessage = "La ubicación es obligatoria")]
        [MinLength(3, ErrorMessage = "La ubicación debe tener al menos 3 caracteres")]
        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Horario de disponibilidad.
        /// </summary>
        public AvailabilityScheduleDto AvailabilitySchedule { get; set; } = new();
    }
} 