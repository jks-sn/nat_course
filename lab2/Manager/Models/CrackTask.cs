// Manager/Models/CrackTask.cs

namespace Manager.Models;

public class CrackTask
{
    public Guid TaskId { get; set; }
    public Guid RequestId { get; set; }
    public CrackRequest Request { get; set; } = null!;

    public int PartNumber { get; set; }
    public int PartCount { get; set; }

    public bool Published { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}