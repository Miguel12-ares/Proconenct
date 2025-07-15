using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProConnect.Core.Entities
{
    /// <summary>
    /// Representa el perfil profesional extendido de un usuario tipo Profesional.
    /// </summary>
    public class ProfessionalProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Referencia al usuario propietario del perfil (User.Id)
        /// </summary>
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Lista de especialidades del profesional.
        /// </summary>
        [BsonElement("specialties")]
        [Required]
        [MinLength(1, ErrorMessage = "Debe tener al menos una especialidad")]
        public List<string> Specialties { get; set; } = new();

        /// <summary>
        /// Biografía profesional (mínimo 100 caracteres).
        /// </summary>
        [BsonElement("bio")]
        [Required]
        [MinLength(100, ErrorMessage = "La biografía debe tener al menos 100 caracteres")]
        [MaxLength(2000, ErrorMessage = "La biografía no puede exceder 2000 caracteres")]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// Años de experiencia profesional.
        /// </summary>
        [BsonElement("experience_years")]
        [Range(0, 50, ErrorMessage = "Los años de experiencia deben estar entre 0 y 50")]
        public int ExperienceYears { get; set; }

        /// <summary>
        /// Tarifa por hora (USD).
        /// </summary>
        [BsonElement("hourly_rate")]
        [Range(1, 10000, ErrorMessage = "La tarifa por hora debe estar entre $1 y $10,000")]
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Lista de credenciales (títulos, certificaciones, etc.).
        /// </summary>
        [BsonElement("credentials")]
        public List<string> Credentials { get; set; } = new();

        /// <summary>
        /// Portafolio de trabajos previos (pueden ser URLs o descripciones).
        /// </summary>
        [BsonElement("portfolio_items")]
        public List<PortfolioItem> PortfolioItems { get; set; } = new();

        /// <summary>
        /// Promedio de calificación (1-5).
        /// </summary>
        [BsonElement("rating_average")]
        [Range(0, 5, ErrorMessage = "El promedio de calificación debe estar entre 0 y 5")]
        public double RatingAverage { get; set; }

        /// <summary>
        /// Total de reseñas recibidas.
        /// </summary>
        [BsonElement("total_reviews")]
        [Range(0, int.MaxValue, ErrorMessage = "El total de reseñas no puede ser negativo")]
        public int TotalReviews { get; set; }

        /// <summary>
        /// Ubicación principal del profesional.
        /// </summary>
        [BsonElement("location")]
        [Required]
        [MinLength(3, ErrorMessage = "La ubicación debe tener al menos 3 caracteres")]
        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Horario de disponibilidad (estructura flexible, puede ser JSON o lista de objetos).
        /// </summary>
        [BsonElement("availability_schedule")]
        public AvailabilitySchedule AvailabilitySchedule { get; set; } = new();

        /// <summary>
        /// Lista de bloqueos de disponibilidad (fechas específicas no disponibles).
        /// </summary>
        [BsonElement("availability_blocks")]
        public List<AvailabilityBlock> AvailabilityBlocks { get; set; } = new();

        /// <summary>
        /// Estado del perfil profesional.
        /// </summary>
        [BsonElement("status")]
        public ProfileStatus Status { get; set; } = ProfileStatus.Draft;

        /// <summary>
        /// Fecha de creación del perfil.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización del perfil.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Valida si el perfil está completo para ser visible públicamente.
        /// </summary>
        public bool IsCompleteForPublicView()
        {
            return !string.IsNullOrWhiteSpace(Bio) &&
                   Bio.Length >= 100 &&
                   Specialties.Count > 0 &&
                   !string.IsNullOrWhiteSpace(Location) &&
                   HourlyRate > 0 &&
                   Status == ProfileStatus.Active;
        }

        /// <summary>
        /// Actualiza la fecha de modificación.
        /// </summary>
        public void UpdateModificationDate()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Representa un item del portafolio del profesional.
    /// </summary>
    public class PortfolioItem
    {
        [BsonElement("title")]
        [Required]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("url")]
        [MaxLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
        public string? Url { get; set; }

        [BsonElement("image_url")]
        [MaxLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
        public string? ImageUrl { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Representa el horario de disponibilidad del profesional.
    /// </summary>
    public class AvailabilitySchedule
    {
        [BsonElement("monday")]
        public DaySchedule Monday { get; set; } = new();

        [BsonElement("tuesday")]
        public DaySchedule Tuesday { get; set; } = new();

        [BsonElement("wednesday")]
        public DaySchedule Wednesday { get; set; } = new();

        [BsonElement("thursday")]
        public DaySchedule Thursday { get; set; } = new();

        [BsonElement("friday")]
        public DaySchedule Friday { get; set; } = new();

        [BsonElement("saturday")]
        public DaySchedule Saturday { get; set; } = new();

        [BsonElement("sunday")]
        public DaySchedule Sunday { get; set; } = new();

        [BsonElement("timezone")]
        public string Timezone { get; set; } = "UTC";
    }

    /// <summary>
    /// Representa el horario de un día específico.
    /// </summary>
    public class DaySchedule
    {
        [BsonElement("is_available")]
        public bool IsAvailable { get; set; } = false;

        [BsonElement("start_time")]
        public string StartTime { get; set; } = "09:00";

        [BsonElement("end_time")]
        public string EndTime { get; set; } = "17:00";

        [BsonElement("break_start")]
        public string? BreakStart { get; set; }

        [BsonElement("break_end")]
        public string? BreakEnd { get; set; }
    }

    /// <summary>
    /// Representa un bloqueo de disponibilidad para fechas específicas.
    /// </summary>
    public class AvailabilityBlock
    {
        [BsonElement("id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("start_date")]
        public DateTime StartDate { get; set; }

        [BsonElement("end_date")]
        public DateTime EndDate { get; set; }

        [BsonElement("reason")]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Estados posibles del perfil profesional.
    /// </summary>
    public enum ProfileStatus
    {
        Draft = 0,      // Borrador, no visible públicamente
        Active = 1,     // Activo y visible
        Inactive = 2,   // Inactivo temporalmente
        Suspended = 3   // Suspendido por moderación
    }
} 