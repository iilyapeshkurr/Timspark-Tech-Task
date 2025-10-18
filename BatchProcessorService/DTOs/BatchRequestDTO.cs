namespace BatchProcessorService.DTOs;

public sealed record BatchRequest
{
    public List<string>? IpAddresses { get; init; }
}
