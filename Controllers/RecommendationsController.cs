using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.Interfaces;
using System.Threading.Tasks;

namespace ProConnect.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Obtiene recomendaciones de profesionales relevantes.
        /// </summary>
        /// <param name="userLocation">Ubicacion opcional del usuario para personalizar resultados</param>
        /// <param name="limit">Limite de resultados (default 10)</param>
        /// <returns>Lista de profesionales recomendados</returns>
        [HttpGet("professionals")]
        public async Task<IActionResult> GetRecommendedProfessionals([FromQuery] string? userLocation = null, [FromQuery] int limit = 10)
        {
            var result = await _recommendationService.GetRecommendedProfessionalsAsync(userLocation, limit);
            return Ok(result);
        }
    }
} 