// Manager/Services/ResultWriter.cs

using System.Threading.Channels;
using Manager.Contracts;
using Manager.Data;
using Manager.Models;
using Microsoft.EntityFrameworkCore;

public class ResultWriter(ILogger<ResultWriter> log, IServiceScopeFactory sf) : BackgroundService
{
    private readonly Channel<IWorkerResultMessage> _channel = Channel.CreateUnbounded<IWorkerResultMessage>();

    public ValueTask EnqueueAsync(IWorkerResultMessage msg) => _channel.Writer.WriteAsync(msg);

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        await foreach (var msg in _channel.Reader.ReadAllAsync(stop))
        {
            try
            {
                await using var scope = sf.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<CrackDbContext>();

                var task = await db.Tasks.Include(t => t.Request)
                    .FirstOrDefaultAsync(t => t.TaskId == msg.TaskId, stop);
                if (task is null) 
                    continue;

                db.FoundWords.AddRange(msg.FoundWords.Select(w => new FoundWord
                {
                    Id = Guid.NewGuid(),
                    RequestId = msg.RequestId,
                    Word = w
                }));
                task.Completed = true;
                
                await db.SaveChangesAsync(stop);

                var allDone = await db.Tasks
                    .Where(t => t.RequestId == msg.RequestId)
                    .AllAsync(t => t.Completed, stop);
                
                if (allDone)
                {
                    task.Request.Status      = "READY";
                    task.Request.CompletedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(stop);
                }
                
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to persist results for request {Request}", msg.RequestId);
            }
        }
    }
}