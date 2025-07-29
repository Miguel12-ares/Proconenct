using ProConnect.Application.DTOs;

namespace ProConnect.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene el perfil de un usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Perfil del usuario o null si no existe</returns>
        Task<UserProfileDto?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Actualiza el perfil de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="updateDto">Datos para actualizar</param>
        /// <returns>Perfil actualizado</returns>
        Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);

        /// <summary>
        /// Verifica si un usuario existe
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>True si el usuario existe</returns>
        Task<bool> UserExistsAsync(string userId);

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Perfil del usuario o null si no existe</returns>
        Task<UserProfileDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Elimina un usuario (soft delete) - Para uso administrativo
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Elimina un usuario permanentemente junto con todos sus datos relacionados
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteUserPermanentlyAsync(string userId);
    }
}
