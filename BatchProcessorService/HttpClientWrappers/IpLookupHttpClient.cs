using System.Text.Json;
using BatchProcessorService.Interfaces;
using Shared.Models;

public class IpLookupHttpClient(HttpClient client, ILogger<IpLookupHttpClient> logger) : IIpLookupHttpClient
{
    private const string LookupPath = "/api/IpLookup";

    public async Task<IpDetails> GetDetailsAsync(string ipAddress)
    {
        logger.LogInformation("Lookup Client: Requesting LIVE details for IP: {Ip}", ipAddress);
        try
        {
            var response = await client.GetAsync($"{LookupPath}/{ipAddress}");
            response.EnsureSuccessStatusCode();

            logger.LogInformation("Lookup Client: Successfully fetched live details for IP: {Ip}", ipAddress);
            
            var details = await response.Content.ReadFromJsonAsync<IpDetails>();

            if (details == null)
            {
                throw new InvalidOperationException($"Lookup Service returned null details for {ipAddress}.");
            }
            
            return details;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Lookup Client: Network or HTTP error while trying to GET live IP details for {Ip}.", ipAddress);
            throw new InvalidOperationException($"Failed to communicate with IP Lookup Service for GET {ipAddress}.", ex);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Lookup Client: Deserialization error for IP: {Ip}.", ipAddress);
            throw new InvalidOperationException($"Failed to parse response from IP Lookup Service for GET {ipAddress}.", ex);
        }
    }
}