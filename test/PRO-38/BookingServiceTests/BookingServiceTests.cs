using Microsoft.Extensions.Logging;
using Moq;
using ProConnect.Application.DTOs;
using ProConnect.Application.Services;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using Xunit;

namespace ProConnect.Tests
{
    /// <summary>
    /// Pruebas unitarias para el servicio de reservas
    /// </summary>
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IProfessionalProfileRepository> _mockProfessionalRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<BookingService>> _mockLogger;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockProfessionalRepository = new Mock<IProfessionalProfileRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<BookingService>>();

            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _mockProfessionalRepository.Object,
                _mockUserRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateBookingAsync_WithValidData_ShouldCreateBooking()
        {
            // Arrange
            var createDto = new CreateBookingDto
            {
                ProfessionalId = "507f1f77bcf86cd799439011",
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Duration = 60,
                ConsultationType = "Virtual",
                Notes = "Consulta de prueba"
            };

            var clientId = "507f1f77bcf86cd799439012";

            var professional = new ProfessionalProfile
            {
                Id = createDto.ProfessionalId,
                FullName = "Juan Pérez",
                HourlyRate = 50.0m,
                Status = ProfileStatus.Active
            };

            var expectedBooking = new Booking
            {
                Id = "507f1f77bcf86cd799439013",
                ClientId = clientId,
                ProfessionalId = createDto.ProfessionalId,
                AppointmentDate = createDto.AppointmentDate,
                AppointmentDuration = createDto.Duration,
                ConsultationType = ConsultationType.Virtual,
                Status = BookingStatus.Pending,
                TotalAmount = 50.0m,
                SpecialNotes = createDto.Notes
            };

            _mockProfessionalRepository.Setup(x => x.GetByIdAsync(createDto.ProfessionalId))
                .ReturnsAsync(professional);

            _mockBookingRepository.Setup(x => x.HasConflictAsync(
                createDto.ProfessionalId, 
                createDto.AppointmentDate, 
                createDto.Duration, 
                null))
                .ReturnsAsync(false);

            _mockBookingRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>()))
                .ReturnsAsync(expectedBooking);

            // Act
            var result = await _bookingService.CreateBookingAsync(createDto, clientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBooking.Id, result.Id);
            Assert.Equal(clientId, result.ClientId);
            Assert.Equal(createDto.ProfessionalId, result.ProfessionalId);
            Assert.Equal(createDto.AppointmentDate, result.AppointmentDate);
            Assert.Equal(createDto.Duration, result.AppointmentDuration);
            Assert.Equal("Virtual", result.ConsultationType);
            Assert.Equal("Pending", result.Status);
            Assert.Equal(50.0m, result.TotalAmount);
            Assert.Equal(createDto.Notes, result.SpecialNotes);

            _mockBookingRepository.Verify(x => x.CreateAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_WithPastDate_ShouldThrowException()
        {
            // Arrange
            var createDto = new CreateBookingDto
            {
                ProfessionalId = "507f1f77bcf86cd799439011",
                AppointmentDate = DateTime.UtcNow.AddDays(-1), // Fecha en el pasado
                Duration = 60,
                ConsultationType = "Virtual"
            };

            var clientId = "507f1f77bcf86cd799439012";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bookingService.CreateBookingAsync(createDto, clientId));
        }

        [Fact]
        public async Task CreateBookingAsync_WithConflict_ShouldThrowException()
        {
            // Arrange
            var createDto = new CreateBookingDto
            {
                ProfessionalId = "507f1f77bcf86cd799439011",
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Duration = 60,
                ConsultationType = "Virtual"
            };

            var clientId = "507f1f77bcf86cd799439012";

            var professional = new ProfessionalProfile
            {
                Id = createDto.ProfessionalId,
                FullName = "Juan Pérez",
                HourlyRate = 50.0m,
                Status = ProfileStatus.Active
            };

            _mockProfessionalRepository.Setup(x => x.GetByIdAsync(createDto.ProfessionalId))
                .ReturnsAsync(professional);

            _mockBookingRepository.Setup(x => x.HasConflictAsync(
                createDto.ProfessionalId, 
                createDto.AppointmentDate, 
                createDto.Duration, 
                null))
                .ReturnsAsync(true); // Hay conflicto

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bookingService.CreateBookingAsync(createDto, clientId));
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ShouldReturnBooking()
        {
            // Arrange
            var bookingId = "507f1f77bcf86cd799439013";
            var userId = "507f1f77bcf86cd799439012";

            var booking = new Booking
            {
                Id = bookingId,
                ClientId = userId,
                ProfessionalId = "507f1f77bcf86cd799439011",
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                AppointmentDuration = 60,
                ConsultationType = ConsultationType.Virtual,
                Status = BookingStatus.Pending,
                TotalAmount = 50.0m
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(bookingId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingId, result.Id);
            Assert.Equal(userId, result.ClientId);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithUnauthorizedUser_ShouldThrowException()
        {
            // Arrange
            var bookingId = "507f1f77bcf86cd799439013";
            var userId = "507f1f77bcf86cd799439012";
            var unauthorizedUserId = "507f1f77bcf86cd799439014";

            var booking = new Booking
            {
                Id = bookingId,
                ClientId = userId,
                ProfessionalId = "507f1f77bcf86cd799439011",
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                AppointmentDuration = 60,
                ConsultationType = ConsultationType.Virtual,
                Status = BookingStatus.Pending,
                TotalAmount = 50.0m
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _bookingService.GetBookingByIdAsync(bookingId, unauthorizedUserId));
        }

        [Fact]
        public async Task CancelBookingAsync_WithValidBooking_ShouldCancelBooking()
        {
            // Arrange
            var bookingId = "507f1f77bcf86cd799439013";
            var userId = "507f1f77bcf86cd799439012";
            var reason = "Cambio de planes";

            var booking = new Booking
            {
                Id = bookingId,
                ClientId = userId,
                ProfessionalId = "507f1f77bcf86cd799439011",
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                AppointmentDuration = 60,
                ConsultationType = ConsultationType.Virtual,
                Status = BookingStatus.Pending,
                TotalAmount = 50.0m
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);

            _mockBookingRepository.Setup(x => x.UpdateAsync(It.IsAny<Booking>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId, userId, reason);

            // Assert
            Assert.True(result);
            _mockBookingRepository.Verify(x => x.UpdateAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task HasConflictAsync_WithConflict_ShouldReturnTrue()
        {
            // Arrange
            var professionalId = "507f1f77bcf86cd799439011";
            var appointmentDate = DateTime.UtcNow.AddDays(1);
            var duration = 60;

            _mockBookingRepository.Setup(x => x.HasConflictAsync(
                professionalId, 
                appointmentDate, 
                duration, 
                null))
                .ReturnsAsync(true);

            // Act
            var result = await _bookingService.HasConflictAsync(professionalId, appointmentDate, duration);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasConflictAsync_WithoutConflict_ShouldReturnFalse()
        {
            // Arrange
            var professionalId = "507f1f77bcf86cd799439011";
            var appointmentDate = DateTime.UtcNow.AddDays(1);
            var duration = 60;

            _mockBookingRepository.Setup(x => x.HasConflictAsync(
                professionalId, 
                appointmentDate, 
                duration, 
                null))
                .ReturnsAsync(false);

            // Act
            var result = await _bookingService.HasConflictAsync(professionalId, appointmentDate, duration);

            // Assert
            Assert.False(result);
        }
    }
} 