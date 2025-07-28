using ProConnect.Core.Entities;
using ProConnect.Application.DTOs.Shared;

namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO de respuesta para perfiles profesionales.
    /// </summary>
    public class ProfessionalProfileResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> Specialties { get; set; } = new();
        public string Bio { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal HourlyRate { get; set; }
        public List<string> Credentials { get; set; } = new();
        public List<PortfolioItemDto> PortfolioItems { get; set; } = new();
        public double RatingAverage { get; set; }
        public int TotalReviews { get; set; }
        public string Location { get; set; } = string.Empty;
        public AvailabilityScheduleDto AvailabilitySchedule { get; set; } = new();
        public ProfileStatusDto Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsCompleteForPublicView { get; set; }
        public int PortfolioFilesCount { get; set; }
        public List<string> Services { get; set; } = new();

        // Información del usuario (solo en vista privada)
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? UserEmail { get; set; }

        // Propiedades adicionales necesarias para el módulo de reservas
        public string Name => $"{UserFirstName} {UserLastName}".Trim();
        public string TimeZone => AvailabilitySchedule?.Timezone ?? "UTC";
        public bool IsAvailableForBooking => Status == ProfileStatusDto.Active && IsCompleteForPublicView;
        
        // Propiedades adicionales para las vistas
        public string? ProfileImageUrl { get; set; }
        public string Specialty => Specialties.FirstOrDefault() ?? "";
        public double AverageRating => RatingAverage;
        public int ReviewCount => TotalReviews;
        public string PhoneNumber { get; set; } = string.Empty;

        // Propiedad para indicar si es vista pública
        public bool IsPublicView { get; set; } = false;
    }

    /// <summary>
    /// DTO para items del portafolio.
    /// </summary>
    public class PortfolioItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 