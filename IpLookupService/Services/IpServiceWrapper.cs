using IpLookupService.Exceptions;
using IpLookupService.Interfaces;
using IpStack.Services;
using Shared.Models;
using Shared.Mappers;

namespace IpLookupService.Services;

public class IpServiceWrapper(ILogger<IpServiceWrapper> _logger, IIpStackService _ipStackService) : IIpServiceWrapper
{
    public async Task<IpDetails> GetIpDetailsAsync(string ipAddress)
    {
        _logger.LogInformation("Received request for IP address: {IpAddress}", ipAddress);

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            _logger.LogWarning("Empty IP address provided");
            throw new ArgumentException("IP address cannot be null or empty");
        }

        if (!System.Net.IPAddress.TryParse(ipAddress, out _))
        {
            _logger.LogWarning("Invalid IP address format: {IpAddress}", ipAddress);
            throw new ArgumentException("Invalid IP address format");
        }

        try
        {
            var ipInfo = await _ipStackService.GetIpAddressDetailsAsync(ipAddress: ipAddress);

            if (ipInfo == null)
            {
                _logger.LogWarning("No information found for IP address: {IpAddress}", ipAddress);
                throw new KeyNotFoundException($"No information found for IP address: {ipAddress}");
            }

            _logger.LogInformation("Successfully retrieved information for IP address: {IpAddress}", ipAddress);
            return ipInfo.ToIpDetails();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching IP details for {IpAddress}", ipAddress);

            throw new IPServiceNotAvailableException(
                "Unable to reach IP lookup service. The service may be temporarily unavailable.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching IP details for {IpAddress}", ipAddress);

            throw new IPServiceNotAvailableException(
                "IP lookup service timeout. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching IP details for {IpAddress}", ipAddress);

            throw new IPServiceNotAvailableException(
                "An unexpected error occurred while retrieving IP information.", ex);
        }   
    }
}