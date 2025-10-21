using BatchProcessorService.Enums;

namespace BatchProcessorService.DTOs;

public sealed record BatchStartResponseDTO
{
    public Guid BatchId { get; init; }
    public BatchJobStatus Status { get; init; }
}