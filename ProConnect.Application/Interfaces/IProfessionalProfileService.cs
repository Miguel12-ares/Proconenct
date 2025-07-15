using ProConnect.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using ProConnect.Application.DTOs.Shared;

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

        /// <summary>
        /// Actualiza el horario de disponibilidad del profesional.
        /// </summary>
        Task<bool> UpdateAvailabilityScheduleAsync(string userId, AvailabilityScheduleDto scheduleDto);

        /// <summary>
        /// Agrega un bloqueo de disponibilidad para el profesional.
        /// </summary>
        Task<AvailabilityBlockDto> AddAvailabilityBlockAsync(string userId, CreateAvailabilityBlockDto blockDto);

        /// <summary>
        /// Obtiene todos los bloqueos de disponibilidad del profesional.
        /// </summary>
        Task<List<AvailabilityBlockDto>> GetAvailabilityBlocksAsync(string userId);

        /// <summary>
        /// Elimina un bloqueo de disponibilidad por id.
        /// </summary>
        Task<bool> DeleteAvailabilityBlockAsync(string userId, string blockId);

        /// <summary>
        /// Consulta los slots disponibles para una fecha específica.
        /// </summary>
        Task<AvailabilityCheckResponseDto> CheckAvailabilityAsync(string userId, DateTime date);
    }
} 