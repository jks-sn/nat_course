// Manager/Controllers/HashController.cs

using Microsoft.AspNetCore.Mvc;
using Manager.Models;
using Manager.Services;

namespace Manager.Controllers;

[ApiController]
[Route("api/hash")]
public class HashController(
    ILogger<HashController> logger,
    RequestStorageService storage,
    WorkerClientService workerClient)
    : ControllerBase
{

    [HttpPost("crack")]
    public IActionResult CrackHash([FromBody] CrackRequestModel model)
    {
        var requestId = Guid.NewGuid().ToString();
        
        logger.LogInformation("Received new crack request. RequestId={RequestId}, Hash={Hash}, MaxLength={MaxLength}", 
            requestId, model.Hash, model.MaxLength);
        
        storage.CreateRequest(requestId, model.Hash, model.MaxLength);
        
        workerClient.SendTasksAsync(requestId, model.Hash, model.MaxLength)
            .ConfigureAwait(false);

        logger.LogInformation("Dispatched tasks to workers for RequestId={RequestId}", requestId);

        return Ok(new { requestId });
    }

    [HttpGet("status")]
    public IActionResult GetStatus([FromQuery] string requestId)
    {
        logger.LogInformation("Checking status for RequestId={RequestId}", requestId);

        var entry = storage.GetRequest(requestId);
        if (entry == null)
        {
            logger.LogWarning("RequestId={RequestId} not found", requestId);
            return NotFound(
                new { status = "ERROR", data = (string[]) null }
            );
        }

        if (entry.Status == "READY")
        {
            logger.LogInformation("RequestId={RequestId} completed successfully", requestId);
            return Ok(new { status = "READY", data = entry.FoundWords });
        }
        else
        {
            logger.LogInformation("RequestId={RequestId} is in status {Status}", requestId, entry.Status);
            return Ok(new { status = entry.Status, data = (string[]) null });
        }
    }
}