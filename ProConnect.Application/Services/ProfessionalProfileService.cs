using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Interfaces;
using System.Threading.Tasks;

namespace ProConnect.Application.Services
{
    /// <summary>
    /// Servicio de lógica de negocio para perfiles profesionales.
    /// </summary>
    public class ProfessionalProfileService : IProfessionalProfileService
    {
        private readonly IProfessionalProfileRepository _profileRepository;

        public ProfessionalProfileService(IProfessionalProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<ProfessionalProfileDto?> GetByUserIdAsync(string userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;
            return new ProfessionalProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Specialties = profile.Specialties,
                Bio = profile.Bio,
                ExperienceYears = profile.ExperienceYears,
                HourlyRate = profile.HourlyRate,
                Credentials = profile.Credentials,
                PortfolioItems = profile.PortfolioItems,
                RatingAverage = profile.RatingAverage,
                TotalReviews = profile.TotalReviews,
                Location = profile.Location,
                AvailabilitySchedule = profile.AvailabilitySchedule
            };
        }
    }
} 