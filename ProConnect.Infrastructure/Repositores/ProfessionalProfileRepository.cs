using MongoDB.Driver;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Core.Models;
using ProConnect.Infrastructure.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task<PagedResult<ProfessionalProfile>> SearchAdvancedAsync(ProfessionalSearchFilters filters)
        {
            var builder = Builders<ProfessionalProfile>.Filter;
            var filterList = new List<FilterDefinition<ProfessionalProfile>>
            {
                builder.Eq(x => x.Status, ProfileStatus.Active)
            };

            // Filtro de consulta de texto y especialidad
            bool hasQuery = !string.IsNullOrWhiteSpace(filters.Query);
            bool hasSpecialties = filters.Specialties != null && filters.Specialties.Count > 0;

            if (hasQuery && hasSpecialties)
            {
                // Si la especialidad seleccionada es igual al texto de búsqueda, solo buscar por texto
                var query = filters.Query.Trim();
                var specialtiesLower = filters.Specialties.Select(s => s.Trim().ToLower()).ToList();
                if (specialtiesLower.Contains(query.ToLower()))
                {
                    var regex = new MongoDB.Bson.BsonRegularExpression(query, "i");
                    filterList.Add(builder.Or(
                        builder.Regex(x => x.Bio, regex),
                        builder.Regex(x => x.Location, regex),
                        builder.AnyIn(x => x.Specialties, new[] { query })
                    ));
                }
                else
                {
                    // Buscar por texto Y filtrar por especialidad
                    var regex = new MongoDB.Bson.BsonRegularExpression(query, "i");
                    filterList.Add(builder.Or(
                        builder.Regex(x => x.Bio, regex),
                        builder.Regex(x => x.Location, regex),
                        builder.AnyIn(x => x.Specialties, new[] { query })
                    ));
                    filterList.Add(builder.AnyIn(x => x.Specialties, filters.Specialties));
                }
            }
            else if (hasQuery)
            {
                var query = filters.Query.Trim();
                var regex = new MongoDB.Bson.BsonRegularExpression(query, "i");
                filterList.Add(builder.Or(
                    builder.Regex(x => x.Bio, regex),
                    builder.Regex(x => x.Location, regex),
                    builder.AnyIn(x => x.Specialties, new[] { query })
                ));
            }
            else if (hasSpecialties)
            {
                filterList.Add(builder.AnyIn(x => x.Specialties, filters.Specialties));
            }

            // Filtro de ubicación
            if (!string.IsNullOrWhiteSpace(filters.Location))
            {
                var locationRegex = new MongoDB.Bson.BsonRegularExpression(filters.Location.Trim(), "i");
                filterList.Add(builder.Regex(x => x.Location, locationRegex));
            }

            // Filtro de tarifa mínima
            if (filters.MinHourlyRate.HasValue)
            {
                filterList.Add(builder.Gte(x => x.HourlyRate, filters.MinHourlyRate.Value));
            }

            // Filtro de tarifa máxima
            if (filters.MaxHourlyRate.HasValue)
            {
                filterList.Add(builder.Lte(x => x.HourlyRate, filters.MaxHourlyRate.Value));
            }

            // Filtro de calificación mínima
            if (filters.MinRating.HasValue)
            {
                filterList.Add(builder.Gte(x => x.RatingAverage, filters.MinRating.Value));
            }

            // Filtro de años de experiencia mínimos
            if (filters.MinExperienceYears.HasValue)
            {
                filterList.Add(builder.Gte(x => x.ExperienceYears, filters.MinExperienceYears.Value));
            }

            // Filtro de consulta virtual
            if (filters.VirtualConsultation.HasValue && filters.VirtualConsultation.Value)
            {
                // Buscar servicios que contengan "virtual" en el nombre usando BSON puro
                var virtualServiceFilter = new MongoDB.Driver.BsonDocumentFilterDefinition<ProfessionalProfile>(
                    new MongoDB.Bson.BsonDocument("services", 
                        new MongoDB.Bson.BsonDocument("$elemMatch",
                            new MongoDB.Bson.BsonDocument("name", 
                                new MongoDB.Bson.BsonDocument("$regex", "virtual").Add("$options", "i")))));
                filterList.Add(virtualServiceFilter);
            }

            var filter = builder.And(filterList);

            // Ordenamiento
            var sort = Builders<ProfessionalProfile>.Sort.Descending(x => x.RatingAverage); // default
            switch (filters.OrderBy?.ToLower())
            {
                case "price_asc":
                    sort = Builders<ProfessionalProfile>.Sort.Ascending(x => x.HourlyRate);
                    break;
                case "price_desc":
                    sort = Builders<ProfessionalProfile>.Sort.Descending(x => x.HourlyRate);
                    break;
                case "rating":
                    sort = Builders<ProfessionalProfile>.Sort.Descending(x => x.RatingAverage);
                    break;
                case "experience":
                    sort = Builders<ProfessionalProfile>.Sort.Descending(x => x.ExperienceYears);
                    break;
                case "relevance":
                    // Para MongoDB, relevance requiere un índice de texto y $text score, aquí se omite por simplicidad
                    break;
            }

            // Paginación
            int skip = (filters.Page - 1) * filters.PageSize;
            int limit = filters.PageSize;

            var totalCount = await _profiles.CountDocumentsAsync(filter);
            var items = await _profiles.Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();

            return new PagedResult<ProfessionalProfile>
            {
                Items = items,
                TotalCount = (int)totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize
            };
        }
    }
} 