using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace ProConnect.Controllers
{
    [ApiController]
    [Route("api/professionals/portfolio")]
    [Authorize(Roles = "Professional")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        /// <summary>
        /// Sube un archivo al portafolio profesional.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string description)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });
            try
            {
                var result = await _portfolioService.UploadPortfolioFileAsync(userId, file, description);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lista los archivos del portafolio del usuario autenticado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });
            var files = await _portfolioService.GetPortfolioFilesAsync(userId);
            return Ok(files);
        }

        /// <summary>
        /// Elimina un archivo del portafolio.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });
            var result = await _portfolioService.DeletePortfolioFileAsync(userId, id);
            if (!result)
                return NotFound(new { message = "Archivo no encontrado o no autorizado" });
            return NoContent();
        }

        /// <summary>
        /// Actualiza la descripción de un archivo del portafolio.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDescription(string id, [FromBody] string description)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });
            var result = await _portfolioService.UpdatePortfolioFileDescriptionAsync(userId, id, description);
            if (!result)
                return NotFound(new { message = "Archivo no encontrado o no autorizado" });
            return NoContent();
        }
    }
} 