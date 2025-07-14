namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO para exponer el perfil profesional extendido.
    /// </summary>
    public class ProfessionalProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> Specialties { get; set; } = new();
        public string Bio { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal HourlyRate { get; set; }
        public List<string> Credentials { get; set; } = new();
        public List<string> PortfolioItems { get; set; } = new();
        public double RatingAverage { get; set; }
        public int TotalReviews { get; set; }
        public string Location { get; set; } = string.Empty;
        public string AvailabilitySchedule { get; set; } = string.Empty;
    }
} 