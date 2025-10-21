using BatchProcessorService.DTOs;

namespace BatchProcessorService.Interfaces;

public interface IIpBatchService
{
    Task<BatchStartResponseDTO> ProcessBatchAsync(IEnumerable<string> ipAddresses, CancellationToken cancellationToken);
    BatchStatusResponseDTO? GetBatchStatus(Guid batchId);
}