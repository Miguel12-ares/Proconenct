using MongoDB.Driver;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Core.Models;
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

        /// <summary>
        /// Obtiene perfiles activos con filtros opcionales.
        /// </summary>
        public async Task<List<ProfessionalProfile>> GetActiveProfilesAsync(
            string? specialty = null,
            string? location = null,
            decimal? minHourlyRate = null,
            decimal? maxHourlyRate = null,
            int? minExperienceYears = null,
            double? minRating = null)
        {
            var filter = Builders<ProfessionalProfile>.Filter.Eq(x => x.Status, ProfileStatus.Active);

            if (!string.IsNullOrEmpty(specialty))
            {
                filter &= Builders<ProfessionalProfile>.Filter.AnyEq(x => x.Specialties, specialty);
            }

            if (!string.IsNullOrEmpty(location))
            {
                filter &= Builders<ProfessionalProfile>.Filter.Regex(x => x.Location, 
                    new MongoDB.Bson.BsonRegularExpression(location, "i"));
            }

            if (minHourlyRate.HasValue)
            {
                filter &= Builders<ProfessionalProfile>.Filter.Gte(x => x.HourlyRate, minHourlyRate.Value);
            }

            if (maxHourlyRate.HasValue)
            {
                filter &= Builders<ProfessionalProfile>.Filter.Lte(x => x.HourlyRate, maxHourlyRate.Value);
            }

            if (minExperienceYears.HasValue)
            {
                filter &= Builders<ProfessionalProfile>.Filter.Gte(x => x.ExperienceYears, minExperienceYears.Value);
            }

            if (minRating.HasValue)
            {
                filter &= Builders<ProfessionalProfile>.Filter.Gte(x => x.RatingAverage, minRating.Value);
            }

            return await _profiles.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Busca perfiles por texto en bio, especialidades y ubicación.
        /// </summary>
        public async Task<List<ProfessionalProfile>> SearchProfilesAsync(string searchTerm)
        {
            var filter = Builders<ProfessionalProfile>.Filter.And(
                Builders<ProfessionalProfile>.Filter.Eq(x => x.Status, ProfileStatus.Active),
                Builders<ProfessionalProfile>.Filter.Or(
                    Builders<ProfessionalProfile>.Filter.Regex(x => x.Bio, 
                        new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<ProfessionalProfile>.Filter.AnyEq(x => x.Specialties, searchTerm),
                    Builders<ProfessionalProfile>.Filter.Regex(x => x.Location, 
                        new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );

            return await _profiles.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Obtiene perfiles ordenados por calificación.
        /// </summary>
        public async Task<List<ProfessionalProfile>> GetTopRatedProfilesAsync(int limit = 10)
        {
            var filter = Builders<ProfessionalProfile>.Filter.Eq(x => x.Status, ProfileStatus.Active);
            var sort = Builders<ProfessionalProfile>.Sort.Descending(x => x.RatingAverage);

            return await _profiles.Find(filter).Sort(sort).Limit(limit).ToListAsync();
        }

        /// <summary>
        /// Obtiene perfiles por ubicación.
        /// </summary>
        public async Task<List<ProfessionalProfile>> GetProfilesByLocationAsync(string location)
        {
            var filter = Builders<ProfessionalProfile>.Filter.And(
                Builders<ProfessionalProfile>.Filter.Eq(x => x.Status, ProfileStatus.Active),
                Builders<ProfessionalProfile>.Filter.Regex(x => x.Location, 
                    new MongoDB.Bson.BsonRegularExpression(location, "i"))
            );

            return await _profiles.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Verifica si un usuario ya tiene un perfil profesional.
        /// </summary>
        public async Task<bool> UserHasProfileAsync(string userId)
        {
            var count = await _profiles.CountDocumentsAsync(x => x.UserId == userId);
            return count > 0;
        }

        /// <summary>
        /// Obtiene estadísticas de perfiles profesionales.
        /// </summary>
        public async Task<ProfileStatistics> GetProfileStatisticsAsync()
        {
            var totalProfiles = await _profiles.CountDocumentsAsync(_ => true);
            var activeProfiles = await _profiles.CountDocumentsAsync(x => x.Status == ProfileStatus.Active);
            var draftProfiles = await _profiles.CountDocumentsAsync(x => x.Status == ProfileStatus.Draft);

            var avgRating = await _profiles.Aggregate()
                .Match(x => x.Status == ProfileStatus.Active && x.RatingAverage > 0)
                .Group(x => 1, g => new { AverageRating = g.Average(x => x.RatingAverage) })
                .FirstOrDefaultAsync();

            var avgHourlyRate = await _profiles.Aggregate()
                .Match(x => x.Status == ProfileStatus.Active)
                .Group(x => 1, g => new { AverageRate = g.Average(x => x.HourlyRate) })
                .FirstOrDefaultAsync();

            return new ProfileStatistics
            {
                TotalProfiles = (int)totalProfiles,
                ActiveProfiles = (int)activeProfiles,
                DraftProfiles = (int)draftProfiles,
                AverageRating = avgRating?.AverageRating ?? 0,
                AverageHourlyRate = avgHourlyRate?.AverageRate ?? 0
            };
        }

        public async Task<bool> AddServiceAsync(string userId, Service service)
        {
            var update = Builders<ProfessionalProfile>.Update.Push(x => x.Services, service);
            var result = await _profiles.UpdateOneAsync(x => x.UserId == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateServiceAsync(string userId, Service service)
        {
            var filter = Builders<ProfessionalProfile>.Filter.And(
                Builders<ProfessionalProfile>.Filter.Eq(x => x.UserId, userId),
                Builders<ProfessionalProfile>.Filter.ElemMatch(x => x.Services, s => s.Id == service.Id)
            );
            var update = Builders<ProfessionalProfile>.Update
                .Set(x => x.Services[-1].Name, service.Name)
                .Set(x => x.Services[-1].Description, service.Description)
                .Set(x => x.Services[-1].Type, service.Type)
                .Set(x => x.Services[-1].Price, service.Price)
                .Set(x => x.Services[-1].EstimatedDurationMinutes, service.EstimatedDurationMinutes)
                .Set(x => x.Services[-1].IsActive, service.IsActive);
            var result = await _profiles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteServiceAsync(string userId, string serviceId)
        {
            var update = Builders<ProfessionalProfile>.Update.PullFilter(x => x.Services, s => s.Id == serviceId);
            var result = await _profiles.UpdateOneAsync(x => x.UserId == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Service>> GetServicesAsync(string userId)
        {
            var profile = await _profiles.Find(x => x.UserId == userId).FirstOrDefaultAsync();
            return profile?.Services ?? new List<Service>();
        }
    }
} 