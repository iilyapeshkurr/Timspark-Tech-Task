using System.Text.Json;
using BatchProcessorService.Constants;
using BatchProcessorService.Exceptions;
using BatchProcessorService.Interfaces;
using Shared.Models;

namespace BatchProcessorService.HttpClientWrappers;

public sealed class IpLookupHttpClient(HttpClient client, ILogger<IpLookupHttpClient> logger) : IIpLookupHttpClient
{
    public async Task<IpDetails> GetDetailsAsync(string ipAddress)
    {
        logger.LogInformation("Lookup Client: Requesting LIVE details for IP.");
        try
        {
            var response = await client.GetAsync($"{BatchServiceConstants.IP_LOOKUP_ENDPOINT}/{ipAddress}");
            response.EnsureSuccessStatusCode();

            logger.LogInformation("Lookup Client: Successfully fetched live details for IP.");

            var details = await response.Content.ReadFromJsonAsync<IpDetails>();

            if (details == null)
            {
                throw new NotFoundException($"Lookup Service returned null details.");
            }

            return details;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Lookup Client: Network or HTTP error while trying to GET live IP.");
            throw new BadRequestException("Failed to communicate with IP Lookup Service for GET.", ex);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Lookup Client: Deserialization error for IP.");
            throw new BadRequestException($"Failed to parse response from IP Lookup Service for GET.", ex);
        }
    }
}