using System.Collections.Concurrent;
using BatchProcessorService.DTOs;
using BatchProcessorService.Interfaces;
using Shared.Models;

namespace BatchProcessorService.Services;

public class IpBatchService(
        IIpCacheHttpClient _cacheClient,
        IIpLookupHttpClient _lookupClient,
        ILogger<IpBatchService> _logger) : IIpBatchService
{
    private class BatchJobState
    {
        public Guid BatchId { get; init; } = Guid.NewGuid();
        public string Status { get; set; } = "Pending";
        public int TotalIps { get; init; }
        public int ProcessedIps { get; set; } = 0;
        public List<IpDetails> Results { get; init; } = new List<IpDetails>();
        public List<string> RemainingIps { get; init; } = new List<string>();
    }

    private static readonly ConcurrentDictionary<Guid, BatchJobState> _jobStatuses = new ConcurrentDictionary<Guid, BatchJobState>();
    private const int BatchSize = 10;

    public Task<BatchStartResponseDTO> ProcessBatchAsync(IEnumerable<string> ipAddresses)
    {
        var ipList = ipAddresses.ToList();

        if (!ipList.Any())
        {
            return Task.FromResult(new BatchStartResponseDTO { BatchId = Guid.Empty, Status = "Completed" });
        }

        var jobState = new BatchJobState
        {
            TotalIps = ipList.Count,
            Status = "Running",
            RemainingIps = ipList
        };

        _jobStatuses[jobState.BatchId] = jobState;

        _logger.LogInformation("Batch job {Id} started with {Count} IPs. Running in background.", jobState.BatchId, jobState.TotalIps);

        _ = Task.Run(() => RunBatchProcessor(jobState));

        return Task.FromResult(new BatchStartResponseDTO { BatchId = jobState.BatchId, Status = jobState.Status });
    }

    public Task<BatchStatusResponseDTO?> GetBatchStatusAsync(Guid batchId)
    {
        if (_jobStatuses.TryGetValue(batchId, out var jobState))
        {
            var response = new BatchStatusResponseDTO{
                BatchId =jobState.BatchId,
                Status = jobState.Status,
                TotalIps = jobState.TotalIps,
                ProcessedIps = jobState.ProcessedIps,
                Results = jobState.Results
            };
            return Task.FromResult<BatchStatusResponseDTO?>(response);
        }

        return Task.FromResult<BatchStatusResponseDTO?>(null);
    }

    private async Task RunBatchProcessor(BatchJobState jobState)
    {
        try
        {
            var ipList = jobState.RemainingIps;
            int processedCount = 0;
            
            while (processedCount < ipList.Count)
            {
                var chunk = ipList.Skip(processedCount).Take(BatchSize).ToList();
                _logger.LogInformation("Job {Id}: Processing chunk starting at index {Start} with {ChunkSize} IPs.", jobState.BatchId, processedCount, chunk.Count);

                var processingTasks = chunk.Select(ip => ProcessSingleIpAsync(ip)).ToList();
                var results = await Task.WhenAll(processingTasks);
                
                lock (jobState) 
                {
                    var successfulResults = results.Where(r => r != null).ToList();
                    jobState.Results.AddRange(successfulResults!);
                    jobState.ProcessedIps += chunk.Count; 
                }
                
                processedCount += chunk.Count;
            }

            jobState.Status = "Completed";
            _logger.LogInformation("Job {Id} completed. Successful lookups: {SuccessCount}.", jobState.BatchId, jobState.Results.Count);
        }
        catch (Exception ex)
        {
            jobState.Status = "Failed";
            _logger.LogError(ex, "Job {Id} failed during execution.", jobState.BatchId);
        }
    }

    private async Task<IpDetails?> ProcessSingleIpAsync(string ipAddress)
    {
        try
        {
            IpDetails? details = await _cacheClient.GetDetailsAsync(ipAddress);

            if (details != null)
            {
                _logger.LogDebug("Cache HIT for IP: {Ip}.", ipAddress);
                return details;
            }

            _logger.LogDebug("Cache MISS for IP: {Ip}. Initiating live lookup.", ipAddress);
            
            details = await _lookupClient.GetDetailsAsync(ipAddress);
            
            if (details == null) 
            {
                throw new InvalidOperationException($"IP Lookup Service returned empty or invalid details for {ipAddress}.");
            }

            try
            {
                await _cacheClient.SetDetailsAsync(details);
                _logger.LogDebug("IP: {Ip} successfully cached after live lookup.", ipAddress);
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