using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using ProConnect.Core.Entities;

namespace ProConnect.Infrastructure.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoConnection");
            var databaseName = configuration.GetConnectionString("DatabaseName");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("MongoConnection string is not configured");
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentException("DatabaseName is not configured");
            }

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");

        public IMongoCollection<ProfessionalProfile> ProfessionalProfiles => _database.GetCollection<ProfessionalProfile>("professionalProfiles");

        // Método para crear índices
        public async Task CreateIndexesAsync()
        {
            try
            {
                // Verificar conexión primero
                await _client.ListDatabaseNamesAsync();
                
                // Índices para Users
                var emailIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.Email);
                var emailIndexOptions = new CreateIndexOptions { Unique = true };
                await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions));

                var userTypeIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.UserType);
                await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(userTypeIndexKeys));

                // Índices para ProfessionalProfiles
                var userIdIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.UserId);
                var userIdIndexOptions = new CreateIndexOptions { Unique = true };
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(userIdIndexKeys, userIdIndexOptions));

                var statusIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.Status);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(statusIndexKeys));

                var specialtiesIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.Specialties);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(specialtiesIndexKeys));

                var locationIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.Location);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(locationIndexKeys));

                var hourlyRateIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.HourlyRate);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(hourlyRateIndexKeys));

                var ratingIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.RatingAverage);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(ratingIndexKeys));

                var experienceIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.ExperienceYears);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(experienceIndexKeys));

                // Índice compuesto para búsquedas avanzadas
                var compoundIndexKeys = Builders<ProfessionalProfile>.IndexKeys
                    .Ascending(x => x.Status)
                    .Ascending(x => x.Specialties)
                    .Ascending(x => x.Location);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(compoundIndexKeys));

                // Índice de texto para búsqueda general
                var textIndexKeys = Builders<ProfessionalProfile>.IndexKeys
                    .Text(x => x.Bio)
                    .Text(x => x.Location);
                var textIndexOptions = new CreateIndexOptions
                {
                    Name = "text_search_index",
                    Weights = new MongoDB.Bson.BsonDocument
                    {
                        { "bio", 3 },
                        { "location", 2 }
                    }
                };
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(textIndexKeys, textIndexOptions));
                
                Console.WriteLine("MongoDB indexes created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create MongoDB indexes. MongoDB might not be running. Error: {ex.Message}");
                // No lanzar excepción para que la aplicación pueda continuar
            }
        }

        // Método para verificar si MongoDB está disponible
        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                await _client.ListDatabaseNamesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
