// Worker/Consumers/CrackTaskConsumer.cs

using Manager.Contracts;
using MassTransit;
using Worker.Options;
using Worker.Services;
using Microsoft.Extensions.Options;

namespace Worker.Consumers;

public class CrackTaskConsumer(
    ILogger<CrackTaskConsumer> logger,
    HashCrackService hash,
    IBus bus,
    IOptions<WorkerOptions> opt) : IConsumer<ICrackTaskMessage>
{
    public async Task Consume(ConsumeContext<ICrackTaskMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Start task {TaskId} part {Part}/{Total}", msg.TaskId, msg.PartNumber, msg.PartCount);

        if (opt.Value.SimulatedDelaySeconds > 0)
            await Task.Delay(TimeSpan.FromSeconds(opt.Value.SimulatedDelaySeconds), context.CancellationToken);

        var words = hash.BruteForce(msg.Hash, msg.MaxLength, msg.PartNumber, msg.PartCount, context.CancellationToken);
        logger.LogInformation("Finished task {TaskId}: found {Count} words", msg.TaskId, words.Count);

        var result = new {
            TaskId = msg.TaskId,
            RequestId = msg.RequestId,
            FoundWords = words.AsReadOnly()
        };
        

        await bus.Publish<IWorkerResultMessage>(result, context.CancellationToken);
    }
}