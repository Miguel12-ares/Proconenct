using System;

namespace ProConnect.Application.DTOs
{
    public class ReviewDto
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string ProfessionalId { get; set; }
        public string BookingId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public string ClientId { get; set; }
        public string ProfessionalId { get; set; }
        public string BookingId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
