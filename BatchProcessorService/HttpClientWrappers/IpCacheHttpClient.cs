using System.Text.Json;
using BatchProcessorService.Interfaces;
using Shared.Models;

namespace BatchProcessorService.HttpClientWrappers;

public sealed class IpCacheHttpClient(HttpClient _client, ILogger<IpCacheHttpClient> _logger) : IIpCacheHttpClient
{
    private const string CachePath = "/api/IpCache";

    public async Task<IpDetails?> GetDetailsAsync(string ipAddress)
    {
        _logger.LogInformation("Cache Client: Requesting details for IP: {Ip}", ipAddress);
        try
        {
            var response = await _client.GetAsync($"{CachePath}/{ipAddress}");

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogInformation("Cache Client: Cache MISS for IP: {Ip} (204 No Content).", ipAddress);
                return null;
            }

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Cache Client: Cache HIT for IP: {Ip} (200 OK).", ipAddress);
            return await response.Content.ReadFromJsonAsync<IpDetails>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cache Client: Network error while trying to GET IP details for {Ip}.", ipAddress);
            throw new InvalidOperationException($"Failed to communicate with IP Cache Service for GET {ipAddress}.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Cache Client: Deserialization error for IP: {Ip}.", ipAddress);
            throw new InvalidOperationException($"Failed to parse response from IP Cache Service for GET {ipAddress}.", ex);
        }
    }

    public async Task SetDetailsAsync(IpDetails details)
    {
        _logger.LogInformation("Cache Client: Requesting to SET details for IP: {Ip}", details.Ip);
        try
        {
            var response = await _client.PostAsJsonAsync(CachePath, details);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Cache Client: Successfully set details for IP: {Ip}", details.Ip);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cache Client: Network error while trying to SET IP details for {Ip}.", details.Ip);
            throw new InvalidOperationException($"Failed to communicate with IP Cache Service for SET {details.Ip}.", ex);
        }
    }
}