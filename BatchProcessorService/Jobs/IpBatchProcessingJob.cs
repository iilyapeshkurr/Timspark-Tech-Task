using BatchProcessorService.Entities;
using BatchProcessorService.Enums;
using BatchProcessorService.Interfaces;
using Quartz;
using Shared.Models;

namespace BatchProcessorService.Jobs;

public class IpBatchProcessingJob : IJob
{
    private readonly IIpLookupHttpClient _lookupClient;
    private readonly ILogger<IpBatchProcessingJob> _logger;

    public IpBatchProcessingJob(IIpLookupHttpClient lookupClient, ILogger<IpBatchProcessingJob> logger)
    {
        _lookupClient = lookupClient;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobState = context.JobDetail.JobDataMap.Get("JobState") as BatchJobState;
        var cancellationToken = context.CancellationToken;

        if (jobState == null)
        {
            _logger.LogError("Job state not found in job data map");
            return;
        }

        try
        {
            _logger.LogInformation("Starting Quartz job processing for batch {BatchId}", jobState.BatchId);

            var ipList = jobState.RemainingIps;
            var chunks = ipList.Chunk(BatchProcessorService.Constants.BatchServiceConstants.BATCH_SIZE);

            foreach (var chunk in chunks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Job {BatchId} was cancelled", jobState.BatchId);
                    jobState.Status = BatchJobStatus.Failed;
                    return;
                }
                
                _logger.LogInformation(
                    "Job {Id}: Processing chunk with {ChunkSize} IPs.",
                    jobState.BatchId,
                    chunk.Length
                );

                var processingTasks = chunk.Select(ip => ProcessSingleIpAsync(ip, cancellationToken));
                var results = await Task.WhenAll(processingTasks);
                
                var successfulResults = results.Where(r => r != null);
                jobState.Results.AddRange(successfulResults!);
                jobState.ProcessedIps += chunk.Length;
            }
            
            jobState.Status = BatchJobStatus.Completed;
            _logger.LogInformation("Job {Id} completed successfully. Successful lookups: {SuccessCount}.", 
                jobState.BatchId, jobState.Results.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job {BatchId} was cancelled", jobState.BatchId);
            jobState.Status = BatchJobStatus.Failed;
        }
        catch (Exception ex)
        {
            jobState.Status = BatchJobStatus.Failed;
            _logger.LogError(ex, "Job {Id} failed during execution.", jobState.BatchId);
        }
    }

    private async Task<IpDetails?> ProcessSingleIpAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing IP address: {IpAddress}", ipAddress);
            
            var details = await _lookupClient.GetDetailsAsync(ipAddress);

            if (details == null)
            {
                _logger.LogWarning("IP Lookup Service returned null for IP: {IpAddress}", ipAddress);
                return null;
            }

            _logger.LogDebug("Successfully processed IP: {IpAddress}", ipAddress);
            return details;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process IP address: {IpAddress}", ipAddress);
            return null;
        }
    }
}
