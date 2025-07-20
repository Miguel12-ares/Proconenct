using ProConnect.Application.DTOs;

namespace ProConnect.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto, string clientId);
        Task<BookingDto?> GetBookingByIdAsync(string bookingId, string userId);
        Task<List<BookingDto>> GetBookingsByClientAsync(string clientId, int limit = 20, int offset = 0);
        Task<List<BookingDto>> GetBookingsByProfessionalAsync(string professionalId, int limit = 20, int offset = 0);
        Task<BookingDto> UpdateBookingAsync(string bookingId, UpdateBookingDto updateDto, string userId);
        Task<bool> CancelBookingAsync(string bookingId, string userId, string? reason = null);
        Task<bool> ConfirmBookingAsync(string bookingId, string professionalId);
        Task<bool> CompleteBookingAsync(string bookingId, string userId);
        Task<bool> HasConflictAsync(string professionalId, DateTime appointmentDate, int duration, string? excludeBookingId = null);
        Task<List<BookingDto>> GetUpcomingBookingsAsync(string professionalId, DateTime fromDate, int limit = 10);
        Task<long> GetBookingCountByStatusAsync(string userId, string status);
    }
} 