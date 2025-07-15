using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System.Threading.Tasks;

namespace Proconenct.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly IProfessionalSearchService _searchService;

        public SearchController(IProfessionalSearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Búsqueda avanzada de profesionales con filtros combinables y paginación.
        /// </summary>
        /// <param name="query">Texto libre (nombre, bio, especialidad, ubicación)</param>
        /// <param name="specialties">Lista de especialidades</param>
        /// <param name="location">Ubicación</param>
        /// <param name="minHourlyRate">Tarifa mínima</param>
        /// <param name="maxHourlyRate">Tarifa máxima</param>
        /// <param name="minRating">Calificación mínima</param>
        /// <param name="minExperienceYears">Años de experiencia mínimos</param>
        /// <param name="virtualConsultation">¿Ofrece consulta virtual?</param>
        /// <param name="orderBy">Ordenar por: relevance, price_asc, price_desc, rating, experience</param>
        /// <param name="page">Página</param>
        /// <param name="pageSize">Tamaño de página</param>
        [HttpGet("professionals")]
        public async Task<IActionResult> SearchProfessionals([
            FromQuery] string? query,
            [FromQuery] List<string>? specialties,
            [FromQuery] string? location,
            [FromQuery] decimal? minHourlyRate,
            [FromQuery] decimal? maxHourlyRate,
            [FromQuery] double? minRating,
            [FromQuery] int? minExperienceYears,
            [FromQuery] bool? virtualConsultation,
            [FromQuery] string? orderBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filters = new ProfessionalSearchFiltersDto
            {
                Query = query,
                Specialties = specialties,
                Location = location,
                MinHourlyRate = minHourlyRate,
                MaxHourlyRate = maxHourlyRate,
                MinRating = minRating,
                MinExperienceYears = minExperienceYears,
                VirtualConsultation = virtualConsultation,
                OrderBy = orderBy,
                Page = page,
                PageSize = pageSize
            };

            var result = await _searchService.SearchProfessionalsAsync(filters);
            return Ok(result);
        }
    }
} 