using System.Collections.Concurrent;
using BatchProcessorService.Jobs;
using BatchProcessorService.Constants;
using BatchProcessorService.DTOs;
using BatchProcessorService.Entities;
using BatchProcessorService.Enums;
using BatchProcessorService.Exceptions;
using BatchProcessorService.Interfaces;
using FluentValidation;
using Quartz;
using Shared.Models;

namespace BatchProcessorService.Services;

public sealed class IpBatchService(
        ILogger<IpBatchService> _logger,
        IValidator<IEnumerable<string>> _validator,
        ISchedulerFactory _schedulerFactory) : IIpBatchService
{
    private static readonly ConcurrentDictionary<Guid, BatchJobState> _jobStatuses = new ConcurrentDictionary<Guid, BatchJobState>();

    public async Task<BatchStartResponseDTO> ProcessBatchAsync(IEnumerable<string> ipAddresses, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(ipAddresses, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Batch request validation failed: {Errors}", validationResult.Errors);
            throw new BadRequestException("Invalid IP addresses in the batch request.");
        }

        var ipList = ipAddresses.ToList();

        var jobState = new BatchJobState
        {
            TotalIps = ipList.Count,
            Status = BatchJobStatus.Running,
            RemainingIps = ipList
        };

        _jobStatuses[jobState.BatchId] = jobState;

        _logger.LogInformation("Batch job {Id} started with {Count} IPs. Scheduling Quartz job.", jobState.BatchId, jobState.TotalIps);

        await ScheduleBatchJobAsync(jobState);

        return new BatchStartResponseDTO { BatchId = jobState.BatchId, Status = jobState.Status };
    }

    public BatchStatusResponseDTO? GetBatchStatus(Guid batchId)
    {
        if (_jobStatuses.TryGetValue(batchId, out var jobState))
        {
            var response = new BatchStatusResponseDTO
            {
                BatchId = jobState.BatchId,
                Status = jobState.Status,
                TotalIps = jobState.TotalIps,
                ProcessedIps = jobState.ProcessedIps,
                Results = jobState.Results
            };
            return response;
        }

        throw new BadRequestException($"Batch job with ID '{batchId}' not found.");
    }

    private async Task ScheduleBatchJobAsync(BatchJobState jobState)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            
            var jobDataMap = new JobDataMap();
            jobDataMap.Put("JobState", jobState);
            
            var jobDetail = JobBuilder.Create<IpBatchProcessingJob>()
                .WithIdentity($"batch-job-{jobState.BatchId}", "batch-processing")
                .SetJobData(jobDataMap)
                .Build();
            
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"batch-trigger-{jobState.BatchId}", "batch-processing")
                .StartNow()
                .Build();
            
            await scheduler.ScheduleJob(jobDetail, trigger);
            
            _logger.LogInformation("Successfully scheduled Quartz job for batch {BatchId}", jobState.BatchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule Quartz job for batch {BatchId}", jobState.BatchId);
            jobState.Status = BatchJobStatus.Failed;
        }
    }
}