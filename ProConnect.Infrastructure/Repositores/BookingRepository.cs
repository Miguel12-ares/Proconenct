using MongoDB.Driver;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace ProConnect.Infrastructure.Repositores
{


    public class BookingRepository : IBookingRepository
    {
        private readonly IMongoCollection<Booking> _bookings;
        private readonly ILogger<BookingRepository> _logger;

        public BookingRepository(MongoDbContext context, ILogger<BookingRepository> logger)
        {
            _bookings = context.Database.GetCollection<Booking>("bookings");
            _logger = logger;
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            try
            {
                booking.CreatedAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                
                await _bookings.InsertOneAsync(booking);
                _logger.LogInformation("Booking created successfully. ID: {BookingId}", booking.Id);
                
                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for client {ClientId} and professional {ProfessionalId}", 
                    booking.ClientId, booking.ProfessionalId);
                throw;
            }
        }

        public async Task<Booking?> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.Id, id);
                return await _bookings.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by ID: {BookingId}", id);
                throw;
            }
        }

        public async Task<List<Booking>> GetByClientIdAsync(string clientId, int limit = 20, int offset = 0)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.ClientId, clientId);
                var sort = Builders<Booking>.Sort.Descending(x => x.AppointmentDate);
                
                return await _bookings.Find(filter)
                    .Sort(sort)
                    .Skip(offset)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for client: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<Booking>> GetByProfessionalIdAsync(string professionalId, int limit = 20, int offset = 0)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId);
                var sort = Builders<Booking>.Sort.Descending(x => x.AppointmentDate);
                
                return await _bookings.Find(filter)
                    .Sort(sort)
                    .Skip(offset)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for professional: {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<List<Booking>> GetByStatusAsync(BookingStatus status, int limit = 20, int offset = 0)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.Status, status);
                var sort = Builders<Booking>.Sort.Descending(x => x.AppointmentDate);
                
                return await _bookings.Find(filter)
                    .Sort(sort)
                    .Skip(offset)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings by status: {Status}", status);
                throw;
            }
        }

        public async Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 20, int offset = 0)
        {
            try
            {
                var filter = Builders<Booking>.Filter.And(
                    Builders<Booking>.Filter.Gte(x => x.AppointmentDate, startDate),
                    Builders<Booking>.Filter.Lte(x => x.AppointmentDate, endDate)
                );
                var sort = Builders<Booking>.Sort.Ascending(x => x.AppointmentDate);
                
                return await _bookings.Find(filter)
                    .Sort(sort)
                    .Skip(offset)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings by date range: {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            try
            {
                booking.UpdatedAt = DateTime.UtcNow;
                
                var filter = Builders<Booking>.Filter.Eq(x => x.Id, booking.Id);
                var options = new ReplaceOptions { IsUpsert = false };
                
                await _bookings.ReplaceOneAsync(filter, booking, options);
                _logger.LogInformation("Booking updated successfully. ID: {BookingId}", booking.Id);
                
                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking: {BookingId}", booking.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.Id, id);
                var result = await _bookings.DeleteOneAsync(filter);
                
                _logger.LogInformation("Booking deleted successfully. ID: {BookingId}, DeletedCount: {DeletedCount}", 
                    id, result.DeletedCount);
                
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking: {BookingId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.Id, id);
                return await _bookings.Find(filter).AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if booking exists: {BookingId}", id);
                throw;
            }
        }

        public async Task<bool> HasConflictAsync(string professionalId, DateTime appointmentDate, int duration, string? excludeBookingId = null)
        {
            try
            {
                var appointmentEnd = appointmentDate.AddMinutes(duration);
                
                // Obtener todas las reservas del profesional que no estén canceladas
                var existingBookings = await _bookings.Find(x => 
                    x.ProfessionalId == professionalId && 
                    x.Status != BookingStatus.Cancelled)
                    .ToListAsync();

                // Excluir la reserva actual si se está actualizando
                if (!string.IsNullOrEmpty(excludeBookingId))
                {
                    existingBookings = existingBookings.Where(x => x.Id != excludeBookingId).ToList();
                }

                // Verificar conflictos en memoria
                foreach (var existingBooking in existingBookings)
                {
                    var existingEnd = existingBooking.AppointmentDate.AddMinutes(existingBooking.AppointmentDuration);
                    
                    // Verificar si hay solapamiento
                    if (appointmentDate < existingEnd && appointmentEnd > existingBooking.AppointmentDate)
                    {
                        _logger.LogWarning("Booking conflict detected for professional {ProfessionalId} at {AppointmentDate}", 
                            professionalId, appointmentDate);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking booking conflicts for professional {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<List<Booking>> GetUpcomingBookingsAsync(string professionalId, DateTime fromDate, int limit = 10)
        {
            try
            {
                var filter = Builders<Booking>.Filter.And(
                    Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId),
                    Builders<Booking>.Filter.Gte(x => x.AppointmentDate, fromDate),
                    Builders<Booking>.Filter.Ne(x => x.Status, BookingStatus.Cancelled)
                );
                var sort = Builders<Booking>.Sort.Ascending(x => x.AppointmentDate);
                
                return await _bookings.Find(filter)
                    .Sort(sort)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings for professional: {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<List<Booking>> GetPendingBookingsAsync(string professionalId, int limit = 20)
        {
            try
            {
                var filter = Builders<Booking>.Filter.And(
                    Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId),
                    Builders<Booking>.Filter.Eq(x => x.Status, BookingStatus.Pending)
                );
                var sort = Builders<Booking>.Sort.Ascending(x => x.AppointmentDate);
                
                return await _bookings.Find(filter)
                    .Sort(sort)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending bookings for professional: {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<long> GetCountByStatusAsync(BookingStatus status)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.Status, status);
                return await _bookings.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting bookings by status: {Status}", status);
                throw;
            }
        }

        public async Task<long> GetCountByProfessionalAsync(string professionalId)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId);
                return await _bookings.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting bookings for professional: {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<long> GetCountByClientAsync(string clientId)
        {
            try
            {
                var filter = Builders<Booking>.Filter.Eq(x => x.ClientId, clientId);
                return await _bookings.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking count for client: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<Booking>> GetByClientIdWithFiltersAsync(string clientId, string? status, DateTime? dateFrom, DateTime? dateTo, string? professionalId, int limit, int offset)
        {
            try
            {
                var filters = new List<FilterDefinition<Booking>>
                {
                    Builders<Booking>.Filter.Eq(x => x.ClientId, clientId)
                };

                // Aplicar filtro de estado si se proporciona
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                {
                    filters.Add(Builders<Booking>.Filter.Eq(x => x.Status, bookingStatus));
                }

                // Aplicar filtro de fecha desde
                if (dateFrom.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Gte(x => x.AppointmentDate, dateFrom.Value));
                }

                // Aplicar filtro de fecha hasta
                if (dateTo.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Lte(x => x.AppointmentDate, dateTo.Value));
                }

                // Aplicar filtro de profesional si se proporciona
                if (!string.IsNullOrEmpty(professionalId))
                {
                    filters.Add(Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId));
                }

                var combinedFilter = Builders<Booking>.Filter.And(filters);
                var sort = Builders<Booking>.Sort.Descending(x => x.AppointmentDate);

                return await _bookings.Find(combinedFilter)
                    .Sort(sort)
                    .Skip(offset)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings with filters for client: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<Booking>> GetByProfessionalIdWithFiltersAsync(string professionalId, string? status, DateTime? dateFrom, DateTime? dateTo, int limit, int offset)
        {
            try
            {
                var filters = new List<FilterDefinition<Booking>>
                {
                    Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId)
                };

                // Aplicar filtro de estado si se proporciona
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                {
                    filters.Add(Builders<Booking>.Filter.Eq(x => x.Status, bookingStatus));
                }

                // Aplicar filtro de fecha desde
                if (dateFrom.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Gte(x => x.AppointmentDate, dateFrom.Value));
                }

                // Aplicar filtro de fecha hasta
                if (dateTo.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Lte(x => x.AppointmentDate, dateTo.Value));
                }

                var combinedFilter = Builders<Booking>.Filter.And(filters);
                var sort = Builders<Booking>.Sort.Descending(x => x.AppointmentDate);

                return await _bookings.Find(combinedFilter)
                    .Sort(sort)
                    .Skip(offset)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings with filters for professional: {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<long> GetCountByClientIdWithFiltersAsync(string clientId, string? status, DateTime? dateFrom, DateTime? dateTo, string? professionalId)
        {
            try
            {
                var filters = new List<FilterDefinition<Booking>>
                {
                    Builders<Booking>.Filter.Eq(x => x.ClientId, clientId)
                };

                // Aplicar filtro de estado si se proporciona
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                {
                    filters.Add(Builders<Booking>.Filter.Eq(x => x.Status, bookingStatus));
                }

                // Aplicar filtro de fecha desde
                if (dateFrom.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Gte(x => x.AppointmentDate, dateFrom.Value));
                }

                // Aplicar filtro de fecha hasta
                if (dateTo.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Lte(x => x.AppointmentDate, dateTo.Value));
                }

                // Aplicar filtro de profesional si se proporciona
                if (!string.IsNullOrEmpty(professionalId))
                {
                    filters.Add(Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId));
                }

                var combinedFilter = Builders<Booking>.Filter.And(filters);
                return await _bookings.CountDocumentsAsync(combinedFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking count with filters for client: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<long> GetCountByProfessionalIdWithFiltersAsync(string professionalId, string? status, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                var filters = new List<FilterDefinition<Booking>>
                {
                    Builders<Booking>.Filter.Eq(x => x.ProfessionalId, professionalId)
                };

                // Aplicar filtro de estado si se proporciona
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                {
                    filters.Add(Builders<Booking>.Filter.Eq(x => x.Status, bookingStatus));
                }

                // Aplicar filtro de fecha desde
                if (dateFrom.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Gte(x => x.AppointmentDate, dateFrom.Value));
                }

                // Aplicar filtro de fecha hasta
                if (dateTo.HasValue)
                {
                    filters.Add(Builders<Booking>.Filter.Lte(x => x.AppointmentDate, dateTo.Value));
                }

                var combinedFilter = Builders<Booking>.Filter.And(filters);
                return await _bookings.CountDocumentsAsync(combinedFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking count with filters for professional: {ProfessionalId}", professionalId);
                throw;
            }
        }
    }
} 