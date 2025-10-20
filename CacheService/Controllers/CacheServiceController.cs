using Shared.Models;
using CacheService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IpProject.IpCacheService.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class IpCacheController(IIpCacheService _cacheService) : ControllerBase
{

    [HttpGet("{ip-address}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IpDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetCacheDetails(string ipAddress)
    {
        var details = _cacheService.GetCacheDetailsAsync(ipAddress);

        if (details != null)
        {
            return Ok(details);
        }

        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SetCacheDetails([FromBody] IpDetails details)
    {
        _cacheService.SetCacheDetailsAsync(details);
        return CreatedAtAction(nameof(GetCacheDetails), new { ipAddress = details.Ip }, details);      
    }
}
