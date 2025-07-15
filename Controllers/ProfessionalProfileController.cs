using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
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
    [Route("api/[controller]")]
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
        [HttpPost]
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
                return CreatedAtAction(nameof(GetMyProfile), new { id = profile.Id }, profile);
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
        [HttpGet("my-profile")]
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
        [HttpPut("my-profile")]
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
        /// Obtiene un perfil profesional público por ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(string id)
        {
            try
            {
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
    }
} 