using BatchProcessorService.DTOs;
using BatchProcessorService.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BatchController(IIpBatchService _batchService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BatchStartResponseDTO), 202)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessBatch([FromBody] BatchRequestDTO request)
    {
        try
        {
            if (request == null || request.IpAddresses == null || !request.IpAddresses.Any())
            {
                return BadRequest("Request must contain a list of IP addresses in the 'IpAddresses' field.");
            }

            var response = await _batchService.ProcessBatchAsync(request.IpAddresses);

            return Accepted(response);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal error occurred while initiating the batch process.");
        }
    }

    [HttpGet("{batchId:guid}")]
    [ProducesResponseType(typeof(BatchStatusResponseDTO), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBatchStatus(Guid batchId)
    {
        try
        {
            var status = await _batchService.GetBatchStatusAsync(batchId);

            if (status == null)
            {
                return NotFound($"Batch job with ID '{batchId}' not found.");
            }

            return Ok(status);
        }
        catch (Exception)
        {
            return StatusCode(500, $"An internal error occurred while retrieving status for job '{batchId}'.");
        }
    }
}