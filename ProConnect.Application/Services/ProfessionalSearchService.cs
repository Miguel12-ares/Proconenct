using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using System.Diagnostics;

namespace ProConnect.Application.Services
{
    // NOTA: Este servicio depende de IProfessionalProfileRepository, IUserRepository y IConnectionMultiplexer (Redis). La inyección de dependencias en Program.cs debe registrar IProfessionalSearchService -> ProfessionalSearchService y también IConnectionMultiplexer (ya realizado). Si se agregan nuevas dependencias, actualizar aquí y en el registro DI.
    public class ProfessionalSearchService : IProfessionalSearchService
    {
        private readonly IProfessionalProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;

        public ProfessionalSearchService(IProfessionalProfileRepository profileRepository, IUserRepository userRepository, ICacheService cacheService)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        public async Task<PagedResultDto<ProfessionalSearchResultDto>> SearchProfessionalsAsync(ProfessionalSearchFiltersDto filtersDto)
        {
            var stopwatch = Stopwatch.StartNew();
            // Validar parámetros de entrada
            if (filtersDto.Page < 1) filtersDto.Page = 1;
            if (filtersDto.PageSize < 1 || filtersDto.PageSize > 100) filtersDto.PageSize = 20;

            // Serializar filtros para clave de caché
            var cacheKey = $"search:{System.Text.Json.JsonSerializer.Serialize(filtersDto)}";
            var cached = await _cacheService.GetAsync<PagedResultDto<ProfessionalSearchResultDto>>(cacheKey);
            if (cached != null)
            {
                stopwatch.Stop();
                Console.WriteLine($"[SEARCH] Respuesta desde caché en {stopwatch.ElapsedMilliseconds} ms. Filtros: {cacheKey}");
                return cached;
            }

            // Mapear DTO a modelo de dominio
            var filters = new ProfessionalSearchFilters
            {
                Query = filtersDto.Query?.Trim(),
                Specialties = filtersDto.Specialties?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
                Location = filtersDto.Location?.Trim(),
                MinHourlyRate = filtersDto.MinHourlyRate,
                MaxHourlyRate = filtersDto.MaxHourlyRate,
                MinRating = filtersDto.MinRating,
                MinExperienceYears = filtersDto.MinExperienceYears,
                VirtualConsultation = filtersDto.VirtualConsultation,
                OrderBy = filtersDto.OrderBy,
                Page = filtersDto.Page,
                PageSize = filtersDto.PageSize
            };

            var pagedResult = await _profileRepository.SearchAdvancedAsync(filters);

            // Obtener usuarios para los perfiles encontrados de manera eficiente
            var userIds = pagedResult.Items.Select(p => p.UserId).Distinct().ToList();
            var users = new Dictionary<string, Core.Entities.User>();
            
            if (userIds.Any())
            {
                foreach (var userId in userIds)
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                        users[userId] = user;
                }
            }

            // Mapear a DTO de resultado
            var items = pagedResult.Items.Select(profile => new ProfessionalSearchResultDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = users.ContainsKey(profile.UserId) 
                    ? $"{users[profile.UserId].FirstName} {users[profile.UserId].LastName}".Trim()
                    : "Profesional",
                Bio = profile.Bio ?? string.Empty,
                ExperienceYears = profile.ExperienceYears,
                HourlyRate = profile.HourlyRate,
                RatingAverage = profile.RatingAverage,
                TotalReviews = profile.TotalReviews,
                Location = profile.Location ?? string.Empty,
                HasVirtualConsultation = profile.Services?.Any(s => 
                    s.Name.ToLower().Contains("virtual") || 
                    s.Name.ToLower().Contains("online") ||
                    s.Name.ToLower().Contains("remoto")) ?? false,
                Services = profile.Services?.Select(s => s.Name).ToList() ?? new List<string>()
            }).ToList();

            var result = new PagedResultDto<ProfessionalSearchResultDto>
            {
                Items = items,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / pagedResult.PageSize)
            };

            // Guardar en caché por 2 minutos
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
            stopwatch.Stop();
            Console.WriteLine($"[SEARCH] Respuesta desde base de datos en {stopwatch.ElapsedMilliseconds} ms. Filtros: {cacheKey}");
            return result;
        }
    }
} 