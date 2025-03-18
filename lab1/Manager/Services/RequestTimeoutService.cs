using Microsoft.Extensions.Hosting;
using Manager.Options;
using Microsoft.Extensions.Options;

namespace Manager.Services;

    public class RequestTimeoutService(
        RequestStorageService storage,
        IOptions<ManagerOptions> options,
        ILogger<RequestTimeoutService> logger)
        : BackgroundService
    {
        private readonly ManagerOptions _options = options.Value;
        private readonly ILogger<RequestTimeoutService> _logger = logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                storage.CheckTimeouts(_options.TaskTimeoutSeconds);
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }