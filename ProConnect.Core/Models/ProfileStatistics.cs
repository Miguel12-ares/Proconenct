namespace ProConnect.Core.Models
{
    /// <summary>
    /// Estadísticas de perfiles profesionales.
    /// </summary>
    public class ProfileStatistics
    {
        public int TotalProfiles { get; set; }
        public int ActiveProfiles { get; set; }
        public int DraftProfiles { get; set; }
        public double AverageRating { get; set; }
        public decimal AverageHourlyRate { get; set; }
    }
} 