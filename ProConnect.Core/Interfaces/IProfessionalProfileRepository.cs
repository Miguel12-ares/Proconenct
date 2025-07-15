using ProConnect.Core.Entities;
using ProConnect.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProConnect.Core.Interfaces
{
    /// <summary>
    /// Contrato para el acceso a la colección de perfiles profesionales.
    /// </summary>
    public interface IProfessionalProfileRepository
    {
        Task<ProfessionalProfile?> GetByIdAsync(string id);
        Task<ProfessionalProfile?> GetByUserIdAsync(string userId);
        Task<string> CreateAsync(ProfessionalProfile profile);
        Task<bool> UpdateAsync(ProfessionalProfile profile);
        Task<bool> DeleteAsync(string id);
        Task<List<ProfessionalProfile>> GetAllAsync();
        Task<List<ProfessionalProfile>> GetBySpecialtyAsync(string specialty);
        
        /// <summary>
        /// Obtiene perfiles activos con filtros opcionales.
        /// </summary>
        Task<List<ProfessionalProfile>> GetActiveProfilesAsync(
            string? specialty = null,
            string? location = null,
            decimal? minHourlyRate = null,
            decimal? maxHourlyRate = null,
            int? minExperienceYears = null,
            double? minRating = null);

        /// <summary>
        /// Busca perfiles por texto en bio, especialidades y ubicación.
        /// </summary>
        Task<List<ProfessionalProfile>> SearchProfilesAsync(string searchTerm);

        /// <summary>
        /// Obtiene perfiles ordenados por calificación.
        /// </summary>
        Task<List<ProfessionalProfile>> GetTopRatedProfilesAsync(int limit = 10);

        /// <summary>
        /// Obtiene perfiles por ubicación.
        /// </summary>
        Task<List<ProfessionalProfile>> GetProfilesByLocationAsync(string location);

        /// <summary>
        /// Verifica si un usuario ya tiene un perfil profesional.
        /// </summary>
        Task<bool> UserHasProfileAsync(string userId);

        /// <summary>
        /// Obtiene estadísticas de perfiles profesionales.
        /// </summary>
        Task<ProfileStatistics> GetProfileStatisticsAsync();
    }
} 