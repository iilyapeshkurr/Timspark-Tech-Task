using Shared.Models;
using CacheService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IpProject.IpCacheService.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class IpCacheController(IIpCacheService _cacheService) : ControllerBase
{

    [HttpGet("{ipAddress}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IpDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDetails(string ipAddress)
    {
        try
        {
            var details = await _cacheService.GetDetailsAsync(ipAddress);

            if (details != null)
            {
                return Ok(details);
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Cache System Unavailable", message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetDetails([FromBody] IpDetails details)
    {
        if (string.IsNullOrEmpty(details.Ip))
        {
            return BadRequest("IP Address is required to cache details.");
        }

        try
        {
            await _cacheService.SetDetailsAsync(details);
            return CreatedAtAction(nameof(GetDetails), new { ipAddress = details.Ip }, details);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Cache System Unavailable", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
