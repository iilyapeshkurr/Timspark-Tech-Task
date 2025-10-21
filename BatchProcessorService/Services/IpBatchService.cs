using System.Collections.Concurrent;
using BatchProcessorService.Constants;
using BatchProcessorService.DTOs;
using BatchProcessorService.Entities;
using BatchProcessorService.Enums;
using BatchProcessorService.Exceptions;
using BatchProcessorService.Interfaces;
using FluentValidation;
using Shared.Models;

namespace BatchProcessorService.Services;

public sealed class IpBatchService(
        IIpLookupHttpClient _lookupClient,
        ILogger<IpBatchService> _logger,
        IValidator<IEnumerable<string>> _validator) : IIpBatchService
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

        _logger.LogInformation("Batch job {Id} started with {Count} IPs. Running in background.", jobState.BatchId, jobState.TotalIps);

        Task.Run(async () => 
        {
            try
            {
                await RunBatchProcessor(jobState, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background batch processing failed for job {Id}", jobState.BatchId);
                jobState.Status = BatchJobStatus.Failed;
            }
        }, cancellationToken);

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

    private async Task RunBatchProcessor(BatchJobState jobState, CancellationToken cancellationToken)
    {
        try
        {
            var ipList = jobState.RemainingIps;

            var chunks = ipList.Chunk(BatchServiceConstants.BATCH_SIZE);

            foreach (var chunk in chunks)
            {
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
            _logger.LogInformation("Job {Id} completed. Successful lookups: {SuccessCount}.", jobState.BatchId, jobState.Results.Count);
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
            _logger.LogDebug("Cache MISS for IP. Initiating live lookup.");
            
            IpDetails? details = await _lookupClient.GetDetailsAsync(ipAddress);

            if (details == null)
            {
                throw new BadRequestException("IP Lookup Service returned empty or invalid details.");
            }

            return details;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL UNHANDLED ERROR processing IP.");
            return new IpDetails();
        }
    }
}