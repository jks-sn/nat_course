// Manager/Controllers/CrackRequestDto.cs
namespace Manager.Controllers;

public class CrackRequestDto
{
    public string Hash { get; set; } = null!;
    public int MaxLength { get; set; }
}