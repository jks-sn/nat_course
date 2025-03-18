// Manager/Models/CrackRequestModel.cs

namespace Manager.Models;

public class CrackRequestModel
{
    public string Hash { get; set; } = null!;
    public int MaxLength { get; set; }
}