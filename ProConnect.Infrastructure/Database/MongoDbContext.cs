using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using ProConnect.Core.Entities;

namespace ProConnect.Infrastructure.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoConnection");
            var databaseName = configuration.GetConnectionString("DatabaseName");

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");

        // Método para crear índices
        public async Task CreateIndexesAsync()
        {
            var emailIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.Email);
            var emailIndexOptions = new CreateIndexOptions { Unique = true };
            await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions));

            var userTypeIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.UserType);
            await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(userTypeIndexKeys));
        }
    }
}
