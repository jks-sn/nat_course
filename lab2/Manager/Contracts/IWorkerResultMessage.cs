// Manager/Contracts/IWorkerResultMessage.cs

namespace Manager.Contracts;

public interface IWorkerResultMessage
{
    Guid TaskId { get; }
    Guid RequestId { get; }
    IReadOnlyList<string> FoundWords { get; }
}