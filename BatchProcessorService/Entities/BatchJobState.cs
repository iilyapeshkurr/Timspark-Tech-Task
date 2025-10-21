using BatchProcessorService.Enums;
using Shared.Models;

namespace BatchProcessorService.Entities;

public sealed class BatchJobState
{
    public Guid BatchId { get; init; } = Guid.NewGuid();
    public BatchJobStatus Status { get; set; } = BatchJobStatus.Pending;
    public int TotalIps { get; init; }
    public int ProcessedIps { get; set; } = 0;
    public List<IpDetails> Results { get; init; } = new List<IpDetails>();
    public List<string> RemainingIps { get; init; } = new List<string>();
}