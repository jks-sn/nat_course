// Manager/Consumers/WorkerResultConsumer.cs

using Manager.Contracts;
using MassTransit;

namespace Manager.Consumers;

public class WorkerResultConsumer(
    ILogger<WorkerResultConsumer> log,
    ResultWriter writer) : IConsumer<IWorkerResultMessage>
{
    public async Task Consume(ConsumeContext<IWorkerResultMessage> ctx)
    {
        log.LogInformation("Task {TaskId} done, enqueue for DB", ctx.Message.TaskId);
        await writer.EnqueueAsync(ctx.Message); 
    }
}