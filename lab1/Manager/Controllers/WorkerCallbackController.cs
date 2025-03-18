//Manager/Controllers/WorkerCallbackController.cs

using Dto;
using Microsoft.AspNetCore.Mvc;
using Manager.Services;

namespace Manager.Controllers;

[ApiController]
[Route("internal/api/manager/hash/crack/request")]
public class WorkerCallbackController(
    ILogger<WorkerCallbackController> logger,
    RequestStorageService storage)
    : ControllerBase
{
    [HttpPatch]
    public IActionResult PatchWorkerResult([FromBody] WorkerResultDto workerResult)
    {
        logger.LogInformation("Patch from worker for RequestId={0}, words={1}",
            workerResult.RequestId, workerResult.FoundWords.Count);

        var ok = storage.ApplyWorkerResult(workerResult.RequestId, workerResult.FoundWords);
        if (!ok)
        {
            return NotFound();
        }
        return Ok();
    }
}