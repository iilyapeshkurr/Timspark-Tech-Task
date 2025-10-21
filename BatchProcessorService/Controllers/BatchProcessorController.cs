using BatchProcessorService.DTOs;
using BatchProcessorService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/[controller]")]
public sealed class BatchController(IIpBatchService _batchService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Start processing a batch of IP addresses",
        Description = "Start proccesing a batch. Returns a batch ID to track the status."
    )]
    [ProducesResponseType(typeof(BatchStartResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessBatch([FromBody] BatchRequestDTO request, CancellationToken cancellationToken)
    {
        var response = await _batchService.ProcessBatchAsync(request.IpAddresses, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{batchId:guid}")]
    [SwaggerOperation(
        Summary = "Get batch processing status",
        Description = "Get the status of a batch processing job using its batch ID."
    )]
    [ProducesResponseType(typeof(BatchStatusResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetBatchStatus(Guid batchId)
    {
        var status = _batchService.GetBatchStatus(batchId);

        return Ok(status);
     }
}