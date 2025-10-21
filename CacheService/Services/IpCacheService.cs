using Shared.Models;
using CacheService.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using CacheService.Options;
using CacheService.Exceptions;
using FluentValidation;

namespace CacheService.Services;

public sealed class IpCacheService(IMemoryCache _cache, ILogger<IpCacheService> _logger, IOptionsMonitor<CacheOptions> _cacheOptions, IValidator<IpDetails> _ipDetailsValidator, IValidator<string> _ipAdressValidator) : IIpCacheService
{
    public IpDetails? GetCacheDetailsAsync(string ipAddress)
    {
        _logger.LogInformation("Attempting to retrieve details for IP");

        var result = _ipAdressValidator.Validate(ipAddress);

        if (!result.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", result.Errors);
            throw new BadRequestException(result.Errors.ToString());
        }

        try
        {
            if (_cache.TryGetValue(ipAddress, out IpDetails? details))
            {
                _logger.LogInformation("Cache HIT for IP");
                return details;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Failed to retrieve data from cache for IP.");
            throw new Exception($"Cache retrieval failed for IP.", ex);
        }

        _logger.LogInformation("Cache MISS for IP.");
        return null;
    }

    public void SetCacheDetailsAsync(IpDetails details)
    {
        var validationResult = _ipDetailsValidator.Validate(details);

        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First().ErrorMessage;
            _logger.LogWarning("Validation failed for IP details: {Error}", firstError);
            throw new BadRequestException(firstError);
        }

        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheOptions.CurrentValue.CacheExpiration));

            _cache.Set(details.Ip, details, cacheEntryOptions);

            _logger.LogInformation("Successfully cached details for IP.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Failed to set data in cache for IP.");
            throw new Exception("Cache storage failed for IP.", ex);
        }

    }
}
