using Microsoft.AspNetCore.Mvc;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Proconenct.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly IProfessionalSearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IProfessionalSearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
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
            try
            {
                // Validar parámetros de entrada
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 1000)
                {
                    return BadRequest("El tamaño de página debe estar entre 1 y 1000");
                }
                if (minRating.HasValue && (minRating < 1 || minRating > 5))
                {
                    return BadRequest("La calificación mínima debe estar entre 1 y 5");
                }
                if (minExperienceYears.HasValue && minExperienceYears < 0)
                {
                    return BadRequest("Los años de experiencia no pueden ser negativos");
                }
                if (minHourlyRate.HasValue && minHourlyRate < 0)
                {
                    return BadRequest("La tarifa mínima no puede ser negativa");
                }
                if (maxHourlyRate.HasValue && maxHourlyRate < 0)
                {
                    return BadRequest("La tarifa máxima no puede ser negativa");
                }
                if (minHourlyRate.HasValue && maxHourlyRate.HasValue && minHourlyRate > maxHourlyRate)
                {
                    return BadRequest("La tarifa mínima no puede ser mayor que la máxima");
                }

                // Validar ordenamiento
                var validOrderBy = new[] { "relevance", "price_asc, price_desc", "rating", "experience" };
                if (!string.IsNullOrEmpty(orderBy) && !validOrderBy.Contains(orderBy.ToLower()))
                {
                    return BadRequest($"Ordenamiento inválido. Valores permitidos: {string.Join(", ", validOrderBy)}");
                }

                var filters = new ProfessionalSearchFiltersDto
                {
                    Query = query?.Trim(),
                    Specialties = specialties?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
                    Location = location?.Trim(),
                    MinHourlyRate = minHourlyRate,
                    MaxHourlyRate = maxHourlyRate,
                    MinRating = minRating,
                    MinExperienceYears = minExperienceYears,
                    VirtualConsultation = virtualConsultation,
                    OrderBy = orderBy,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Búsqueda de profesionales iniciada con filtros: {@Filters}", filters);

                var result = await _searchService.SearchProfessionalsAsync(filters);

                _logger.LogInformation("Búsqueda completada. Resultados encontrados: {Count}", result.TotalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la búsqueda de profesionales");
                return StatusCode(500, new { error = "Error interno del servidor durante la búsqueda" });
            }
        }

        /// <summary>
        /// Genera datos de prueba para el sistema de búsqueda.
        /// </summary>
        [HttpPost("generate-test-data")]
        public async Task<IActionResult> GenerateTestData()
        {
            try
            {
                // Crear algunos perfiles profesionales de prueba
                var testProfiles = new List<object>
                {
                    new {
                        UserId = "507f1f77cf86cd799439011",
                        Specialties = new List<string> { "Abogado, Derecho Civil" },
                        Bio = "Abogado especializado en derecho civil con más de 10 años de experiencia. Ofrezco servicios de consultoría legal y representación en casos civiles.",
                        ExperienceYears = 10,
                        HourlyRate = 80.0m,
                        RatingAverage = 4.5,
                        TotalReviews = 25,
                        Location = "Medellín, Colombia",
                        Status = 1, // Active
                        Services = new List<object>
                        {
                            new { Name = "Consulta Legal Virtual", Price = 80m, IsActive = true },
                            new { Name = "Revisión de Contratos", Price = 150m, IsActive = true }
                        }
                    },
                    new {
                        UserId = "507f1f77cf86cd799439012",
                        Specialties = new List<string> { "Contador", "Contabilidad" },
                        Bio = "Contador público con experiencia en contabilidad empresarial y asesoría fiscal. Especializado en pequeñas y medianas empresas.",
                        ExperienceYears = 8,
                        HourlyRate = 60.0m,
                        RatingAverage = 4.2,
                        TotalReviews = 18,
                        Location = "Bogotá, Colombia",
                        Status = 1,
                        Services = new List<object>
                        {
                            new { Name = "Asesoría Contable", Price = 60m, IsActive = true },
                            new { Name = "Declaración de Impuestos", Price = 200m, IsActive = true }
                        }
                    },
                    new {
                        UserId = "507f1f77cf86cd799439013",
                        Specialties = new List<string> { "Diseñador", "Diseño Gráfico" },
                        Bio = "Diseñador gráfico creativo con experiencia en branding, diseño web y material promocional. Trabajo con empresas de diversos sectores.",
                        ExperienceYears = 5,
                        HourlyRate = 45.0m,
                        RatingAverage = 4.8,
                        TotalReviews = 32,
                        Location = "Cali, Colombia",
                        Status = 1,
                        Services = new List<object>
                        {
                            new { Name = "Diseño de Logo", Price = 300m, IsActive = true },
                            new { Name = "Diseño Web", Price = 45m, IsActive = true }
                        }
                    }
                };

                _logger.LogInformation("Datos de prueba generados: {Count} perfiles", testProfiles.Count);
                
                return Ok(new { 
                    message = "Datos de prueba generados exitosamente",
                    profiles = testProfiles,
                    instructions = "Estos datos deben ser insertados manualmente en la base de datos MongoDB para probar el sistema de búsqueda."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar datos de prueba");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de búsqueda para debugging.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSearchStats()
        {
            try
            {
                // Aquí podrías implementar estadísticas de búsqueda
                var stats = new
                {
                    TotalProfiles = 0, // Implementar conteo real
                    ActiveProfiles = 0,
                    AverageRating = 0.0,
                    AverageHourlyRate = 0.0               };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de búsqueda");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
} 