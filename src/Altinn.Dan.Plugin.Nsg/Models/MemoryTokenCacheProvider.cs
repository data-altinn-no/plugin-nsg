using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Altinn.Dan.Plugin.Nsg.Models
{
    public class MemoryTokenCacheProvider : ITokenCacheProvider
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryTokenCacheProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public Task<(bool success, TokenResponse result)> TryGetToken(string key)
        {
            bool success = _memoryCache.TryGetValue(key, out TokenResponse result);
            return Task.FromResult((success, result));
        }

        public async Task Set(string key, TokenResponse value, TimeSpan timeToLive)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.High,
            };

            cacheEntryOptions.SetAbsoluteExpiration(timeToLive);
            _memoryCache.Set(key, value, cacheEntryOptions);

            await Task.CompletedTask;
        }
    }
}
