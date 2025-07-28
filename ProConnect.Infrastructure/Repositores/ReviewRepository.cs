using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ProConnect.Core.Entities;

using ProConnect.Application.Interfaces;

namespace ProConnect.Infrastructure.Repositores
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewRepository(IMongoDatabase database)
        {
            _reviews = database.GetCollection<Review>("Reviews");
        }

        public async Task<List<Review>> GetByProfessionalIdAsync(string professionalId)
        {
            return await _reviews.Find(r => r.ProfessionalId == professionalId).ToListAsync();
        }

        public async Task<List<Review>> GetByClientIdAsync(string clientId)
        {
            return await _reviews.Find(r => r.ClientId == clientId).ToListAsync();
        }

        public async Task<Review> GetByIdAsync(string id)
        {
            return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Review review)
        {
            await _reviews.InsertOneAsync(review);
        }

        public async Task UpdateAsync(string id, Review updatedReview)
        {
            await _reviews.ReplaceOneAsync(r => r.Id == id, updatedReview);
        }

        public async Task DeleteAsync(string id)
        {
            await _reviews.DeleteOneAsync(r => r.Id == id);
        }
    }
}
