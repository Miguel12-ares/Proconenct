using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using ProConnect.Application.Validators;
using FluentValidation;
using System.Security.Claims;

namespace ProConnect.Controllers
{
    /// <summary>
    /// Controlador para la gestión de perfiles profesionales.
    /// </summary>
    [ApiController]
    [Route("api/professionals")]
    public class ProfessionalProfileController : ControllerBase
    {
        private readonly IProfessionalProfileService _profileService;
        private readonly IValidator<CreateProfessionalProfileDto> _createValidator;
        private readonly IValidator<UpdateProfessionalProfileDto> _updateValidator;

        public ProfessionalProfileController(
            IProfessionalProfileService profileService,
            IValidator<CreateProfessionalProfileDto> createValidator,
            IValidator<UpdateProfessionalProfileDto> updateValidator)
        {
            _profileService = profileService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        /// <summary>
        /// Crea un nuevo perfil profesional.
        /// </summary>
        /// <param name="createDto">Datos del perfil a crear</param>
        /// <returns>Perfil creado con ID</returns>
        [HttpPost("profile")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfessionalProfileDto createDto)
        {
            try
            {
                // Validar el DTO
                var validationResult = await _createValidator.ValidateAsync(createDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Datos de entrada inválidos",
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
                }

                // Obtener el ID del usuario del token JWT
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                var profile = await _profileService.CreateProfileAsync(createDto, userId);
                return CreatedAtAction(nameof(GetMyProfile), new { }, profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el perfil profesional del usuario autenticado.
        /// </summary>
        /// <returns>Perfil completo del usuario autenticado</returns>
        [HttpGet("profile")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                var profile = await _profileService.GetMyProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new { Message = "No se encontró un perfil profesional para este usuario" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza el perfil profesional del usuario autenticado.
        /// </summary>
        /// <param name="updateDto">Datos actualizados del perfil</param>
        /// <returns>Perfil actualizado</returns>
        [HttpPut("profile")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfessionalProfileDto updateDto)
        {
            try
            {
                // Validar el DTO
                var validationResult = await _updateValidator.ValidateAsync(updateDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Datos de entrada inválidos",
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                // Obtener el perfil actual para obtener el ID
                var currentProfile = await _profileService.GetMyProfileAsync(userId);
                if (currentProfile == null)
                {
                    return NotFound(new { Message = "No se encontró un perfil profesional para actualizar" });
                }

                var updatedProfile = await _profileService.UpdateProfileAsync(currentProfile.Id, updateDto, userId);
                return Ok(updatedProfile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza el horario de disponibilidad del profesional.
        /// </summary>
        [HttpPut("availability/schedule")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> UpdateAvailabilitySchedule([FromBody] AvailabilityScheduleDto scheduleDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });

                var result = await _profileService.UpdateAvailabilityScheduleAsync(userId, scheduleDto);
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Agrega un bloqueo de disponibilidad para el profesional.
        /// </summary>
        [HttpPost("availability/block")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> AddAvailabilityBlock([FromBody] CreateAvailabilityBlockDto blockDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });

                var block = await _profileService.AddAvailabilityBlockAsync(userId, blockDto);
                return Ok(block);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los bloqueos de disponibilidad del profesional.
        /// </summary>
        [HttpGet("availability/blocks")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> GetAvailabilityBlocks()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });

                var blocks = await _profileService.GetAvailabilityBlocksAsync(userId);
                return Ok(blocks);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un bloqueo de disponibilidad por id.
        /// </summary>
        [HttpDelete("availability/block/{id}")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> DeleteAvailabilityBlock(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });

                var result = await _profileService.DeleteAvailabilityBlockAsync(userId, id);
                return Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Consulta los slots disponibles para una fecha específica.
        /// </summary>
        [HttpGet("availability/check")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> CheckAvailability([FromQuery] string date)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });

                if (!DateTime.TryParse(date, out var parsedDate))
                    return BadRequest(new { Message = "Formato de fecha inválido. Use YYYY-MM-DD" });

                var response = await _profileService.CheckAvailabilityAsync(userId, parsedDate);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un perfil profesional público por ID.
        /// </summary>
        /// <param name="id">ID del perfil profesional</param>
        /// <returns>Perfil público (sin datos sensibles)</returns>
        [HttpGet("profile/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { Message = "ID del perfil es requerido" });
                }

                var profile = await _profileService.GetPublicProfileAsync(id);
                if (profile == null)
                {
                    return NotFound(new { Message = "Perfil profesional no encontrado o no está activo" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los perfiles profesionales activos.
        /// </summary>
        /// <returns>Lista de perfiles activos</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllActiveProfiles()
        {
            try
            {
                var profiles = await _profileService.GetAllActiveProfilesAsync();
                return Ok(profiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca perfiles por especialidad.
        /// </summary>
        /// <param name="specialty">Especialidad a buscar</param>
        /// <returns>Lista de perfiles con la especialidad</returns>
        [HttpGet("specialty/{specialty}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfilesBySpecialty(string specialty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(specialty))
                {
                    return BadRequest(new { Message = "La especialidad es requerida" });
                }

                var profiles = await _profileService.GetProfilesBySpecialtyAsync(specialty);
                return Ok(profiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de perfiles profesionales.
        /// </summary>
        /// <returns>Estadísticas de perfiles</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetProfileStatistics()
        {
            try
            {
                // Este endpoint requeriría implementación adicional en el servicio
                // Por ahora retornamos un placeholder
                return Ok(new
                {
                    Message = "Estadísticas de perfiles profesionales",
                    Note = "Implementación pendiente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo servicio para el perfil profesional del usuario autenticado.
        /// </summary>
        [HttpPost("services")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> AddService([FromBody] CreateServiceDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                var result = await _profileService.AddServiceAsync(userId, dto);
                return result ? Ok(new { Success = true }) : BadRequest(new { Message = "No se pudo agregar el servicio" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Lista todos los servicios del perfil profesional del usuario autenticado.
        /// </summary>
        [HttpGet("services")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> GetServices()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                var services = await _profileService.GetServicesAsync(userId);
                return Ok(services);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un servicio existente del perfil profesional del usuario autenticado.
        /// </summary>
        [HttpPut("services/{id}")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> UpdateService(string id, [FromBody] UpdateServiceDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                if (id != dto.Id)
                    return BadRequest(new { Message = "El id de la ruta no coincide con el del cuerpo" });
                var result = await _profileService.UpdateServiceAsync(userId, dto);
                return result ? Ok(new { Success = true }) : BadRequest(new { Message = "No se pudo actualizar el servicio" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un servicio del perfil profesional del usuario autenticado.
        /// </summary>
        [HttpDelete("services/{id}")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> DeleteService(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                var result = await _profileService.DeleteServiceAsync(userId, id);
                return result ? Ok(new { Success = true }) : BadRequest(new { Message = "No se pudo eliminar el servicio" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene la disponibilidad pública de un profesional por id y rango de fechas.
        /// </summary>
        [HttpGet("{id}/availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability(string id, [FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest(new { Message = "ID del profesional es requerido" });

                DateTime start, end;
                if (!DateTime.TryParse(startDate, out start))
                    start = DateTime.UtcNow.Date;
                if (!DateTime.TryParse(endDate, out end))
                    end = start.AddMonths(2).AddDays(-1);

                var availability = await _profileService.GetAvailabilityByProfessionalIdAsync(id, start, end);
                return Ok(availability);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }
} 