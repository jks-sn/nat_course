// Worker/Controllers/HashTaskController.cs

using Dto;
using Microsoft.AspNetCore.Mvc;
using Worker.Services;

namespace Worker.Controllers;

[ApiController]
[Route("internal/api/worker/hash/crack/task")]
public class HashTaskController(
    ILogger<HashTaskController> logger,
    WorkerTaskRunner taskRunner)
    : ControllerBase
{
    [HttpPost]
    public IActionResult ReceiveTask([FromBody] WorkerTaskDto taskDto)
    {
        logger.LogInformation(
            "Worker received task for RequestId={reqId} (Part={partNumber}/{partCount})",
            taskDto.RequestId, taskDto.PartNumber, taskDto.PartCount);

        _ = Task.Run(() => taskRunner.RunTaskAsync(taskDto));
        logger.LogInformation("Worker start task running");
        return Ok(new { status = "Task accepted" });
    }
}