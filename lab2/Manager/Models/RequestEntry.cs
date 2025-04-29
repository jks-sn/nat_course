// Manager/Models/RequestEntry.cs

namespace Manager.Models;

public class RequestEntry
{
    public string RequestId { get; set; } = null!;
    public string Hash { get; set; } = null!;
    public int MaxLength { get; set; }
    public string Status { get; set; } = "IN_PROGRESS";

    public List<string> FoundWords { get; set; } = new();

    public int WorkerCount { get; set; }
    public int WorkerResponses { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}