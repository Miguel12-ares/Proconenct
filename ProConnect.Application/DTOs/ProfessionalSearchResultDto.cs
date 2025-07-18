using System.Collections.Generic;

namespace ProConnect.Application.DTOs
{
    public class ProfessionalSearchResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Specialties { get; set; } = new();
        public string Bio { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal HourlyRate { get; set; }
        public double RatingAverage { get; set; }
        public int TotalReviews { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool HasVirtualConsultation { get; set; }
        public List<string> Services { get; set; } = new();
    }
} 