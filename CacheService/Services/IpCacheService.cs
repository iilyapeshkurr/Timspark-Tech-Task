namespace IpProject.IpCacheService.Services
{
    using Shared.Models;
    using CacheService.Services;
    using Microsoft.Extensions.Caching.Memory;

    public class IpCacheService(IMemoryCache _cache) : IIpCacheService
    {
        private readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
        public Task<IpDetails?> GetDetailsAsync(string ipAddress)
        {
            if (_cache.TryGetValue(ipAddress, out IpDetails? details))
            {
                return Task.FromResult(details);
            }

            return Task.FromResult<IpDetails?>(null);
        }

        public Task SetDetailsAsync(IpDetails details)
        {
            if (details.Ip == null)
            {
                throw new ArgumentException("IP Address is required for caching key.", nameof(details.Ip));
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheExpiration);

            _cache.Set(details.Ip, details, cacheEntryOptions);

            return Task.CompletedTask;
        }
    }
}
