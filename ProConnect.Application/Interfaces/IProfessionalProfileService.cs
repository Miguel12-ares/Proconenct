using ProConnect.Application.DTOs;
using System.Threading.Tasks;

namespace ProConnect.Application.Interfaces
{
    /// <summary>
    /// Contrato para la lógica de negocio de perfiles profesionales.
    /// </summary>
    public interface IProfessionalProfileService
    {
        Task<ProfessionalProfileDto?> GetByUserIdAsync(string userId);
        // Otros métodos de negocio pueden agregarse aquí
    }
} 