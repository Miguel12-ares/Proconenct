using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;

namespace Proconenct.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="registerDto">Datos de registro del usuario</param>
        /// <returns>Respuesta de autenticación con token JWT</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                _logger.LogInformation("Intento de registro para email: {Email}", registerDto.Email);

                var result = await _authService.RegisterAsync(registerDto);

                if (result.Success)
                {
                    _logger.LogInformation("Usuario registrado exitosamente: {UserId}", result.UserId);
                    return Ok(result);
                }

                _logger.LogWarning("Fallo en registro para email: {Email}. Errores: {Errors}",
                    registerDto.Email, string.Join(", ", result.Errors));
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en registro para email: {Email}", registerDto.Email);
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Error interno del servidor" }
                });
            }
        }

        /// <summary>
        /// Autentica un usuario existente
        /// </summary>
        /// <param name="loginDto">Credenciales de login</param>
        /// <returns>Respuesta de autenticación con token JWT</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto loginDto)
        {
            try
            {
                _logger.LogInformation("Intento de login para email: {Email}", loginDto.Email);

                var result = await _authService.LoginAsync(loginDto);

                if (result.Success)
                {
                    _logger.LogInformation("Login exitoso para usuario: {UserId}", result.UserId);
                    return Ok(result);
                }

                _logger.LogWarning("Fallo en login para email: {Email}. Errores: {Errors}",
                    loginDto.Email, string.Join(", ", result.Errors));
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en login para email: {Email}", loginDto.Email);
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Error interno del servidor" }
                });
            }
        }

        /// <summary>
        /// Valida si un token JWT es válido
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>Resultado de validación</returns>
        [HttpPost("validate-token")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);

                if (isValid)
                {
                    return Ok(new { valid = true, message = "Token válido" });
                }

                return Unauthorized(new { valid = false, message = "Token inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token");
                return Unauthorized(new { valid = false, message = "Token inválido" });
            }
        }

        /// <summary>
        /// Cierra sesión del usuario actual
        /// </summary>
        /// <returns>Resultado del logout</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _authService.LogoutAsync(userId);
                if (result)
                {
                    _logger.LogInformation("Logout exitoso para usuario: {UserId}", userId);
                    return Ok(new { message = "Logout exitoso" });
                }

                return BadRequest(new { message = "Error al cerrar sesión" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en logout");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Endpoint solo para administradores (ejemplo de protección por rol)
        /// </summary>
        [HttpGet("admin-only")]
        [AuthorizeRoles(ProConnect.Core.Entities.UserType.Administrator)]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Acceso permitido solo para administradores." });
        }

        /// <summary>
        /// Envía o reenvía el email de verificación
        /// </summary>
        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification([FromBody] string email)
        {
            var result = await _authService.SendEmailVerificationAsync(email);
            if (result)
                return Ok(new { message = "Correo de verificación enviado" });
            return BadRequest(new { message = "No se pudo enviar el correo de verificación" });
        }

        /// <summary>
        /// Verifica el email usando el token
        /// </summary>
        [HttpGet("verify-email/{token}")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            // Siempre redirigir al login, sin mostrar detalles de error
            return Redirect("/auth/Login?verified=" + (result ? "1" : "0"));
        }
    }
}
