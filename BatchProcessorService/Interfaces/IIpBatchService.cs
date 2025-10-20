using BatchProcessorService.DTOs;

namespace BatchProcessorService.Interfaces;

public interface IIpBatchService
{
    Task<BatchStartResponseDTO> ProcessBatchAsync(IEnumerable<string> ipAddresses);
    Task<BatchStatusResponseDTO?> GetBatchStatusAsync(Guid batchId);
}