using Shared.Models;

namespace BatchProcessorService.Interfaces;

public interface IIpBatchService
{
    Task<List<IpDetails>> ProcessBatchAsync(IEnumerable<string> ipAddresses);
}