using Shared.Models;

namespace BatchProcessorService.Interfaces;

public interface IIpCacheHttpClient
{
    Task<IpDetails?> GetDetailsAsync(string ipAddress);
    Task SetDetailsAsync(IpDetails details);
}