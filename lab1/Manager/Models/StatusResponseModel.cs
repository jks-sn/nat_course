// Manager/Models/StatusResponseModel.cs

namespace Manager.Models;

public class StatusResponseModel
{
    public string Status { get; set; } = null!;
    public List<string>? Data { get; set; }
}