using MongoDB.Driver;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Infrastructure.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProConnect.Infrastructure.Repositores
{
    /// <summary>
    /// Implementación del repositorio de perfiles profesionales.
    /// </summary>
    public class ProfessionalProfileRepository : IProfessionalProfileRepository
    {
        private readonly IMongoCollection<ProfessionalProfile> _profiles;

        public ProfessionalProfileRepository(MongoDbContext context)
        {
            _profiles = context.ProfessionalProfiles;
        }

        public async Task<ProfessionalProfile?> GetByIdAsync(string id)
        {
            return await _profiles.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ProfessionalProfile?> GetByUserIdAsync(string userId)
        {
            return await _profiles.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<string> CreateAsync(ProfessionalProfile profile)
        {
            await _profiles.InsertOneAsync(profile);
            return profile.Id;
        }

        public async Task<bool> UpdateAsync(ProfessionalProfile profile)
        {
            var result = await _profiles.ReplaceOneAsync(x => x.Id == profile.Id, profile);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _profiles.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<List<ProfessionalProfile>> GetAllAsync()
        {
            return await _profiles.Find(_ => true).ToListAsync();
        }

        public async Task<List<ProfessionalProfile>> GetBySpecialtyAsync(string specialty)
        {
            return await _profiles.Find(x => x.Specialties.Contains(specialty)).ToListAsync();
        }
    }
} 