using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ProConnect.Infrastructure.Services
{
    /// <summary>
    /// Servicio para la gestión de usuarios
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserDeletionService _userDeletionService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IUserDeletionService userDeletionService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userDeletionService = userDeletionService;
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
                return null;
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

                // Actualizar campos permitidos
                if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName;

                if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                    user.LastName = updateDto.LastName;

                if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
                    user.PhoneNumber = updateDto.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(updateDto.Bio))
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
                return null;
            }
        }

        /// <summary>
        /// Elimina un usuario (soft delete) - Para uso administrativo
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

        /// <summary>
        /// Elimina un usuario permanentemente junto con todos sus datos relacionados
        /// </summary>
        public async Task<bool> DeleteUserPermanentlyAsync(string userId)
        {
            return await _userDeletionService.DeleteUserPermanentlyAsync(userId);
        }

        #region Private Methods

        /// <summary>
        /// Mapea la entidad User a UserProfileDto
        /// </summary>
        private UserProfileDto MapToUserProfileDto(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                UserType = user.UserType,
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        #endregion
    }
}
