using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using System.Security.Claims;

namespace ProConnect.Pages.Booking
{
    /// <summary>
    /// Página principal para realizar reservas con profesionales
    /// </summary>
    [Authorize]
    public class BookingIndexModel : PageModel
    {
        private readonly IProfessionalProfileService _professionalService;
        private readonly IUserService _userService;
        private readonly ILogger<BookingIndexModel> _logger;

        public BookingIndexModel(
            IProfessionalProfileService professionalService,
            IUserService userService,
            ILogger<BookingIndexModel> logger)
        {
            _professionalService = professionalService;
            _userService = userService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string ProfessionalId { get; set; } = string.Empty;

        public ProfessionalProfileResponseDto? Professional { get; set; }
        public UserProfileDto? Client { get; set; }
        public string TimeZone { get; set; } = "America/Bogota";
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Maneja las peticiones GET para mostrar la página de reservas
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Validar que se proporcione el ID del profesional
                if (string.IsNullOrEmpty(ProfessionalId))
                {
                    _logger.LogWarning("Intento de acceso a booking sin ID de profesional");
                    return NotFound("ID de profesional requerido");
                }

                // Obtener información del profesional
                Professional = await _professionalService.GetPublicProfileAsync(ProfessionalId);
                if (Professional == null)
                {
                    _logger.LogWarning("Profesional no encontrado: {ProfessionalId}", ProfessionalId);
                    ErrorMessage = "El profesional solicitado no fue encontrado o no está disponible.";
                    return Page();
                }

                // Verificar que el profesional esté activo y disponible para reservas
                if (Professional.Status != ProfileStatusDto.Active || !Professional.IsCompleteForPublicView)
                {
                    _logger.LogWarning("Intento de reserva con profesional inactivo: {ProfessionalId}", ProfessionalId);
                    ErrorMessage = "El profesional no está disponible para reservas en este momento.";
                    return Page();
                }

                // Obtener información del cliente actual
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    Client = await _userService.GetUserProfileAsync(userId);
                }

                // Configurar zona horaria (se puede obtener del perfil del profesional o configuración)
                TimeZone = Professional.TimeZone ?? "America/Bogota";

                _logger.LogInformation("Página de booking cargada para profesional: {ProfessionalId}, cliente: {ClientId}", 
                    ProfessionalId, userId);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la página de booking para profesional: {ProfessionalId}", ProfessionalId);
                ErrorMessage = "Ocurrió un error al cargar la información. Por favor, intenta nuevamente.";
                return Page();
            }
        }

        /// <summary>
        /// Maneja las peticiones POST para procesar reservas (si se implementa aquí en lugar de API)
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Esta funcionalidad se maneja principalmente a través de la API
                // Pero se puede implementar aquí si se prefiere el enfoque tradicional de Razor Pages
                
                _logger.LogInformation("POST request recibido en booking page para profesional: {ProfessionalId}", ProfessionalId);
                
                // Redirigir a la API o manejar la lógica aquí
                return RedirectToPage("/booking/confirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar reserva para profesional: {ProfessionalId}", ProfessionalId);
                ErrorMessage = "Error al procesar la reserva. Por favor, intenta nuevamente.";
                return Page();
            }
        }

        /// <summary>
        /// Valida que el usuario tenga permisos para realizar reservas
        /// </summary>
        private bool ValidateUserPermissions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Usuario no autenticado intentando acceder a booking");
                return false;
            }

            // Verificar que el usuario no sea el mismo profesional
            if (userId == ProfessionalId)
            {
                _logger.LogWarning("Profesional intentando reservar consigo mismo: {ProfessionalId}", ProfessionalId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtiene la información de zona horaria formateada
        /// </summary>
        public string GetFormattedTimeZone()
        {
            try
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                return timeZoneInfo.DisplayName;
            }
            catch
            {
                return TimeZone;
            }
        }

        /// <summary>
        /// Verifica si el profesional está disponible para reservas
        /// </summary>
        public bool IsProfessionalAvailable()
        {
            return Professional != null && 
                   Professional.Status == ProfileStatusDto.Active && 
                   Professional.IsAvailableForBooking;
        }
    }
}
