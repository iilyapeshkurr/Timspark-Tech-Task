using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using IpLookupService.Interfaces;

namespace IpLookupService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IpLookupController(IIpServiceWrapper _ipServiceWrapper) : ControllerBase
{

    [HttpGet("{ip-address}")]
    [ProducesResponseType(typeof(IpDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetIpInfo(string ipAddress)
    {
        var ipDetails = await _ipServiceWrapper.GetIpDetailsAsync(ipAddress);
        return Ok(ipDetails);   
    }

}