using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Core.Entities;

namespace ProConnect.Application.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByProfessionalIdAsync(string professionalId);
        Task<List<Review>> GetByProfessionalIdPagedAsync(string professionalId, int limit, int offset);
        Task<long> GetCountByProfessionalIdAsync(string professionalId);
        Task<List<Review>> GetByClientIdAsync(string clientId);
        Task<List<Review>> GetByClientIdPagedAsync(string clientId, int limit, int offset);
        Task<long> GetCountByClientIdAsync(string clientId);
        Task<Review> GetByIdAsync(string id);
        Task<Review> GetByClientProfessionalBookingAsync(string clientId, string professionalId, string bookingId);
        Task AddAsync(Review review);
        Task UpdateAsync(string id, Review updatedReview);
        Task DeleteAsync(string id);
    }
}
