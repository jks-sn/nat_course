// Manager/Options/ManagerOptions.cs

using System.ComponentModel.DataAnnotations;

namespace Manager.Options;

public class ManagerOptions
{
    [Range(1,int.MaxValue)]
    public int WorkersCount { get; set; }

    [Range(1,int.MaxValue)]
    public int TaskTimeoutSeconds { get; set; }
}