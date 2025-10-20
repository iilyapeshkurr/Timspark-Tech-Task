using Shared.Models;

namespace BatchProcessorService.DTOs;

public sealed record BatchStatusResponseDTO
{
    public Guid BatchId { get; init; } 
    public string Status { get; init; }
    public int TotalIps { get; init; } 
    public int ProcessedIps { get; init; } 
    public List<IpDetails> Results { get; init; } 
}

