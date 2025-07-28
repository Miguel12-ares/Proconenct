using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ProConnect.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IProfessionalProfileRepository _professionalRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IProfessionalProfileRepository professionalRepository,
            IUserRepository userRepository,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _professionalRepository = professionalRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto, string clientId)
        {
            try
            {
                // Usar el clientId del DTO si se proporciona, sino usar el del parámetro
                var actualClientId = !string.IsNullOrEmpty(createDto.ClientId) ? createDto.ClientId : clientId;

                // Validar que el profesional existe y está activo
                var professional = await _professionalRepository.GetByIdAsync(createDto.ProfessionalId);
                if (professional == null)
                {
                    throw new InvalidOperationException("El profesional no existe");
                }

                if (professional.Status != ProfileStatus.Active)
                {
                    throw new InvalidOperationException("El profesional no está disponible para reservas");
                }

                // Validar que la fecha no esté en el pasado
                if (createDto.AppointmentDate <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("No se pueden crear reservas en fechas pasadas");
                }

                // Validar que no haya conflictos de horario
                var hasConflict = await _bookingRepository.HasConflictAsync(
                    createDto.ProfessionalId, 
                    createDto.AppointmentDate, 
                    createDto.Duration);

                if (hasConflict)
                {
                    throw new InvalidOperationException("El horario seleccionado no está disponible");
                }

                // Parsear el tipo de consulta
                if (!Enum.TryParse<ConsultationType>(createDto.ConsultationType, true, out var consultationType))
                {
                    var validTypes = string.Join(", ", Enum.GetNames(typeof(ConsultationType)));
                    throw new InvalidOperationException($"Tipo de consulta no válido. Valores permitidos: {validTypes}");
                }

                // Calcular el costo total basado en la duración seleccionada y la tarifa del profesional
                var totalAmount = Math.Round(professional.HourlyRate * ((decimal)createDto.Duration / 60), 2);

                // Convertir la fecha/hora de la cita desde la zona horaria local del profesional a UTC
                DateTime appointmentUtc = createDto.AppointmentDate;
                try
                {
                    string tzId = professional.AvailabilitySchedule?.Timezone ?? "UTC";
                    TimeZoneInfo tz;
                    try
                    {
                        tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        _logger.LogWarning("Zona horaria '{Timezone}' no encontrada. Se usará UTC por defecto.", tzId);
                        tz = TimeZoneInfo.Utc;
                    }
                    catch (InvalidTimeZoneException)
                    {
                        _logger.LogWarning("Zona horaria '{Timezone}' inválida. Se usará UTC por defecto.", tzId);
                        tz = TimeZoneInfo.Utc;
                    }
                    // Interpretar la fecha recibida como hora local del profesional y convertir a UTC
                    appointmentUtc = TimeZoneInfo.ConvertTimeToUtc(createDto.AppointmentDate, tz);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al convertir la fecha de la cita a UTC. Se usará la fecha original recibida.");
                }

                // Crear la reserva
                var booking = new Booking
                {
                    ClientId = actualClientId,
                    ProfessionalId = createDto.ProfessionalId,
                    AppointmentDate = appointmentUtc,
                    AppointmentDuration = createDto.Duration,
                    ConsultationType = consultationType,
                    Status = BookingStatus.Pending,
                    TotalAmount = totalAmount,
                    SpecialNotes = createDto.Notes
                };

                var createdBooking = await _bookingRepository.CreateAsync(booking);
                _logger.LogInformation("Booking created successfully. ID: {BookingId}, Client: {ClientId}, Professional: {ProfessionalId}", 
                    createdBooking.Id, actualClientId, createDto.ProfessionalId);

                return await MapToBookingDtoAsync(createdBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for client {ClientId} and professional {ProfessionalId}", 
                    clientId, createDto.ProfessionalId);
                throw;
            }
        }

        public async Task<BookingDto?> GetBookingByIdAsync(string bookingId, string userId)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return null;
                }

                // Verificar que el usuario tiene acceso a esta reserva
                if (booking.ClientId != userId && booking.ProfessionalId != userId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para ver esta reserva");
                }

                return await MapToBookingDtoAsync(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId} for user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<List<BookingDto>> GetBookingsByClientAsync(string clientId, int limit = 20, int offset = 0)
        {
            try
            {
                var bookings = await _bookingRepository.GetByClientIdAsync(clientId, limit, offset);
                var bookingDtos = new List<BookingDto>();

                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToBookingDtoAsync(booking));
                }

                return bookingDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<BookingDto>> GetBookingsByProfessionalAsync(string professionalId, int limit = 20, int offset = 0)
        {
            try
            {
                var bookings = await _bookingRepository.GetByProfessionalIdAsync(professionalId, limit, offset);
                var bookingDtos = new List<BookingDto>();

                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToBookingDtoAsync(booking));
                }

                return bookingDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for professional {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<BookingDto> UpdateBookingAsync(string bookingId, UpdateBookingDto updateDto, string userId)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("La reserva no existe");
                }

                // Verificar que el usuario tiene permisos para actualizar esta reserva
                if (booking.ClientId != userId && booking.ProfessionalId != userId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para actualizar esta reserva");
                }

                // Solo permitir actualizaciones si la reserva no está completada o cancelada
                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                {
                    throw new InvalidOperationException("No se puede actualizar una reserva completada o cancelada");
                }

                // Actualizar campos si se proporcionan
                if (updateDto.AppointmentDate.HasValue)
                {
                    if (updateDto.AppointmentDate.Value <= DateTime.UtcNow)
                    {
                        throw new InvalidOperationException("No se pueden programar reservas en fechas pasadas");
                    }

                    // Verificar conflictos si se cambia la fecha
                    var hasConflict = await _bookingRepository.HasConflictAsync(
                        booking.ProfessionalId,
                        updateDto.AppointmentDate.Value,
                        updateDto.AppointmentDuration ?? booking.AppointmentDuration,
                        bookingId);

                    if (hasConflict)
                    {
                        throw new InvalidOperationException("El nuevo horario no está disponible");
                    }

                    booking.AppointmentDate = updateDto.AppointmentDate.Value;
                }

                if (updateDto.AppointmentDuration.HasValue)
                {
                    // Validar que la nueva duración esté en el rango permitido
                    if (updateDto.AppointmentDuration.Value < 15 || updateDto.AppointmentDuration.Value > 480)
                    {
                        throw new InvalidOperationException("La duración debe estar entre 15 y 480 minutos");
                    }

                    // Si solo se está cambiando la duración (no la fecha), verificar conflictos con la fecha actual
                    if (!updateDto.AppointmentDate.HasValue)
                    {
                        var hasConflict = await _bookingRepository.HasConflictAsync(
                            booking.ProfessionalId,
                            booking.AppointmentDate,
                            updateDto.AppointmentDuration.Value,
                            bookingId);

                        if (hasConflict)
                        {
                            throw new InvalidOperationException("La nueva duración genera un conflicto con otras reservas");
                        }
                    }

                    booking.AppointmentDuration = updateDto.AppointmentDuration.Value;
                }

                if (!string.IsNullOrEmpty(updateDto.ConsultationType))
                {
                    if (Enum.TryParse<ConsultationType>(updateDto.ConsultationType, true, out var consultationType))
                    {
                        booking.ConsultationType = consultationType;
                    }
                }

                if (updateDto.SpecialNotes != null)
                {
                    booking.SpecialNotes = updateDto.SpecialNotes;
                }

                if (!string.IsNullOrEmpty(updateDto.Status))
                {
                    if (Enum.TryParse<BookingStatus>(updateDto.Status, true, out var status))
                    {
                        booking.Status = status;
                    }
                }

                booking.UpdateModificationDate();
                var updatedBooking = await _bookingRepository.UpdateAsync(booking);

                _logger.LogInformation("Booking updated successfully. ID: {BookingId}, UpdatedBy: {UserId}", 
                    bookingId, userId);

                return await MapToBookingDtoAsync(updatedBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId} by user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(string bookingId, string userId, string? reason = null)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("La reserva no existe");
                }

                // Verificar que el usuario tiene permisos para cancelar esta reserva
                if (booking.ClientId != userId && booking.ProfessionalId != userId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para cancelar esta reserva");
                }

                // Verificar que la reserva puede ser cancelada
                if (!booking.CanBeCancelled())
                {
                    throw new InvalidOperationException("La reserva no puede ser cancelada en su estado actual");
                }

                // Verificar que no se cancela muy cerca de la hora de la cita (mínimo 2 horas)
                var timeUntilAppointment = booking.AppointmentDate - DateTime.UtcNow;
                if (timeUntilAppointment.TotalHours < 2)
                {
                    throw new InvalidOperationException("No se puede cancelar una reserva menos de 2 horas antes de la cita");
                }

                booking.Cancel(userId, reason);
                await _bookingRepository.UpdateAsync(booking);

                _logger.LogInformation("Booking cancelled successfully. ID: {BookingId}, CancelledBy: {UserId}, Reason: {Reason}", 
                    bookingId, userId, reason ?? "No reason provided");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId} by user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<bool> ConfirmBookingAsync(string bookingId, string professionalId)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("La reserva no existe");
                }

                // Verificar que el profesional es el propietario de la reserva
                if (booking.ProfessionalId != professionalId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para confirmar esta reserva");
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    throw new InvalidOperationException("Solo se pueden confirmar reservas pendientes");
                }

                booking.Confirm();
                await _bookingRepository.UpdateAsync(booking);

                _logger.LogInformation("Booking confirmed successfully. ID: {BookingId}, Professional: {ProfessionalId}", 
                    bookingId, professionalId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId} by professional {ProfessionalId}", 
                    bookingId, professionalId);
                throw;
            }
        }

        public async Task<bool> CompleteBookingAsync(string bookingId, string userId)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("La reserva no existe");
                }

                // Verificar que el usuario tiene permisos para completar esta reserva
                if (booking.ClientId != userId && booking.ProfessionalId != userId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para completar esta reserva");
                }

                if (booking.Status != BookingStatus.Confirmed)
                {
                    throw new InvalidOperationException("Solo se pueden completar reservas confirmadas");
                }

                booking.Complete();
                await _bookingRepository.UpdateAsync(booking);

                _logger.LogInformation("Booking completed successfully. ID: {BookingId}, CompletedBy: {UserId}", 
                    bookingId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing booking {BookingId} by user {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<bool> HasConflictAsync(string professionalId, DateTime appointmentDate, int duration, string? excludeBookingId = null)
        {
            try
            {
                return await _bookingRepository.HasConflictAsync(professionalId, appointmentDate, duration, excludeBookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking conflicts for professional {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<List<BookingDto>> GetUpcomingBookingsAsync(string professionalId, DateTime fromDate, int limit = 10)
        {
            try
            {
                var bookings = await _bookingRepository.GetUpcomingBookingsAsync(professionalId, fromDate, limit);
                var bookingDtos = new List<BookingDto>();

                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToBookingDtoAsync(booking));
                }

                return bookingDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings for professional {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<long> GetBookingCountByStatusAsync(string userId, string status)
        {
            try
            {
                if (Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                {
                    return await _bookingRepository.GetCountByStatusAsync(bookingStatus);
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking count by status {Status} for user {UserId}", status, userId);
                throw;
            }
        }

        private decimal CalculateTotalAmount(decimal hourlyRate, int durationMinutes)
        {
            var hours = (decimal)durationMinutes / 60;
            return Math.Round(hourlyRate * hours, 2);
        }

        private async Task<BookingDto> MapToBookingDtoAsync(Booking booking)
        {
            var dto = new BookingDto
            {
                Id = booking.Id,
                ClientId = booking.ClientId,
                ProfessionalId = booking.ProfessionalId,
                AppointmentDate = booking.AppointmentDate,
                AppointmentDuration = booking.AppointmentDuration,
                ConsultationType = booking.ConsultationType.ToString(),
                Status = booking.Status.ToString(),
                TotalAmount = booking.TotalAmount,
                SpecialNotes = booking.SpecialNotes,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason,
                CancelledBy = booking.CancelledBy
            };

            // Mapear detalles de la reunión
            if (booking.MeetingDetails != null)
            {
                dto.MeetingDetails = new MeetingDetailsDto
                {
                    VirtualMeetingUrl = booking.MeetingDetails.VirtualMeetingUrl,
                    VirtualMeetingId = booking.MeetingDetails.VirtualMeetingId,
                    VirtualMeetingPassword = booking.MeetingDetails.VirtualMeetingPassword,
                    PhysicalAddress = booking.MeetingDetails.PhysicalAddress,
                    Directions = booking.MeetingDetails.Directions,
                    PhoneNumber = booking.MeetingDetails.PhoneNumber,
                    CountryCode = booking.MeetingDetails.CountryCode
                };
            }

            // Obtener información adicional del cliente y profesional
            try
            {
                var client = await _userRepository.GetByIdAsync(booking.ClientId);
                if (client != null)
                {
                    dto.ClientName = $"{client.FirstName} {client.LastName}";
                }

                var professional = await _professionalRepository.GetByIdAsync(booking.ProfessionalId);
                if (professional != null)
                {
                    dto.ProfessionalName = professional.FullName;
                    dto.ProfessionalSpecialty = professional.Specialties.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting additional information for booking {BookingId}", booking.Id);
                // No fallar si no se puede obtener la información adicional
            }

            return dto;
        }
    }
} 