using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using IpLookupService.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace IpLookupService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IpLookupController(IIpServiceWrapper _ipServiceWrapper) : ControllerBase
{

    [HttpGet("{ipAddress}")]
    [SwaggerOperation(
        Summary = "Get Ip Details info",
        Description = "Provides detailed information about a single Ip."
    )]
    [ProducesResponseType(typeof(IpDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIpInfo(string ipAddress)
    {
        var ipDetails = await _ipServiceWrapper.GetIpDetailsAsync(ipAddress);
        return Ok(ipDetails);   
    }

}