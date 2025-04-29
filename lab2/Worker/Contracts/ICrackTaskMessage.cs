// Manager/Contracts/ICrackTaskMessage.cs
namespace Manager.Contracts;

public interface ICrackTaskMessage
{
    Guid TaskId { get; }
    Guid RequestId { get; }
    string Hash { get; }
    int MaxLength { get; }
    int PartNumber { get; }
    int PartCount { get; }
}