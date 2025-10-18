using IpStack;
using IpStack.Services;
using IpLookupService.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Models;
using IpLookupService.Interfaces;

namespace IpLookupService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class IpLookupController(IIpServiceWrapper _ipServiceWrapper) : ControllerBase
{

    [HttpGet("{ipAddress}")]
    [ProducesResponseType(typeof(IpDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetIpInfo(string ipAddress)
    {
        try
        {
            var ipDetails = await _ipServiceWrapper.GetIpDetailsAsync(ipAddress);
            return Ok(ipDetails);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (IPServiceNotAvailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }   
    }

}