// Manager/Models/CrackRequest.cs

namespace Manager.Models;

public class CrackRequest
{
    public Guid Id { get; set; }
    public string Hash { get; set; } = null!;
    public int MaxLength { get; set; }
    public string Status { get; set; } = "IN_PROGRESS"; // IN_PROGRESS|READY|ERROR
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<CrackTask> Tasks { get; set; } = new List<CrackTask>();
    public ICollection<FoundWord> FoundWords { get; set; } = new List<FoundWord>();
}