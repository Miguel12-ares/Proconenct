using MongoDB.Driver;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Infrastructure.Database;

namespace ProConnect.Infrastructure.Repositores
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _context = context;
            _users = _context.Users;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(x => x.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByDocumentIdAsync(string documentId)
        {
            return await _users.Find(x => x.DocumentId == documentId).FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _users.InsertOneAsync(user);
            return user.Id;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _users.ReplaceOneAsync(x => x.Id == user.Id, user);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var count = await _users.CountDocumentsAsync(x => x.Email == email.ToLowerInvariant());
            return count > 0;
        }

        public async Task<bool> DocumentIdExistsAsync(string documentId)
        {
            var count = await _users.CountDocumentsAsync(x => x.DocumentId == documentId);
            return count > 0;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<List<User>> GetByUserTypeAsync(UserType userType)
        {
            return await _users.Find(x => x.UserType == userType).ToListAsync();
        }

        public async Task<bool> UpdateProfileFieldsAsync(string userId, string firstName, string lastName, string phone, string bio, string documentId, DocumentType documentType)
        {
            var update = Builders<User>.Update
                .Set(u => u.FirstName, firstName)
                .Set(u => u.LastName, lastName)
                .Set(u => u.PhoneNumber, phone)
                .Set(u => u.DocumentId, documentId)
                .Set(u => u.DocumentType, documentType)
                .Set(u => u.UpdatedAt, DateTime.UtcNow)
                .Set(u => u.Bio, bio);

            var result = await _users.UpdateOneAsync(
                x => x.Id == userId,
                update
            );
            return result.ModifiedCount > 0;
        }
    }
}
