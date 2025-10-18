namespace Shared.Models;

public sealed record IpDetails
{
    public string? Ip { get; init; }
    public string? Type { get; init; }
    public string? ContinentCode { get; init; }
    public string? ContinentName { get; init; }
    public string? CountryCode { get; init; }
    public string? CountryName { get; init; }
    public string? RegionCode { get; init; }
    public string? RegionName { get; init; }
    public string? City { get; init; }
    public string? Zip { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public DateTime RetrievedAt { get; init; }
}