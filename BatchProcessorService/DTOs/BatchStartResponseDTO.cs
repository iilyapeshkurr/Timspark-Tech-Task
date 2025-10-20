namespace BatchProcessorService.DTOs;

public sealed record BatchStartResponseDTO
{
    public Guid BatchId { get; init; }
    public string Status { get; init; }
}