using Shared.Models;

namespace CacheService.Services;

public interface IIpCacheService
{
    Task<IpDetails> GetDetailsAsync(string ipAddress);
    Task SetDetailsAsync(IpDetails ipDetails);
}
