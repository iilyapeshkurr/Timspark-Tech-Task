using Shared.Models;

namespace CacheService.Interfaces;

public interface IIpCacheService
{
    Task<IpDetails> GetDetailsAsync(string ipAddress);
    Task SetDetailsAsync(IpDetails ipDetails);
}
