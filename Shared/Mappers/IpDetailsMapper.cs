using IpStack.Models;
using Shared.Models;

namespace Shared.Mappers;

public static class IpDetailsMapper
{
    public static IpDetails ToIpDetails(this IpAddressDetails source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return new IpDetails
        {
            Ip = source.Ip ,
            Type = source.Type ?? string.Empty,
            ContinentCode = source.ContinentCode ?? string.Empty,
            ContinentName = source.ContinentName ?? string.Empty,
            CountryCode = source.CountryCode ?? string.Empty,
            CountryName = source.CountryName ?? string.Empty,
            RegionCode = source.RegionCode ?? string.Empty,
            RegionName = source.RegionName ?? string.Empty,
            City = source.City ?? string.Empty,
            Zip = source.Zip ?? string.Empty,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            RetrievedAt = DateTime.UtcNow,
        };
    }
}