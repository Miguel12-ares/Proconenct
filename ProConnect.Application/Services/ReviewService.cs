using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;


namespace ProConnect.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;

        public ReviewService(IReviewRepository repository)
        {
            _repository = repository;
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

        public async Task<List<ReviewDto>> GetByClientIdAsync(string clientId)
        {
            var reviews = await _repository.GetByClientIdAsync(clientId);
            return reviews.ConvertAll(ToDto);
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
        {
            var review = new Review
            {
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
