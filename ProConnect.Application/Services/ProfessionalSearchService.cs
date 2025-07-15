using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProConnect.Application.Services
{
    public class ProfessionalSearchService : IProfessionalSearchService
    {
        private readonly IProfessionalProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;

        public ProfessionalSearchService(IProfessionalProfileRepository profileRepository, IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResultDto<ProfessionalSearchResultDto>> SearchProfessionalsAsync(ProfessionalSearchFiltersDto filtersDto)
        {
            // Mapear DTO a modelo de dominio
            var filters = new ProfessionalSearchFilters
            {
                Query = filtersDto.Query,
                Specialties = filtersDto.Specialties,
                Location = filtersDto.Location,
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

            // Obtener usuarios para los perfiles encontrados
            var userIds = pagedResult.Items.Select(p => p.UserId).Distinct().ToList();
            var users = new Dictionary<string, Core.Entities.User>();
            foreach (var userId in userIds)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                    users[userId] = user;
            }

            // Mapear a DTO de resultado
            var items = pagedResult.Items.Select(profile => new ProfessionalSearchResultDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = users.ContainsKey(profile.UserId) ? $"{users[profile.UserId].FirstName} {users[profile.UserId].LastName}" : string.Empty,
                Specialties = profile.Specialties,
                Bio = profile.Bio,
                ExperienceYears = profile.ExperienceYears,
                HourlyRate = profile.HourlyRate,
                RatingAverage = profile.RatingAverage,
                TotalReviews = profile.TotalReviews,
                Location = profile.Location,
                HasVirtualConsultation = profile.Services.Any(s => s.Name.ToLower().Contains("virtual")),
                Services = profile.Services.Select(s => s.Name).ToList()
            }).ToList();

            return new PagedResultDto<ProfessionalSearchResultDto>
            {
                Items = items,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }
    }
} 