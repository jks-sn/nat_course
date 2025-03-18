//Worker/Services/WorkerTaskRunner.cs

using System.Net.Http.Json;
using Dto;
using Microsoft.Extensions.Options;
using Worker.Interfaces;
using Worker.Options;

namespace Worker.Services;

public class WorkerTaskRunner(
    ILogger<WorkerTaskRunner> logger,
    HttpClient httpClient,
    IHashCrackService hashCrackService,
    IOptions<WorkerOptions> options)
{
    private readonly WorkerOptions _workerOptions = options.Value;

    public async Task RunTaskAsync(WorkerTaskDto taskDto)
    {
        logger.LogInformation(
            "Starting crack for RequestId={RequestId}, Part={PartNumber}/{PartCount}",
            taskDto.RequestId, taskDto.PartNumber, taskDto.PartCount);

        if (_workerOptions.SimulatedDelaySeconds > 0)
        {
            logger.LogInformation("Simulating delay of {Delay} seconds", _workerOptions.SimulatedDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_workerOptions.SimulatedDelaySeconds));
        }

        var foundWords = hashCrackService.BruteForce(
            taskDto.Hash,
            taskDto.MaxLength,
            taskDto.PartNumber,
            taskDto.PartCount);

        logger.LogInformation(
            "Finished crack for {RequestId}. Found {Count} words.",
            taskDto.RequestId, foundWords.Count);

        var result = new WorkerResultDto
        {
            RequestId = taskDto.RequestId,
            FoundWords = foundWords
        };
        try
        {
            var patchUrl = "/internal/api/manager/hash/crack/request";
            var response = await httpClient.PatchAsync(patchUrl, JsonContent.Create(result));
            response.EnsureSuccessStatusCode();
            logger.LogInformation("Successfully sent result for RequestId={RequestId} to Manager", taskDto.RequestId);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex,
                "Error sending results to manager for RequestId={RequestId}",
                taskDto.RequestId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error while sending results to manager for RequestId={RequestId}",
                taskDto.RequestId);
        }
    }
}