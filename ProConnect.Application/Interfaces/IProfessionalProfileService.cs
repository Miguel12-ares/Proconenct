using ProConnect.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProConnect.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de perfiles profesionales.
    /// </summary>
    public interface IProfessionalProfileService
    {
        /// <summary>
        /// Crea un nuevo perfil profesional.
        /// </summary>
        Task<ProfessionalProfileResponseDto> CreateProfileAsync(CreateProfessionalProfileDto createDto, string userId);

        /// <summary>
        /// Actualiza un perfil profesional existente.
        /// </summary>
        Task<ProfessionalProfileResponseDto> UpdateProfileAsync(string profileId, UpdateProfessionalProfileDto updateDto, string userId);

        /// <summary>
        /// Obtiene el perfil profesional del usuario autenticado.
        /// </summary>
        Task<ProfessionalProfileResponseDto?> GetMyProfileAsync(string userId);

        /// <summary>
        /// Obtiene un perfil profesional público por ID.
        /// </summary>
        Task<ProfessionalProfileResponseDto?> GetPublicProfileAsync(string profileId);

        /// <summary>
        /// Obtiene todos los perfiles profesionales activos.
        /// </summary>
        Task<List<ProfessionalProfileResponseDto>> GetAllActiveProfilesAsync();

        /// <summary>
        /// Busca perfiles por especialidad.
        /// </summary>
        Task<List<ProfessionalProfileResponseDto>> GetProfilesBySpecialtyAsync(string specialty);
    }
} 