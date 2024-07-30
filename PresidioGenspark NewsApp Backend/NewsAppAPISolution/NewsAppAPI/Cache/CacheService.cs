using Microsoft.Extensions.Caching.Memory;

namespace NewsAppAPI.Cache
{
    public class CacheService:ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SetAsync(string key, object value, TimeSpan expiration)
        {
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
            return Task.CompletedTask;
        }

        public Task<object> GetAsync(string key)
        {
            _cache.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ContainsAsync(string key)
        {
            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }

        public Task ClearAsync()
        {
            // MemoryCache does not support clearing all entries directly
            // This would need to be managed through cache entries and their expiration.
            // For the purpose of this example, this method does nothing.
            return Task.CompletedTask;
        }
    }
}
