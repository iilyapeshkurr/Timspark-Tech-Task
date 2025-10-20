using Shared.Models;
using CacheService.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using CacheService.Options;

namespace CacheService.Services;

public sealed class IpCacheService(IMemoryCache _cache, ILogger<IpCacheService> _logger, IOptionsMonitor<CacheOptions> _cacheOptions) : IIpCacheService
{
    public IpDetails? GetCacheDetailsAsync(string ipAddress)
    {
        _logger.LogInformation("Attempting to retrieve details for IP: {Ip}", ipAddress);

        try
        {
            if (_cache.TryGetValue(ipAddress, out IpDetails? details))
            {
                _logger.LogInformation("Cache HIT for IP: {Ip}", ipAddress);
                return details;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Failed to retrieve data from cache for IP: {Ip}.", ipAddress);
            throw new Exception($"Cache retrieval failed for IP: {ipAddress}.", ex);
        }

        _logger.LogInformation("Cache MISS for IP: {Ip}", ipAddress);
        return new IpDetails();
    }

    public void SetCacheDetailsAsync(IpDetails details)
    {
        if (details.Ip == null)
        {
            _logger.LogInformation("IP address is null, cannot cache.");
            throw new ArgumentException("IP Address is required for caching key.", nameof(details.Ip));
        }

        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheOptions.CurrentValue.CacheExpiration));

            _cache.Set(details.Ip, details, cacheEntryOptions);

            _logger.LogInformation("Successfully cached details for IP: {Ip}", details.Ip);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Failed to set data in cache for IP: {Ip}.", details.Ip);
            throw new Exception($"Cache storage failed for IP: {details.Ip}.", ex);
        }

    }
}
