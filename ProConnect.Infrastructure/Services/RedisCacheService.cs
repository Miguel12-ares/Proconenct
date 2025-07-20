using ProConnect.Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace ProConnect.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly bool _isAvailable;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            try
            {
                // Probar conexión
                _isAvailable = _redis.GetDatabase().Ping() != TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisCacheService] No se pudo conectar a Redis: {ex.Message}");
                _isAvailable = false;
            }
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            if (!_isAvailable) return default;
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);
                if (!value.HasValue) return default;
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisCacheService] Error al obtener cache: {ex.Message}");
                return default;
            }
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (!_isAvailable) return;
            try
            {
                var db = _redis.GetDatabase();
                var json = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, json, expiry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisCacheService] Error al guardar cache: {ex.Message}");
            }
        }
    }
} 