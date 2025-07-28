using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ProConnect.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de usuarios
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el perfil de un usuario por su ID
        /// </summary>
        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de usuario: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Actualiza el perfil de un usuario
        /// </summary>
        public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("Usuario no encontrado");
                }

                // Actualizar campos usando las propiedades correctas del DTO
                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName;

                if (!string.IsNullOrEmpty(updateDto.LastName))
                    user.LastName = updateDto.LastName;

                if (!string.IsNullOrEmpty(updateDto.Phone))
                    user.PhoneNumber = updateDto.Phone;

                if (!string.IsNullOrEmpty(updateDto.Bio))
                    user.Bio = updateDto.Bio;

                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Perfil de usuario actualizado: {UserId}", userId);
                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil de usuario: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un usuario existe
        /// </summary>
        public async Task<bool> UserExistsAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de usuario: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        public async Task<UserProfileDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return null;
                }

                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Elimina un usuario (soft delete)
        /// </summary>
        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Soft delete - marcar como eliminado
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Usuario eliminado (soft delete): {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {UserId}", userId);
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Mapea una entidad User a UserProfileDto
        /// </summary>
        private static UserProfileDto MapToUserProfileDto(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                Bio = user.Bio ?? string.Empty
            };
        }

        #endregion
    }
}
