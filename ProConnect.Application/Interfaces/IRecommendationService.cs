using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Application.DTOs;

namespace ProConnect.Application.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<ProfessionalRecommendationDto>> GetRecommendedProfessionalsAsync(string? userLocation = null, int limit = 10);
    }
} 