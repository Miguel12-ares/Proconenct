using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace ProConnect.Controllers
{
    /// <summary>
    /// Controlador para gestionar reservas de citas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el ID del usuario autenticado desde el token JWT
        /// </summary>
        private string GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return userIdClaim;
        }

        /// <summary>
        /// Crea una nueva reserva
        /// </summary>
        /// <param name="createDto">Datos para crear la reserva</param>
        /// <returns>Reserva creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BookingDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto createDto)
        {
            try
            {
                var clientId = GetCurrentUserId();
                createDto.ClientId = clientId;
                var booking = await _bookingService.CreateBookingAsync(createDto, clientId);
                
                _logger.LogInformation("Booking created successfully. ID: {BookingId}, Client: {ClientId}", 
                    booking.Id, clientId);
                
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when creating booking: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access when creating booking: {Message}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una reserva específica por ID
        /// </summary>
        /// <param name="id">ID de la reserva</param>
        /// <returns>Detalles de la reserva</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookingDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BookingDto>> GetBooking(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.GetBookingByIdAsync(id, userId);
                
                if (booking == null)
                {
                    return NotFound(new { error = "Reserva no encontrada" });
                }
                
                return Ok(booking);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access to booking {BookingId}: {Message}", id, ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Lista reservas del usuario autenticado con filtros y paginación
        /// </summary>
        /// <param name="status">Filtrar por estado de reserva</param>
        /// <param name="dateFrom">Fecha desde (formato: yyyy-MM-dd)</param>
        /// <param name="dateTo">Fecha hasta (formato: yyyy-MM-dd)</param>
        /// <param name="professionalId">Filtrar por profesional específico</param>
        /// <param name="limit">Límite de resultados (default: 20, máximo: 100)</param>
        /// <param name="offset">Desplazamiento para paginación (default: 0)</param>
        /// <returns>Lista paginada de reservas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<BookingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PagedResultDto<BookingDto>>> GetBookings(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] string? professionalId = null,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                // Validar parámetros
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new { error = "El límite debe estar entre 1 y 100" });
                }

                if (offset < 0)
                {
                    return BadRequest(new { error = "El offset no puede ser negativo" });
                }

                var userId = GetCurrentUserId();
                var bookings = await _bookingService.GetBookingsWithFiltersAsync(
                    userId, status, dateFrom, dateTo, professionalId, limit, offset);

                var totalCount = await _bookingService.GetBookingsCountWithFiltersAsync(
                    userId, status, dateFrom, dateTo, professionalId);

                var result = new PagedResultDto<BookingDto>
                {
                    Items = bookings,
                    TotalCount = totalCount,
                    PageSize = limit,
                    CurrentPage = (offset / limit) + 1,
                    TotalPages = (int)Math.Ceiling((double)totalCount / limit)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las reservas del usuario autenticado (cliente)
        /// </summary>
        /// <param name="limit">Límite de resultados (default: 20)</param>
        /// <param name="offset">Desplazamiento para paginación (default: 0)</param>
        /// <returns>Lista de reservas del cliente</returns>
        [HttpGet("my-bookings")]
        [ProducesResponseType(typeof(List<BookingDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<BookingDto>>> GetMyBookings([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            try
            {
                var clientId = GetCurrentUserId();
                var bookings = await _bookingService.GetBookingsByClientAsync(clientId, limit, offset);
                
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las reservas de un profesional específico
        /// </summary>
        /// <param name="professionalId">ID del profesional</param>
        /// <param name="limit">Límite de resultados (default: 20)</param>
        /// <param name="offset">Desplazamiento para paginación (default: 0)</param>
        /// <returns>Lista de reservas del profesional</returns>
        [HttpGet("professional/{professionalId}")]
        [ProducesResponseType(typeof(List<BookingDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<BookingDto>>> GetProfessionalBookings(
            string professionalId, 
            [FromQuery] int limit = 20, 
            [FromQuery] int offset = 0)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Verificar que el usuario autenticado es el profesional o tiene permisos
                if (currentUserId != professionalId)
                {
                    return Forbid();
                }
                
                var bookings = await _bookingService.GetBookingsByProfessionalAsync(professionalId, limit, offset);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for professional {ProfessionalId}", professionalId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza una reserva existente
        /// </summary>
        /// <param name="id">ID de la reserva</param>
        /// <param name="updateDto">Datos para actualizar la reserva</param>
        /// <returns>Reserva actualizada</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BookingDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BookingDto>> UpdateBooking(string id, [FromBody] UpdateBookingDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.UpdateBookingAsync(id, updateDto, userId);
                
                _logger.LogInformation("Booking updated successfully. ID: {BookingId}, UpdatedBy: {UserId}", 
                    id, userId);
                
                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when updating booking {BookingId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access when updating booking {BookingId}: {Message}", id, ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cancela una reserva
        /// </summary>
        /// <param name="id">ID de la reserva</param>
        /// <param name="reason">Razón de la cancelación (opcional)</param>
        /// <returns>Resultado de la cancelación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> CancelBooking(string id, [FromQuery] string? reason = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _bookingService.CancelBookingAsync(id, userId, reason);
                
                if (result)
                {
                    _logger.LogInformation("Booking cancelled successfully. ID: {BookingId}, CancelledBy: {UserId}", 
                        id, userId);
                    return NoContent();
                }
                
                return NotFound(new { error = "Reserva no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when cancelling booking {BookingId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access when cancelling booking {BookingId}: {Message}", id, ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Confirma una reserva (solo para profesionales)
        /// </summary>
        /// <param name="id">ID de la reserva</param>
        /// <returns>Resultado de la confirmación</returns>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ConfirmBooking(string id)
        {
            try
            {
                var professionalId = GetCurrentUserId();
                var result = await _bookingService.ConfirmBookingAsync(id, professionalId);
                
                if (result)
                {
                    _logger.LogInformation("Booking confirmed successfully. ID: {BookingId}, ConfirmedBy: {ProfessionalId}", 
                        id, professionalId);
                    return Ok(new { message = "Reserva confirmada exitosamente" });
                }
                
                return NotFound(new { error = "Reserva no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when confirming booking {BookingId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access when confirming booking {BookingId}: {Message}", id, ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Marca una reserva como completada
        /// </summary>
        /// <param name="id">ID de la reserva</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/complete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> CompleteBooking(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _bookingService.CompleteBookingAsync(id, userId);
                
                if (result)
                {
                    _logger.LogInformation("Booking completed successfully. ID: {BookingId}, CompletedBy: {UserId}", 
                        id, userId);
                    return Ok(new { message = "Reserva marcada como completada" });
                }
                
                return NotFound(new { error = "Reserva no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when completing booking {BookingId}: {Message}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access when completing booking {BookingId}: {Message}", id, ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing booking {BookingId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica si hay conflictos de horario para un profesional
        /// </summary>
        /// <param name="professionalId">ID del profesional</param>
        /// <param name="appointmentDate">Fecha y hora de la cita</param>
        /// <param name="duration">Duración en minutos (default: 60)</param>
        /// <param name="excludeBookingId">ID de reserva a excluir (para actualizaciones)</param>
        /// <returns>Información sobre conflictos</returns>
        [HttpGet("check-conflict")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> CheckConflict(
            [FromQuery] string professionalId,
            [FromQuery] DateTime appointmentDate,
            [FromQuery] int duration = 60,
            [FromQuery] string? excludeBookingId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(professionalId))
                {
                    return BadRequest(new { error = "El ID del profesional es requerido" });
                }

                if (appointmentDate <= DateTime.UtcNow)
                {
                    return BadRequest(new { error = "La fecha de la cita no puede estar en el pasado" });
                }

                if (duration < 15 || duration > 480)
                {
                    return BadRequest(new { error = "La duración debe estar entre 15 y 480 minutos" });
                }

                var hasConflict = await _bookingService.HasConflictAsync(professionalId, appointmentDate, duration, excludeBookingId);
                
                return Ok(new { 
                    hasConflict = hasConflict,
                    professionalId = professionalId,
                    appointmentDate = appointmentDate,
                    duration = duration,
                    message = hasConflict ? "El horario seleccionado no está disponible" : "El horario está disponible"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking booking conflicts");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las próximas reservas de un profesional
        /// </summary>
        /// <param name="professionalId">ID del profesional</param>
        /// <param name="fromDate">Fecha desde la cual obtener reservas (default: hoy)</param>
        /// <param name="limit">Límite de resultados (default: 10)</param>
        /// <returns>Lista de próximas reservas</returns>
        [HttpGet("professional/{professionalId}/upcoming")]
        [ProducesResponseType(typeof(List<BookingDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<BookingDto>>> GetUpcomingBookings(
            string professionalId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Verificar que el usuario autenticado es el profesional
                if (currentUserId != professionalId)
                {
                    return Forbid();
                }
                
                var startDate = fromDate ?? DateTime.UtcNow;
                var bookings = await _bookingService.GetUpcomingBookingsAsync(professionalId, startDate, limit);
                
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings for professional {ProfessionalId}", professionalId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de reservas por estado
        /// </summary>
        /// <param name="status">Estado de las reservas a contar</param>
        /// <returns>Estadísticas de reservas</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> GetBookingStats([FromQuery] string status)
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _bookingService.GetBookingCountByStatusAsync(userId, status);
                
                return Ok(new { 
                    status = status,
                    count = count,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking stats for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
} 