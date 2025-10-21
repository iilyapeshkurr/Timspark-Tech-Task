using Shared.Models;
using CacheService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IpProject.IpCacheService.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class IpCacheController(IIpCacheService _cacheService) : ControllerBase
{

    [HttpGet("{ipAddress}")]
    [SwaggerOperation(
        Summary = "Get Ip Details cache info",
        Description = "Provides detailed information about a single Ip using its cache."
    )]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IpDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetCacheDetails(string ipAddress)
    {
        var details = _cacheService.GetCacheDetailsAsync(ipAddress);
        
        if (details != null)
        {
            return Ok(details);
        }

        return NotFound();
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Sets Ip Details info cache",
        Description = "Sets Ip Details info cache in memory."
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SetCacheDetails([FromBody] IpDetails details)
    {
        _cacheService.SetCacheDetailsAsync(details);
        return CreatedAtAction(nameof(GetCacheDetails), new { ipAddress = details.Ip }, details);      
    }
}
