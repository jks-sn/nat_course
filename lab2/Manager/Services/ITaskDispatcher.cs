// Manager/Services/ITaskDispatcher.cs

using Manager.Models;

namespace Manager.Services;

public interface ITaskDispatcher
{
    Task PublishTasksAsync(
        CrackRequest request,
        CancellationToken cancellationToken = default);
}