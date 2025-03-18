using System.ComponentModel.DataAnnotations;

namespace Worker.Options;

public class WorkerOptions
{
    [Required]
    public string ManagerApiUrl { get; set; } = string.Empty;
    public int SimulatedDelaySeconds { get; set; }

}