using ProConnect.Core.Entities;
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
        // Otros métodos de búsqueda avanzada pueden agregarse aquí
    }
} 