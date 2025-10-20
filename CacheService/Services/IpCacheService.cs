using Shared.Models;
using CacheService.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CacheService.Services;

public sealed class IpCacheService(IMemoryCache _cache, ILogger<IpCacheService> _logger) : IIpCacheService
{
    private readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);
    public Task<IpDetails?> GetDetailsAsync(string ipAddress)
    {
        _logger.LogInformation("Attempting to retrieve details for IP: {Ip}", ipAddress);

        try
        {
            if (_cache.TryGetValue(ipAddress, out IpDetails? details))
            {
                _logger.LogInformation("Cache HIT for IP: {Ip}", ipAddress);
                return Task.FromResult(details);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FATAL: Failed to retrieve data from cache for IP: {Ip}.", ipAddress);
            throw new InvalidOperationException($"Cache retrieval failed for IP: {ipAddress}.", ex);
        }

        _logger.LogInformation("Cache MISS for IP: {Ip}", ipAddress);
        return Task.FromResult<IpDetails?>(null);
    }

    public Task SetDetailsAsync(IpDetails details)
    {
        if (details.Ip == null)
        {
            _logger.LogInformation("IP address is null, cannot cache.");
            throw new ArgumentException("IP Address is required for caching key.", nameof(details.Ip));
        }

        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheExpiration);

            _cache.Set(details.Ip, details, cacheEntryOptions);

            _logger.LogInformation("Successfully cached details for IP: {Ip}", details.Ip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FATAL: Failed to set data in cache for IP: {Ip}.", details.Ip);
            throw new InvalidOperationException($"Cache storage failed for IP: {details.Ip}.", ex);
        }

        return Task.CompletedTask;
    }
}
