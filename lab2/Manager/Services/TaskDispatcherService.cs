// Manager/Services/TaskDispatcherService.cs

using Manager.Contracts;
using Manager.Data;
using Manager.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class TaskDispatcherService(
    IServiceProvider provider,
    IOptions<RabbitOptions> rabbitOpts,
    ILogger<TaskDispatcherService> logger)
    : BackgroundService
{
    private readonly RabbitOptions _rabbit = rabbitOpts.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CrackDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IBus>();

                var tasks = await db.Tasks
                    .Include(t => t.Request)
                    .Where(t => !t.Published)
                    .OrderBy(t => t.CreatedAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                if (tasks.Count == 0)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                
                logger.LogInformation("Dispatcher: picked {Count} new tasks", tasks.Count);

                var endpoint = await bus.GetSendEndpoint(new Uri($"queue:{_rabbit.TaskQueue}"));
                
                foreach (var task in tasks)
                {
                    await endpoint.Send<ICrackTaskMessage>(new
                    {
                        task.TaskId,
                        task.RequestId,
                        task.Request.Hash,
                        task.Request.MaxLength,
                        task.PartNumber,
                        task.PartCount
                    }, stoppingToken);
                    
                    task.Published = true;
                    logger.LogInformation("Task {TaskId} published to MQ", task.TaskId);
                }

                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Dispatcher: saved state, sleep 1 s");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in TaskDispatcher loop");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}