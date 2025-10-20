using BatchProcessorService.Interfaces;
using Shared.Models;

namespace BatchProcessorService.Services;

public class IpBatchService(
        IIpCacheHttpClient _cacheClient,
        IIpLookupHttpClient _lookupClient,
        ILogger<IpBatchService> _logger) : IIpBatchService
{
    private const int BatchSize = 10;

    public async Task<List<IpDetails>> ProcessBatchAsync(IEnumerable<string> ipAddresses)
    {
        var ipList = ipAddresses.ToList();
        _logger.LogInformation("Starting batch processing for {Count} IP addresses in chunks of {Size}.", ipList.Count, BatchSize);

        var allResults = new List<IpDetails>();
        int processedCount = 0;

        while (processedCount < ipList.Count)
        {
            var chunk = ipList.Skip(processedCount).Take(BatchSize).ToList();
            _logger.LogInformation("Processing chunk starting at index {Start} with {ChunkSize} IPs.", processedCount, chunk.Count);

            var processingTasks = chunk.Select(ip => ProcessSingleIpAsync(ip)).ToList();
            var results = await Task.WhenAll(processingTasks);

            allResults.AddRange(results.Where(r => r != null)!);

            processedCount += chunk.Count;
        }

        _logger.LogInformation("Batch processing complete. Successfully processed {SuccessCount} details.", allResults.Count);

        return allResults;
    }

    private async Task<IpDetails?> ProcessSingleIpAsync(string ipAddress)
    {
        _logger.LogInformation("Processing single IP: {Ip} via live lookup and caching.", ipAddress);

        try
        {

            var details = await _cacheClient.GetDetailsAsync(ipAddress);

            if (details != null)
            {
                _logger.LogInformation("IP: {Ip} found in cache. Returning cached details.", ipAddress);
                return details;
            }

            _logger.LogInformation("IP: {Ip} not in cache. Initiating live lookup.", ipAddress);
        
            details = await _lookupClient.GetDetailsAsync(ipAddress);

            try
            {
                await _cacheClient.SetDetailsAsync(details);
                _logger.LogInformation("IP: {Ip} successfully cached after live lookup.", ipAddress);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Non-critical: Failed to cache details for IP: {Ip}.", ipAddress);
            }

            return details;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "FATAL ERROR during processing IP: {Ip}. Skipping this IP.", ipAddress);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL UNHANDLED ERROR processing IP: {Ip}.", ipAddress);
            return null;
        }
    }
}