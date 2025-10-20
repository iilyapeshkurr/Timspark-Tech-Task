namespace BatchProcessorService.DTOs;

public sealed record BatchRequestDTO
{
    public List<string>? IpAddresses { get; init; }
}
