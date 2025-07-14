using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

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
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Lista de especialidades del profesional.
        /// </summary>
        [BsonElement("specialties")]
        public List<string> Specialties { get; set; } = new();

        /// <summary>
        /// Biografía profesional (mínimo 100 caracteres).
        /// </summary>
        [BsonElement("bio")]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// Años de experiencia profesional.
        /// </summary>
        [BsonElement("experience_years")]
        public int ExperienceYears { get; set; }

        /// <summary>
        /// Tarifa por hora (USD).
        /// </summary>
        [BsonElement("hourly_rate")]
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
        public List<string> PortfolioItems { get; set; } = new();

        /// <summary>
        /// Promedio de calificación (1-5).
        /// </summary>
        [BsonElement("rating_average")]
        public double RatingAverage { get; set; }

        /// <summary>
        /// Total de reseñas recibidas.
        /// </summary>
        [BsonElement("total_reviews")]
        public int TotalReviews { get; set; }

        /// <summary>
        /// Ubicación principal del profesional.
        /// </summary>
        [BsonElement("location")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Horario de disponibilidad (estructura flexible, puede ser JSON o lista de objetos).
        /// </summary>
        [BsonElement("availability_schedule")]
        public string AvailabilitySchedule { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 