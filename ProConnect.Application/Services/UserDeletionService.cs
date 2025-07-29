using ProConnect.Application.Interfaces;
using ProConnect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ProConnect.Application.Services
{
    /// <summary>
    /// Servicio para la eliminación completa de usuarios y sus datos relacionados
    /// </summary>
    public class UserDeletionService : IUserDeletionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfessionalProfileRepository _professionalProfileRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly ILogger<UserDeletionService> _logger;

        public UserDeletionService(
            IUserRepository userRepository,
            IProfessionalProfileRepository professionalProfileRepository,
            IBookingRepository bookingRepository,
            IReviewRepository reviewRepository,
            IPortfolioRepository portfolioRepository,
            ILogger<UserDeletionService> logger)
        {
            _userRepository = userRepository;
            _professionalProfileRepository = professionalProfileRepository;
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;
            _portfolioRepository = portfolioRepository;
            _logger = logger;
        }

        /// <summary>
        /// Elimina un usuario permanentemente junto con todos sus datos relacionados
        /// </summary>
        public async Task<bool> DeleteUserPermanentlyAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Iniciando eliminación permanente del usuario: {UserId}", userId);

                // 1. Verificar que el usuario existe
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado para eliminación permanente: {UserId}", userId);
                    return false;
                }

                // 2. Eliminar perfil profesional si existe
                await DeleteProfessionalProfileAsync(userId);

                // 3. Cancelar todas las reservas del usuario (como cliente)
                await CancelUserBookingsAsync(userId);

                // 4. Cancelar todas las reservas del profesional (si es profesional)
                await CancelProfessionalBookingsAsync(userId);

                // 5. Eliminar todas las reseñas asociadas
                await DeleteUserReviewsAsync(userId);

                // 6. Eliminar archivos de portafolio si existen
                await DeletePortfolioFilesAsync(userId);

                // 7. Finalmente, eliminar el usuario de la base de datos
                var deleteResult = await _userRepository.DeleteAsync(userId);
                
                if (deleteResult)
                {
                    _logger.LogInformation("Usuario eliminado permanentemente: {UserId}", userId);
                }
                else
                {
                    _logger.LogError("Error al eliminar usuario de la base de datos: {UserId}", userId);
                }

                return deleteResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario permanentemente: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Elimina el perfil profesional del usuario
        /// </summary>
        private async Task DeleteProfessionalProfileAsync(string userId)
        {
            try
            {
                var profile = await _professionalProfileRepository.GetByUserIdAsync(userId);
                if (profile != null)
                {
                    var result = await _professionalProfileRepository.DeleteAsync(profile.Id);
                    if (result)
                    {
                        _logger.LogInformation("Perfil profesional eliminado para usuario: {UserId}", userId);
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo eliminar el perfil profesional para usuario: {UserId}", userId);
                    }
                }
                else
                {
                    _logger.LogInformation("No se encontró perfil profesional para eliminar: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar perfil profesional: {UserId}", userId);
            }
        }

        /// <summary>
        /// Cancela todas las reservas donde el usuario es cliente
        /// </summary>
        private async Task CancelUserBookingsAsync(string userId)
        {
            try
            {
                var userBookings = await _bookingRepository.GetByClientIdAsync(userId);
                var pendingBookings = userBookings.Where(b => b.Status == Core.Entities.BookingStatus.Pending || 
                                                             b.Status == Core.Entities.BookingStatus.Confirmed);

                foreach (var booking in pendingBookings)
                {
                    booking.Cancel("Sistema", "Usuario eliminado");
                    await _bookingRepository.UpdateAsync(booking);
                }

                if (pendingBookings.Any())
                {
                    _logger.LogInformation("Canceladas {Count} reservas como cliente para usuario: {UserId}", 
                        pendingBookings.Count(), userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reservas como cliente: {UserId}", userId);
            }
        }

        /// <summary>
        /// Cancela todas las reservas donde el usuario es profesional
        /// </summary>
        private async Task CancelProfessionalBookingsAsync(string userId)
        {
            try
            {
                // Primero obtener el perfil profesional para obtener su ID
                var profile = await _professionalProfileRepository.GetByUserIdAsync(userId);
                if (profile != null)
                {
                    var professionalBookings = await _bookingRepository.GetByProfessionalIdAsync(profile.Id);
                    var pendingBookings = professionalBookings.Where(b => b.Status == Core.Entities.BookingStatus.Pending || 
                                                                         b.Status == Core.Entities.BookingStatus.Confirmed);

                    foreach (var booking in pendingBookings)
                    {
                        booking.Cancel("Sistema", "Profesional eliminado");
                        await _bookingRepository.UpdateAsync(booking);
                    }

                    if (pendingBookings.Any())
                    {
                        _logger.LogInformation("Canceladas {Count} reservas como profesional para usuario: {UserId}", 
                            pendingBookings.Count(), userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reservas como profesional: {UserId}", userId);
            }
        }

        /// <summary>
        /// Elimina todas las reseñas asociadas al usuario
        /// </summary>
        private async Task DeleteUserReviewsAsync(string userId)
        {
            try
            {
                // Eliminar reseñas donde el usuario es cliente
                var clientReviews = await _reviewRepository.GetByClientIdAsync(userId);
                foreach (var review in clientReviews)
                {
                    await _reviewRepository.DeleteAsync(review.Id);
                }

                // Eliminar reseñas donde el usuario es profesional
                var profile = await _professionalProfileRepository.GetByUserIdAsync(userId);
                if (profile != null)
                {
                    var professionalReviews = await _reviewRepository.GetByProfessionalIdAsync(profile.Id);
                    foreach (var review in professionalReviews)
                    {
                        await _reviewRepository.DeleteAsync(review.Id);
                    }
                }

                var totalReviews = clientReviews.Count() + (profile != null ? 
                    (await _reviewRepository.GetByProfessionalIdAsync(profile.Id)).Count() : 0);

                if (totalReviews > 0)
                {
                    _logger.LogInformation("Eliminadas {Count} reseñas para usuario: {UserId}", totalReviews, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reseñas: {UserId}", userId);
            }
        }

        /// <summary>
        /// Elimina archivos de portafolio del usuario
        /// </summary>
        private async Task DeletePortfolioFilesAsync(string userId)
        {
            try
            {
                var portfolioFiles = await _portfolioRepository.GetFilesByUserAsync(userId);
                foreach (var file in portfolioFiles)
                {
                    await _portfolioRepository.DeleteFileAsync(userId, file.Id.ToString());
                }

                if (portfolioFiles.Any())
                {
                    _logger.LogInformation("Eliminados {Count} archivos de portafolio para usuario: {UserId}", 
                        portfolioFiles.Count(), userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivos de portafolio: {UserId}", userId);
            }
        }
    }
} 