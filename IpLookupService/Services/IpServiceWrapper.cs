using IpLookupService.Exceptions;
using IpLookupService.Interfaces;
using IpStack.Services;
using Shared.Models;
using Shared.Mappers;
using FluentValidation;

namespace IpLookupService.Services;

public sealed class IpServiceWrapper(IHttpClientFactory _httpFactory, ILogger<IpServiceWrapper> _logger, IIpStackService _ipStackService, IValidator<string> _validator) : IIpServiceWrapper
{
    public async Task<IpDetails> GetIpDetailsAsync(string ipAddress)
    {
        _logger.LogInformation("Received request for IP address");

        var result = await _validator.ValidateAsync(ipAddress);

        if (!result.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", result.Errors);
            throw new BadRequestException(result.Errors.ToString());
        }

        try
        {
            var _client = _httpFactory.CreateClient("CacheService");

            var cachedInfo = await _client.GetAsync($"/api/IpCache/{ipAddress}");

            if (cachedInfo.IsSuccessStatusCode)
            {
                _logger.LogInformation("Cache hit for IP address");
                return cachedInfo.Content.ReadFromJsonAsync<IpDetails>().Result!;
            }

            var ipInfo = await _ipStackService.GetIpAddressDetailsAsync(ipAddress: ipAddress);

            if (ipInfo.Ip == null)
            {
                _logger.LogWarning("No information found for IP address");
                throw new BadRequestException("No information found for IP address");
            }

            await _client.PostAsJsonAsync("/api/IpCache", ipInfo.ToIpDetails());

            _logger.LogInformation("Successfully retrieved information for IP address");
            return ipInfo.ToIpDetails();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching IP details");

            throw new IPServiceNotAvailableException(
                "Unable to reach IP lookup service. The service may be temporarily unavailable.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching IP details");

            throw new IPServiceNotAvailableException(
                "An unexpected error occurred while retrieving IP information.", ex);
        }   
    }
}