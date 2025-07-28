using System.Collections.Generic;
using System.Threading.Tasks;
using ProConnect.Core.Entities;

namespace ProConnect.Application.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByProfessionalIdAsync(string professionalId);
        Task<List<Review>> GetByClientIdAsync(string clientId);
        Task<Review> GetByIdAsync(string id);
        Task AddAsync(Review review);
        Task UpdateAsync(string id, Review updatedReview);
        Task DeleteAsync(string id);
    }
}
