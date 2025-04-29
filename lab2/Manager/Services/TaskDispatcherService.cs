// Manager/Services/TaskDispatcherService.cs

using Manager.Contracts;
using Manager.Data;
using Manager.Models;
using Manager.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class TaskDispatcherService(
    IServiceProvider provider,
    IOptions<RabbitOptions> rabbitOpts,
    ILogger<TaskDispatcherService> logger)
    : BackgroundService, ITaskDispatcher
{
    private readonly RabbitOptions _rabbit = rabbitOpts.Value;

    //---------------------------------------------------------------
    //  ITaskDispatcher – немедленная публикация из контроллера
    //---------------------------------------------------------------
    public async Task PublishTasksAsync(
        CrackRequest request,
        CancellationToken ct = default)
    {
        // отдельный DI-scope, чтобы не тащить DbContext из контроллера
        using var scope = provider.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<CrackDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var tasks = await db.Tasks
            .Where(t => t.RequestId == request.Id && !t.Published)
            .ToListAsync(ct);

        if (tasks.Count == 0) return;

        var endpoint = await bus.GetSendEndpoint(
            new Uri($"queue:{_rabbit.TaskQueue}"));

        foreach (var t in tasks)
        {
            await endpoint.Send<ICrackTaskMessage>(new
            {
                t.TaskId, t.RequestId,
                request.Hash, request.MaxLength,
                t.PartNumber, t.PartCount
            }, ct);

            t.Published = true;
        }
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "PublishTasksAsync: sent {Cnt} tasks for Request {Id}",
            tasks.Count, request.Id);
    }
    
    //---------------------------------------------------------------
    //  BackgroundService – проверяем БД раз в секунду
    //---------------------------------------------------------------
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