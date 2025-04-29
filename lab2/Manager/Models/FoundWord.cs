// Manager/Models/FoundWord.cs

namespace Manager.Models;

public class FoundWord
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public CrackRequest Request { get; set; } = null!;
    public string Word { get; set; } = null!;
}