using Shared.Models;

namespace BatchProcessorService.Interfaces;

public interface IIpLookupHttpClient
{
    Task<IpDetails> GetDetailsAsync(string ipAddress);
}