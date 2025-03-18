//Worker/Services/WorkerClientService.cs

using System.Net.Http.Json;
using Dto;
using Manager.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class WorkerClientService(
    ILogger<WorkerClientService> logger,
    HttpClient httpClient,
    IOptions<ManagerOptions> managerOptions,
    RequestStorageService storage)
{
    private readonly ManagerOptions _options = managerOptions.Value;

    public async Task SendTasksAsync(string requestId, string hash, int maxLength)
    {
        logger.LogInformation(
            "Sending tasks for {RequestId} to {Count} workers ...",
            requestId, _options.WorkersCount);

        storage.SetWorkerCount(requestId, _options.WorkersCount);
        var tasks = new List<Task>();
        for (var i = 1; i <= _options.WorkersCount; i++)
        {
            var endpoint = string.Format(_options.WorkerUrlPattern, i) + "/internal/api/worker/hash/crack/task";
            
            var dto = new WorkerTaskDto
            {
                RequestId = requestId,
                Hash = hash,
                MaxLength = maxLength,
                PartNumber = i,
                PartCount = _options.WorkersCount
            };

            logger.LogInformation(
                "POST to {Endpoint} (partNumber={i})", endpoint, i);

            Task.Run(async () =>
            {
                try
                {
                    var response = await httpClient.PostAsJsonAsync(endpoint, dto);
                    response.EnsureSuccessStatusCode();
                    logger.LogInformation("Task posted successfully to {Endpoint}", endpoint);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending task to worker {Endpoint}", endpoint);
                }
            });
        }
    }
}