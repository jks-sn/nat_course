// Manager/Controllers/HashController.cs

using Manager.Data;
using Manager.Models;
using Manager.Options;
using Manager.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Manager.Controllers;

[ApiController]
[Route("api/hash")]
public class HashController(
    IOptions<ManagerOptions> opt,
    CrackDbContext db,
    ILogger<HashController> logger) : ControllerBase
{
    private readonly int _workers = opt.Value.WorkersCount;

    [HttpPost("crack")]
    public async Task<IActionResult> Crack([FromBody] CrackRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Hash)) 
            return BadRequest("hash required");
     
        logger.LogInformation("Received crack request. Hash={Hash}, L={Max}", 
            dto.Hash, dto.MaxLength);
        
        var requestId = Guid.NewGuid();
        var request = new CrackRequest
        {
            Id = requestId,
            Hash = dto.Hash,
            MaxLength = dto.MaxLength,
            Status = "IN_PROGRESS"
        };

        for (var i = 1; i <= _workers; i++)
        {
            request.Tasks.Add(new CrackTask
            {
                TaskId = Guid.NewGuid(),
                PartNumber = i,
                PartCount = _workers,
                Published = false,
                Completed = false
            });
        }

        db.Requests.Add(request);
        await db.SaveChangesAsync();
        logger.LogInformation("Request {Id} persisted with {Tasks} subtasks", 
            requestId, _workers);
        return Ok(new { requestId });
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] Guid requestId)
    {
        logger.LogDebug("Status poll for {Id}", requestId);
        var req = await db.Requests.Include(r => r.FoundWords).FirstOrDefaultAsync(r => r.Id == requestId);
        if (req == null) 
            return NotFound(new { status = "ERROR", data = (string[])null! });
        if (req.Status == "READY")
            return Ok(new { status = "READY", data = req.FoundWords.Select(f => f.Word).ToArray() });
        return Ok(new { status = req.Status, data = (string[])null! });
    }
}