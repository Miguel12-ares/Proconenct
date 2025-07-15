using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;

namespace ProConnect.Infrastructure.Repositores
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly IMongoCollection<PortfolioFile> _collection;

        public PortfolioRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PortfolioFile>("portfolioFiles");
        }

        public async Task AddFileAsync(PortfolioFile file)
        {
            await _collection.InsertOneAsync(file);
        }

        public async Task<List<PortfolioFile>> GetFilesByUserAsync(string userId)
        {
            return await _collection.Find(f => f.UserId == userId).ToListAsync();
        }

        public async Task<PortfolioFile?> GetFileByIdAsync(string userId, string fileId)
        {
            return await _collection.Find(f => f.UserId == userId && f.Id == fileId).FirstOrDefaultAsync();
        }

        public async Task DeleteFileAsync(string userId, string fileId)
        {
            await _collection.DeleteOneAsync(f => f.UserId == userId && f.Id == fileId);
        }

        public async Task<bool> UpdateFileDescriptionAsync(string userId, string fileId, string description)
        {
            var update = Builders<PortfolioFile>.Update.Set(f => f.Description, description);
            var result = await _collection.UpdateOneAsync(f => f.UserId == userId && f.Id == fileId, update);
            return result.ModifiedCount > 0;
        }
    }
} 