using ProConnect.Core.Entities;

namespace ProConnect.Core.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking?> GetByIdAsync(string id);
        Task<List<Booking>> GetByClientIdAsync(string clientId, int limit = 20, int offset = 0);
        Task<List<Booking>> GetByProfessionalIdAsync(string professionalId, int limit = 20, int offset = 0);
        Task<List<Booking>> GetByStatusAsync(BookingStatus status, int limit = 20, int offset = 0);
        Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 20, int offset = 0);
        Task<Booking> UpdateAsync(Booking booking);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> HasConflictAsync(string professionalId, DateTime appointmentDate, int duration, string? excludeBookingId = null);
        Task<List<Booking>> GetUpcomingBookingsAsync(string professionalId, DateTime fromDate, int limit = 10);
        Task<List<Booking>> GetPendingBookingsAsync(string professionalId, int limit = 20);
        Task<long> GetCountByStatusAsync(BookingStatus status);
        Task<long> GetCountByProfessionalAsync(string professionalId);
        Task<long> GetCountByClientAsync(string clientId);
        
        // Nuevos métodos para filtros avanzados
        Task<List<Booking>> GetByClientIdWithFiltersAsync(string clientId, string? status, DateTime? dateFrom, DateTime? dateTo, string? professionalId, int limit, int offset);
        Task<List<Booking>> GetByProfessionalIdWithFiltersAsync(string professionalId, string? status, DateTime? dateFrom, DateTime? dateTo, int limit, int offset);
        Task<long> GetCountByClientIdWithFiltersAsync(string clientId, string? status, DateTime? dateFrom, DateTime? dateTo, string? professionalId);
        Task<long> GetCountByProfessionalIdWithFiltersAsync(string professionalId, string? status, DateTime? dateFrom, DateTime? dateTo);
    }
} 