using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace ProConnect.Pages.Booking
{
    /// <summary>
    /// Página de confirmación de reserva
    /// </summary>
    [Authorize]
    public class BookingConfirmationModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IProfessionalProfileService _professionalService;
        private readonly ILogger<BookingConfirmationModel> _logger;

        public BookingConfirmationModel(
            IBookingService bookingService,
            IProfessionalProfileService professionalService,
            ILogger<BookingConfirmationModel> logger)
        {
            _bookingService = bookingService;
            _professionalService = professionalService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string BookingId { get; set; } = string.Empty;

        public BookingDto? Booking { get; set; }
        public ProfessionalProfileResponseDto? Professional { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Maneja las peticiones GET para mostrar la confirmación de reserva
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Validar que se proporcione el ID de la reserva
                if (string.IsNullOrEmpty(BookingId))
                {
                    _logger.LogWarning("Intento de acceso a confirmación sin ID de reserva");
                    ErrorMessage = "ID de reserva requerido";
                    return Page();
                }

                // Obtener el ID del usuario actual
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuario no autenticado intentando ver confirmación");
                    return Challenge();
                }

                // Obtener información de la reserva
                Booking = await _bookingService.GetBookingByIdAsync(BookingId, userId);
                if (Booking == null)
                {
                    _logger.LogWarning("Reserva no encontrada: {BookingId} para usuario: {UserId}", BookingId, userId);
                    ErrorMessage = "La reserva solicitada no fue encontrada o no tienes permisos para verla.";
                    return Page();
                }

                // Verificar que el usuario sea el propietario de la reserva
                if (Booking.ClientId != userId)
                {
                    _logger.LogWarning("Usuario {UserId} intentando acceder a reserva {BookingId} que no le pertenece", userId, BookingId);
                    ErrorMessage = "No tienes permisos para ver esta reserva.";
                    return Page();
                }

                // Obtener información del profesional
                Professional = await _professionalService.GetPublicProfileAsync(Booking.ProfessionalId);
                if (Professional == null)
                {
                    _logger.LogWarning("Profesional no encontrado para reserva: {BookingId}", BookingId);
                    // No es un error crítico, la página puede mostrarse sin info del profesional
                }

                _logger.LogInformation("Confirmación de reserva cargada: {BookingId} para usuario: {UserId}", BookingId, userId);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar confirmación de reserva: {BookingId}", BookingId);
                ErrorMessage = "Ocurrió un error al cargar la información de la reserva. Por favor, intenta nuevamente.";
                return Page();
            }
        }

        /// <summary>
        /// Obtiene la URL para agregar el evento a Google Calendar
        /// </summary>
        public string GetGoogleCalendarUrl()
        {
            if (Booking == null) return string.Empty;

            var startTime = Booking.AppointmentDate.ToString("yyyyMMddTHHmmss");
            var endTime = Booking.AppointmentDate.AddMinutes(Booking.AppointmentDuration).ToString("yyyyMMddTHHmmss");
            
            var title = HttpUtility.UrlEncode($"Cita con {Professional?.Name ?? "Profesional"}");
            var details = HttpUtility.UrlEncode($"Tipo: {GetConsultationTypeDisplay()}\nNotas: {Booking.SpecialNotes ?? "Sin notas"}");
            var location = HttpUtility.UrlEncode(Booking.MeetingDetails?.PhysicalAddress ?? "Consulta virtual");

            return $"https://calendar.google.com/calendar/render?action=TEMPLATE&text={title}&dates={startTime}/{endTime}&details={details}&location={location}";
        }

        /// <summary>
        /// Obtiene la URL para agregar el evento a Outlook Calendar
        /// </summary>
        public string GetOutlookCalendarUrl()
        {
            if (Booking == null) return string.Empty;

            var startTime = Booking.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss");
            var endTime = Booking.AppointmentDate.AddMinutes(Booking.AppointmentDuration).ToString("yyyy-MM-ddTHH:mm:ss");
            
            var title = HttpUtility.UrlEncode($"Cita con {Professional?.Name ?? "Profesional"}");
            var body = HttpUtility.UrlEncode($"Tipo: {GetConsultationTypeDisplay()}\nNotas: {Booking.SpecialNotes ?? "Sin notas"}");
            var location = HttpUtility.UrlEncode(Booking.MeetingDetails?.PhysicalAddress ?? "Consulta virtual");

            return $"https://outlook.live.com/calendar/0/deeplink/compose?subject={title}&startdt={startTime}&enddt={endTime}&body={body}&location={location}";
        }

        /// <summary>
        /// Genera el contenido del archivo iCal
        /// </summary>
        public string GetICalUrl()
        {
            if (Booking == null) return string.Empty;

            var startTime = Booking.AppointmentDate.ToString("yyyyMMddTHHmmssZ");
            var endTime = Booking.AppointmentDate.AddMinutes(Booking.AppointmentDuration).ToString("yyyyMMddTHHmmssZ");
            var created = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

            var icalContent = new StringBuilder();
            icalContent.AppendLine("BEGIN:VCALENDAR");
            icalContent.AppendLine("VERSION:2.0");
            icalContent.AppendLine("PRODID:-//ProConnect//Booking System//ES");
            icalContent.AppendLine("BEGIN:VEVENT");
            icalContent.AppendLine($"UID:{Booking.Id}@proconnect.com");
            icalContent.AppendLine($"DTSTAMP:{created}");
            icalContent.AppendLine($"DTSTART:{startTime}");
            icalContent.AppendLine($"DTEND:{endTime}");
            icalContent.AppendLine($"SUMMARY:Cita con {Professional?.Name ?? "Profesional"}");
            icalContent.AppendLine($"DESCRIPTION:Tipo: {GetConsultationTypeDisplay()}\\nNotas: {Booking.SpecialNotes ?? "Sin notas"}");
            
            if (!string.IsNullOrEmpty(Booking.MeetingDetails?.PhysicalAddress))
            {
                icalContent.AppendLine($"LOCATION:{Booking.MeetingDetails.PhysicalAddress}");
            }
            
            icalContent.AppendLine("STATUS:CONFIRMED");
            icalContent.AppendLine("BEGIN:VALARM");
            icalContent.AppendLine("TRIGGER:-PT15M");
            icalContent.AppendLine("ACTION:DISPLAY");
            icalContent.AppendLine("DESCRIPTION:Recordatorio de cita");
            icalContent.AppendLine("END:VALARM");
            icalContent.AppendLine("END:VEVENT");
            icalContent.AppendLine("END:VCALENDAR");

            var bytes = Encoding.UTF8.GetBytes(icalContent.ToString());
            return $"data:text/calendar;charset=utf8;base64,{Convert.ToBase64String(bytes)}";
        }

        /// <summary>
        /// Obtiene el texto descriptivo del tipo de consulta
        /// </summary>
        public string GetConsultationTypeDisplay()
        {
            if (Booking == null) return string.Empty;

            return Booking.ConsultationType switch
            {
                "consultation" => "Consulta General",
                "follow-up" => "Seguimiento",
                "emergency" => "Urgencia",
                _ => Booking.ConsultationType
            };
        }

        /// <summary>
        /// Obtiene el texto descriptivo del estado de la reserva
        /// </summary>
        public string GetStatusDisplay()
        {
            if (Booking == null) return string.Empty;

            return Booking.Status switch
            {
                "Pending" => "Pendiente",
                "Confirmed" => "Confirmada",
                "InProgress" => "En Progreso",
                "Completed" => "Completada",
                "Cancelled" => "Cancelada",
                "NoShow" => "No Asistió",
                _ => Booking.Status
            };
        }

        /// <summary>
        /// Verifica si la reserva puede ser cancelada
        /// </summary>
        public bool CanCancelBooking()
        {
            if (Booking == null) return false;

            // Solo se puede cancelar si está pendiente o confirmada
            if (Booking.Status != "Pending" && Booking.Status != "Confirmed")
                return false;

            // Solo se puede cancelar hasta 24 horas antes
            var hoursUntilAppointment = (Booking.AppointmentDate - DateTime.UtcNow).TotalHours;
            return hoursUntilAppointment > 24;
        }

        /// <summary>
        /// Verifica si la reserva puede ser reprogramada
        /// </summary>
        public bool CanRescheduleBooking()
        {
            if (Booking == null) return false;

            // Solo se puede reprogramar si está pendiente o confirmada
            if (Booking.Status != "Pending" && Booking.Status != "Confirmed")
                return false;

            // Solo se puede reprogramar hasta 24 horas antes
            var hoursUntilAppointment = (Booking.AppointmentDate - DateTime.UtcNow).TotalHours;
            return hoursUntilAppointment > 24;
        }

        /// <summary>
        /// Obtiene el color CSS para el estado de la reserva
        /// </summary>
        public string GetStatusColor()
        {
            if (Booking == null) return "secondary";

            return Booking.Status switch
            {
                "Pending" => "warning",
                "Confirmed" => "success",
                "InProgress" => "info",
                "Completed" => "primary",
                "Cancelled" => "danger",
                "NoShow" => "dark",
                _ => "secondary"
            };
        }
    }
}
