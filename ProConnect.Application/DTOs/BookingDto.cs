using ProConnect.Core.Entities;

namespace ProConnect.Application.DTOs
{
    /// <summary>
    /// DTO para representar una reserva completa
    /// </summary>
    public class BookingDto
    {
        public string Id { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ProfessionalId { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public int AppointmentDuration { get; set; }
        public string ConsultationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? SpecialNotes { get; set; }
        public MeetingDetailsDto? MeetingDetails { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public string? CancelledBy { get; set; }

        // Información adicional del cliente y profesional
        public string? ClientName { get; set; }
        public string? ProfessionalName { get; set; }
        public string? ProfessionalSpecialty { get; set; }
    }

    /// <summary>
    /// DTO para los detalles de la reunión
    /// </summary>
    public class MeetingDetailsDto
    {
        public string? VirtualMeetingUrl { get; set; }
        public string? VirtualMeetingId { get; set; }
        public string? VirtualMeetingPassword { get; set; }
        public string? PhysicalAddress { get; set; }
        public string? Directions { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCode { get; set; }
    }
} 