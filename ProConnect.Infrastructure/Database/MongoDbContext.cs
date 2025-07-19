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

        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("bookings");

        public IMongoDatabase Database => _database;

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

                var professionalStatusIndexKeys = Builders<ProfessionalProfile>.IndexKeys.Ascending(x => x.Status);
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(professionalStatusIndexKeys));

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
                    .Text(x => x.Location)
                    .Text(x => x.FullName); // Agregar FullName
                var textIndexOptions = new CreateIndexOptions
                {
                    Name = "text_search_index",
                    Weights = new MongoDB.Bson.BsonDocument
                    {
                        { "bio", 3 },
                        { "location", 2 },
                        { "fullName", 4 } // Darle mayor peso al nombre
                    }
                };
                await ProfessionalProfiles.Indexes.CreateOneAsync(new CreateIndexModel<ProfessionalProfile>(textIndexKeys, textIndexOptions));

                // Índices para Bookings
                var clientIdIndexKeys = Builders<Booking>.IndexKeys.Ascending(x => x.ClientId);
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(clientIdIndexKeys));

                var professionalIdIndexKeys = Builders<Booking>.IndexKeys.Ascending(x => x.ProfessionalId);
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(professionalIdIndexKeys));

                var appointmentDateIndexKeys = Builders<Booking>.IndexKeys.Ascending(x => x.AppointmentDate);
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(appointmentDateIndexKeys));

                var bookingStatusIndexKeys = Builders<Booking>.IndexKeys.Ascending(x => x.Status);
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(bookingStatusIndexKeys));

                // Índice compuesto para consultas frecuentes de disponibilidad
                var availabilityIndexKeys = Builders<Booking>.IndexKeys
                    .Ascending(x => x.ProfessionalId)
                    .Ascending(x => x.AppointmentDate)
                    .Ascending(x => x.Status);
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(availabilityIndexKeys));

                // Índice compuesto para consultas de cliente
                var clientBookingsIndexKeys = Builders<Booking>.IndexKeys
                    .Ascending(x => x.ClientId)
                    .Ascending(x => x.AppointmentDate);
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(clientBookingsIndexKeys));

                // TTL Index para limpiar reservas expiradas automáticamente (después de 1 año)
                var ttlIndexKeys = Builders<Booking>.IndexKeys.Ascending(x => x.CreatedAt);
                var ttlIndexOptions = new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(365)
                };
                await Bookings.Indexes.CreateOneAsync(new CreateIndexModel<Booking>(ttlIndexKeys, ttlIndexOptions));
                
                Console.WriteLine("MongoDB indexes created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create MongoDB indexes. MongoDB might not be running. Error: {ex.Message}");
                // No lanzar excepción para que la aplicación pueda continuar
            }
        }

        // Método para verificar conexión
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
