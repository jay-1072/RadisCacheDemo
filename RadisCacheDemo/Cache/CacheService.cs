using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace RadisCacheDemo.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetData<T>(string key)
        {
            var value = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(value)) 
            {
                return JsonConvert.DeserializeObject<T>(value);
            }

            return default;
        }

        public async Task SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            DistributedCacheEntryOptions expiryTime = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2)
            };

            await _cache.SetStringAsync(key, JsonConvert.SerializeObject(value), expiryTime);
        }

        public async Task RemoveData(string key)
        {
            var value = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(value)) 
            { 
               await _cache.RemoveAsync(key);
            }           
        }
    }
}
