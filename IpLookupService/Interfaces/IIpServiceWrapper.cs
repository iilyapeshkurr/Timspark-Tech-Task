using Shared.Models;

namespace IpLookupService.Interfaces;

public interface IIpServiceWrapper
{
    Task<IpDetails> GetIpDetailsAsync(string ipAddress);
}