using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ProConnect.Controllers
{
    /// <summary>
    /// Controlador para la gestión del perfil de usuario
    /// </summary>
    [ApiController]
    [Route("api/user")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class UserProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IUserService userService, 
            ILogger<UserProfileController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el perfil del usuario actual
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                var userProfile = await _userService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return NotFound(new { Message = "Perfil de usuario no encontrado" });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario");
                return StatusCode(500, new { Message = "Error interno del servidor al obtener el perfil" });
            }
        }

        /// <summary>
        /// Actualiza el perfil del usuario actual
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Datos de entrada inválidos", Errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                var updatedProfile = await _userService.UpdateUserProfileAsync(userId, updateDto);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario");
                return StatusCode(500, new { Message = "Error interno del servidor al actualizar el perfil" });
            }
        }

        /// <summary>
        /// Elimina la cuenta del usuario actual (eliminación permanente)
        /// </summary>
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                // Verificar que el usuario existe
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound(new { Message = "Usuario no encontrado" });
                }

                // Obtener información del usuario para logging
                var userProfile = await _userService.GetUserProfileAsync(userId);
                var userType = userProfile?.UserType.ToString() ?? "Desconocido";

                _logger.LogWarning("Usuario {UserId} ({UserType}) solicitó eliminación permanente de su cuenta", userId, userType);

                // Realizar la eliminación permanente
                var result = await _userService.DeleteUserPermanentlyAsync(userId);
                if (!result)
                {
                    return StatusCode(500, new { Message = "No se pudo eliminar la cuenta del usuario. Por favor, inténtelo de nuevo." });
                }

                _logger.LogInformation("Cuenta de usuario eliminada permanentemente: {UserId} ({UserType})", userId, userType);
                
                // Eliminar todas las cookies de autenticación
                Response.Cookies.Delete("jwtToken");
                Response.Cookies.Delete("refreshToken");
                Response.Cookies.Delete("userSession");
                
                return Ok(new { 
                    Message = "Cuenta eliminada permanentemente. Todos sus datos han sido removidos del sistema.",
                    DeletedAt = DateTime.UtcNow,
                    RedirectTo = "/auth/Login",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la cuenta del usuario");
                return StatusCode(500, new { Message = "Error interno del servidor al eliminar la cuenta" });
            }
        }
    }
}
