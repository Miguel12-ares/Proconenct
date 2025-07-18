using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProConnect.Application.DTOs;
using ProConnect.Core.Entities;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Interfaces;

namespace ProConnect.Application.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IProfessionalProfileRepository _profileRepository;

        public RecommendationService(IProfessionalProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<List<ProfessionalRecommendationDto>> GetRecommendedProfessionalsAsync(string? userLocation = null, int limit = 10)
        {
            var profiles = await _profileRepository.GetAllAsync();
            var now = DateTime.UtcNow;

            // Solo perfiles activos y completos
            var filtered = profiles.Where(p => p.Status == ProfileStatus.Active && p.IsCompleteForPublicView()).ToList();

            // Calculo de score
            var scored = filtered.Select(p => new
            {
                Profile = p,
                Score = CalcularScore(p, userLocation, now)
            }).ToList();

            // Ordenar por score descendente
            var ordered = scored.OrderByDescending(x => x.Score).ToList();

            // Diversidad: max 2 por especialidad en top 10
            var top = new List<ProfessionalRecommendationDto>();
            var specialtyCount = new Dictionary<string, int>();
            foreach (var item in ordered)
            {
                bool canAdd = true;
                foreach (var spec in item.Profile.Specialties)
                {
                    if (!specialtyCount.ContainsKey(spec)) specialtyCount[spec] = 0;
                    if (specialtyCount[spec] >= 2)
                    {
                        canAdd = false;
                        break;
                    }
                }
                if (canAdd)
                {
                    top.Add(ToDto(item.Profile, item.Score));
                    foreach (var spec in item.Profile.Specialties)
                        specialtyCount[spec]++;
                }
                if (top.Count >= limit)
                    break;
            }
            // Fallback: al menos 5 recomendaciones
            if (top.Count < 5)
            {
                var faltantes = ordered.Select(x => x.Profile)
                    .Where(p => !top.Any(t => t.Id == p.Id))
                    .Take(5 - top.Count)
                    .Select(p => ToDto(p, CalcularScore(p, userLocation, now)));
                top.AddRange(faltantes);
            }
            return top;
        }

        private double CalcularScore(ProfessionalProfile p, string? userLocation, DateTime now)
        {
            // Score base: rating promedio (peso 0.5), total reviews (peso 0.2), actividad reciente (peso 0.2), proximidad (peso 0.1)
            double score = 0;
            score += (p.RatingAverage / 5.0) * 0.5;
            score += Math.Min(p.TotalReviews, 50) / 50.0 * 0.2; // max boost 50 reviews
            var daysSinceUpdate = (now - p.UpdatedAt).TotalDays;
            score += (daysSinceUpdate < 30 ? 1 : Math.Max(0, 1 - (daysSinceUpdate - 30) / 60.0)) * 0.2;
            if (!string.IsNullOrWhiteSpace(userLocation) && !string.IsNullOrWhiteSpace(p.Location))
            {
                score += (string.Equals(userLocation.Trim(), p.Location.Trim(), StringComparison.OrdinalIgnoreCase) ? 0.1 : 0);
            }
            return score;
        }

        private ProfessionalRecommendationDto ToDto(ProfessionalProfile p, double score)
        {
            return new ProfessionalRecommendationDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = "", // Se puede obtener del usuario si se requiere
                Specialties = p.Specialties,
                Bio = p.Bio,
                ExperienceYears = p.ExperienceYears,
                HourlyRate = p.HourlyRate,
                RatingAverage = p.RatingAverage,
                TotalReviews = p.TotalReviews,
                Location = p.Location,
                HasVirtualConsultation = p.Services.Any(s => s.Type == ServiceType.Hourly && s.Name.ToLower().Contains("virtual")),
                Services = p.Services.Select(s => s.Name).ToList(),
                Score = Math.Round(score, 4)
            };
        }
    }
} 