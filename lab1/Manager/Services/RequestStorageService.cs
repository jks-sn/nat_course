// Manager/Services/RequestStorageService.cs

using System.Collections.Concurrent;
using Manager.Models;
using Manager.Options;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class RequestStorageService(
    ILogger<RequestStorageService> logger,
    IOptions<ManagerOptions> configOptions)
{
    private readonly ConcurrentDictionary<string, RequestEntry> _requests =
        new ConcurrentDictionary<string, RequestEntry>();

    private readonly ILogger<RequestStorageService> _logger = logger;
    private readonly ManagerOptions _config = configOptions.Value;

    public void CreateRequest(string requestId, string hash, int maxLength)
    {
        var entry = new RequestEntry
        {
            RequestId = requestId,
            Hash = hash,
            MaxLength = maxLength,
            Status = "IN_PROGRESS",
            FoundWords = new List<string>(),
            
            WorkerCount = _config.WorkersCount,
            WorkerResponses = 0,
            CreatedAt = DateTime.UtcNow
        };
        _requests[requestId] = entry;
    }

    public RequestEntry? GetRequest(string requestId)
    {
        _requests.TryGetValue(requestId, out var entry);
        return entry;
    }

    public bool ApplyWorkerResult(string requestId, List<string> foundWords)
    {
        if (!_requests.TryGetValue(requestId, out var entry))
        {
            return false;
        }

        lock (entry)
        {
            foreach (var w in foundWords)
            {
                if (!entry.FoundWords.Contains(w))
                {
                    entry.FoundWords.Add(w);
                }
            }

            entry.WorkerResponses += 1;
            if (entry.WorkerResponses >= entry.WorkerCount)
            {
                entry.Status = "READY";
            }
        }
        return true;
    }
    
    public void SetWorkerCount(string requestId, int count)
    {
        if (_requests.TryGetValue(requestId, out var entry))
        {
            lock (entry)
            {
                entry.WorkerCount = count;
            }
        }
    }
    
    public void CheckTimeouts(int taskTimeoutSeconds)
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _requests)
        {
            var requestId = kvp.Key;
            var entry = kvp.Value;
            lock (entry)
            {
                if (entry.Status == "IN_PROGRESS" && (now - entry.CreatedAt).TotalSeconds > taskTimeoutSeconds)
                {
                    entry.Status = "ERROR";
                    _logger.LogWarning("Request {RequestId} timed out and marked as ERROR", requestId);
                }
            }
        }
    }
}
