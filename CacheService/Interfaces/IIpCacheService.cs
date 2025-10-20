using Shared.Models;

namespace CacheService.Interfaces;

public interface IIpCacheService
{
    IpDetails GetCacheDetailsAsync(string ipAddress);
    void SetCacheDetailsAsync(IpDetails ipDetails);
}
