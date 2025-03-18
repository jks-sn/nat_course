// Manager/Options/ManagerOptions.cs

using System.ComponentModel.DataAnnotations;

namespace Manager.Options;

public class ManagerOptions
{
    [Range(1, int.MaxValue, ErrorMessage = "WorkersCount должен быть не менее 1.")]
    public int WorkersCount { get; set; }
    
    [Required]
    public string WorkerUrlPattern { get; set;} = string.Empty;
    
    [Range(1, int.MaxValue, ErrorMessage = "TaskTimeoutSeconds должен быть положительным числом.")]
    public int TaskTimeoutSeconds { get; set; }
}