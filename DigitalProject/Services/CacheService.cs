using DigitalProject.Interface;
using FluentValidation.Internal;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DigitalProject.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (value == null) return default;
            return JsonSerializer.Deserialize<T>(value);

        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            await _cache.RemoveAsync(prefix);
        }

        public  async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
            };
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
        }
    }

   }

