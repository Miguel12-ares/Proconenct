using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
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
            return await _collection.Find(x => x.UserId == userId).ToListAsync();
        }

        public async Task<PortfolioFile?> GetFileByIdAsync(string userId, string fileId)
        {
            var objectId = ObjectId.Parse(fileId);
            return await _collection.Find(x => x.UserId == userId && x.Id == objectId).FirstOrDefaultAsync();
        }

        public async Task DeleteFileAsync(string userId, string fileId)
        {
            var objectId = ObjectId.Parse(fileId);
            await _collection.DeleteOneAsync(x => x.UserId == userId && x.Id == objectId);
        }

        public async Task<bool> UpdateFileDescriptionAsync(string userId, string fileId, string description)
        {
            var objectId = ObjectId.Parse(fileId);
            var update = Builders<PortfolioFile>.Update.Set(x => x.Description, description);
            var result = await _collection.UpdateOneAsync(x => x.UserId == userId && x.Id == objectId, update);
            return result.ModifiedCount > 0;
        }
    }
} 