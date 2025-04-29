// Manager/Services/RequestTimeoutService.cs

using Manager.Data;
using Manager.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Manager.Services;

public class RequestTimeoutService(IOptions<ManagerOptions> opts, IServiceProvider provider, ILogger<RequestTimeoutService> logger)
    : BackgroundService
{
    private readonly int _timeout = opts.Value.TaskTimeoutSeconds;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CrackDbContext>();
                var now = DateTime.UtcNow;
                var expired = await db.Requests
                    .Where(r => r.Status == "IN_PROGRESS" && (now - r.CreatedAt).TotalSeconds > _timeout)
                    .ToListAsync(stoppingToken);

                foreach (var req in expired)
                {
                    req.Status = "ERROR";
                    req.CompletedAt = DateTime.UtcNow;
                }
                if (expired.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Timeout checker error");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}