using ProConnect.Core.Interfaces;

namespace ProConnect.Infrastructure.Services
{
    public class NullCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult<T?>(default);
        }
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            return Task.CompletedTask;
        }
    }
} 