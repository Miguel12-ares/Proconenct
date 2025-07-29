using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProConnect.Core.Entities
{
    /// <summary>
    /// Representa una reserva de cita entre un cliente y un profesional.
    /// </summary>
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Referencia al cliente que realiza la reserva (User.Id)
        /// </summary>
        [BsonElement("client_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Referencia al profesional (ProfessionalProfile.Id)
        /// </summary>
        [BsonElement("professional_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string ProfessionalId { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora exacta de la cita
        /// </summary>
        [BsonElement("appointment_date")]
        [Required]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Duración de la cita en minutos (default: 60)
        /// </summary>
        [BsonElement("appointment_duration")]
        [Range(15, 480, ErrorMessage = "La duración debe estar entre 15 y 480 minutos")]
        public int AppointmentDuration { get; set; } = 60;

        /// <summary>
        /// Tipo de consulta
        /// </summary>
        [BsonElement("consultation_type")]
        [Required]
        public ConsultationType ConsultationType { get; set; } = ConsultationType.Virtual;

        /// <summary>
        /// Estado de la reserva
        /// </summary>
        [BsonElement("status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        /// <summary>
        /// Costo total de la consulta
        /// </summary>
        [BsonElement("total_amount")]
        [Range(0, 10000, ErrorMessage = "El monto total debe estar entre $0 y $10,000")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Observaciones especiales del cliente (opcional)
        /// </summary>
        [BsonElement("special_notes")]
        [MaxLength(1000, ErrorMessage = "Las notas especiales no pueden exceder 1000 caracteres")]
        public string? SpecialNotes { get; set; }

        /// <summary>
        /// Información de la reunión (enlace virtual, dirección, etc.)
        /// </summary>
        [BsonElement("meeting_details")]
        public MeetingDetails? MeetingDetails { get; set; }

        /// <summary>
        /// Fecha de creación de la reserva
        /// </summary>
        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de cancelación (si aplica)
        /// </summary>
        [BsonElement("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Razón de cancelación (si aplica)
        /// </summary>
        [BsonElement("cancellation_reason")]
        [MaxLength(500, ErrorMessage = "La razón de cancelación no puede exceder 500 caracteres")]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// ID del usuario que canceló (cliente o profesional) o "Sistema" para cancelaciones automáticas
        /// </summary>
        [BsonElement("cancelled_by")]
        public string? CancelledBy { get; set; }

        /// <summary>
        /// Valida si la reserva puede ser cancelada (mínimo 2 horas antes)
        /// </summary>
        public bool CanBeCancelled()
        {
            return Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;
        }

        /// <summary>
        /// Valida si la fecha de la cita no está en el pasado
        /// </summary>
        public bool IsValidAppointmentDate()
        {
            return AppointmentDate > DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza la fecha de modificación
        /// </summary>
        public void UpdateModificationDate()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancela la reserva
        /// </summary>
        public void Cancel(string cancelledBy, string? reason = null)
        {
            Status = BookingStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancelledBy = cancelledBy;
            CancellationReason = reason;
            UpdateModificationDate();
        }

        /// <summary>
        /// Confirma la reserva
        /// </summary>
        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            UpdateModificationDate();
        }

        /// <summary>
        /// Marca la reserva como completada
        /// </summary>
        public void Complete()
        {
            Status = BookingStatus.Completed;
            UpdateModificationDate();
        }

        /// <summary>
        /// Reprograma la reserva
        /// </summary>
        public void Reschedule(DateTime newAppointmentDate)
        {
            Status = BookingStatus.Rescheduled;
            AppointmentDate = newAppointmentDate;
            UpdateModificationDate();
        }
    }

    /// <summary>
    /// Tipos de consulta disponibles
    /// </summary>
    public enum ConsultationType
    {
        Presencial = 0,
        Virtual = 1,
        Telefonica = 2
    }

    /// <summary>
    /// Estados de una reserva
    /// </summary>
    public enum BookingStatus
    {
        Pending = 0,      // Pendiente de confirmación
        Confirmed = 1,    // Confirmada
        Completed = 2,    // Completada
        Cancelled = 3,    // Cancelada
        Rescheduled = 4   // Reprogramada
    }

    /// <summary>
    /// Detalles de la reunión según el tipo de consulta
    /// </summary>
    public class MeetingDetails
    {
        /// <summary>
        /// Enlace de la reunión virtual (para consultas virtuales)
        /// </summary>
        [BsonElement("virtual_meeting_url")]
        [MaxLength(500, ErrorMessage = "La URL de la reunión no puede exceder 500 caracteres")]
        public string? VirtualMeetingUrl { get; set; }

        /// <summary>
        /// ID de la reunión virtual
        /// </summary>
        [BsonElement("virtual_meeting_id")]
        [MaxLength(100, ErrorMessage = "El ID de la reunión no puede exceder 100 caracteres")]
        public string? VirtualMeetingId { get; set; }

        /// <summary>
        /// Contraseña de la reunión virtual
        /// </summary>
        [BsonElement("virtual_meeting_password")]
        [MaxLength(50, ErrorMessage = "La contraseña no puede exceder 50 caracteres")]
        public string? VirtualMeetingPassword { get; set; }

        /// <summary>
        /// Dirección física (para consultas presenciales)
        /// </summary>
        [BsonElement("physical_address")]
        [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string? PhysicalAddress { get; set; }

        /// <summary>
        /// Instrucciones adicionales para llegar
        /// </summary>
        [BsonElement("directions")]
        [MaxLength(1000, ErrorMessage = "Las instrucciones no pueden exceder 1000 caracteres")]
        public string? Directions { get; set; }

        /// <summary>
        /// Número de teléfono para consultas telefónicas
        /// </summary>
        [BsonElement("phone_number")]
        [MaxLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Código de país para llamadas internacionales
        /// </summary>
        [BsonElement("country_code")]
        [MaxLength(5, ErrorMessage = "El código de país no puede exceder 5 caracteres")]
        public string? CountryCode { get; set; } = "+1";
    }
} 