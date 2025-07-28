using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;


namespace ProConnect.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;
        private readonly IBookingService _bookingService;

        public ReviewService(IReviewRepository repository, IBookingService bookingService)
        {
            _repository = repository;
            _bookingService = bookingService;
        }

        public async Task<ReviewDto> GetByIdAsync(string id)
        {
            var review = await _repository.GetByIdAsync(id);
            return review == null ? null : ToDto(review);
        }

        public async Task<List<ReviewDto>> GetByProfessionalIdAsync(string professionalId)
        {
            var reviews = await _repository.GetByProfessionalIdAsync(professionalId);
            return reviews.ConvertAll(ToDto);
        }

        public async Task<PagedResultDto<ReviewDto>> GetByProfessionalIdPagedAsync(string professionalId, int page, int pageSize)
        {
            var totalCount = await _repository.GetCountByProfessionalIdAsync(professionalId);
            var offset = (page - 1) * pageSize;
            var reviews = await _repository.GetByProfessionalIdPagedAsync(professionalId, pageSize, offset);
            var items = reviews.ConvertAll(ToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            return new PagedResultDto<ReviewDto>
            {
                Items = items,
    TotalCount = totalCount,
    PageSize = pageSize,
    CurrentPage = page,
    TotalPages = totalPages
            };
        }

        public async Task<List<ReviewDto>> GetByClientIdAsync(string clientId)
        {
            var reviews = await _repository.GetByClientIdAsync(clientId);
            return reviews.ConvertAll(ToDto);
        }

        public async Task<PagedResultDto<ReviewDto>> GetByClientIdPagedAsync(string clientId, int page, int pageSize)
        {
            var totalCount = await _repository.GetCountByClientIdAsync(clientId);
            var offset = (page - 1) * pageSize;
            var reviews = await _repository.GetByClientIdPagedAsync(clientId, pageSize, offset);
            var items = reviews.ConvertAll(ToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            return new PagedResultDto<ReviewDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
        {
            // 1. Validar unicidad de review por booking
            var existingReview = await _repository.GetByClientProfessionalBookingAsync(dto.ClientId, dto.ProfessionalId, dto.BookingId);
            if (existingReview != null)
                throw new InvalidOperationException("Ya existe una reseña para esta reserva entre este cliente y profesional.");

            // 2. Validar que la reserva exista y esté completada
            var booking = await _bookingService.GetBookingByIdAsync(dto.BookingId, dto.ClientId);
            if (booking == null)
                throw new InvalidOperationException("No se encontró la reserva especificada para este cliente.");
            if (!string.Equals(booking.ProfessionalId, dto.ProfessionalId, StringComparison.Ordinal))
                throw new InvalidOperationException("La reserva no corresponde al profesional indicado.");
            if (!string.Equals(booking.ClientId, dto.ClientId, StringComparison.Ordinal))
                throw new InvalidOperationException("La reserva no corresponde al cliente autenticado.");
            if (!string.Equals(booking.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Solo se pueden dejar reseñas para reservas completadas.");

            // 3. Crear la reseña
            var review = new Review
            {
                Id = Guid.NewGuid().ToString(), // Asignar un Id único requerido
                ClientId = dto.ClientId,
                ProfessionalId = dto.ProfessionalId,
                BookingId = dto.BookingId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(review);
            return ToDto(review);
        }

        public async Task<bool> UpdateAsync(string id, UpdateReviewDto dto)
        {
            var review = await _repository.GetByIdAsync(id);
            if (review == null) return false;
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(id, review);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var review = await _repository.GetByIdAsync(id);
            if (review == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }

        private ReviewDto ToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ClientId = review.ClientId,
                ProfessionalId = review.ProfessionalId,
                BookingId = review.BookingId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}
