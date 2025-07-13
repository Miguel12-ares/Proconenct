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

        // Método para crear índices
        public async Task CreateIndexesAsync()
        {
            try
            {
                // Verificar conexión primero
                await _client.ListDatabaseNamesAsync();
                
                var emailIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.Email);
                var emailIndexOptions = new CreateIndexOptions { Unique = true };
                await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions));

                var userTypeIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.UserType);
                await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(userTypeIndexKeys));
                
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
