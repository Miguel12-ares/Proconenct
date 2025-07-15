using System.Collections.Generic;

namespace ProConnect.Application.DTOs
{
    public class ProfessionalSearchFiltersDto
    {
        public string? Query { get; set; }
        public List<string>? Specialties { get; set; }
        public string? Location { get; set; }
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public double? MinRating { get; set; }
        public int? MinExperienceYears { get; set; }
        public bool? VirtualConsultation { get; set; }
        public string? OrderBy { get; set; } // relevance, price_asc, price_desc, rating, experience
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
} 