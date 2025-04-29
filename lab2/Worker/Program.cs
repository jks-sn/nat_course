// Worker/Program.cs

using MassTransit;
using Worker.Options;
using Worker.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));

builder.Services.AddSingleton<HashCrackService>();

// MassTransit RabbitMQ
var rabbit = builder.Configuration.GetSection("Rabbit").Get<RabbitOptions>()!;

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<Worker.Consumers.CrackTaskConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbit.Host, h =>
        {
            h.Username(rabbit.Username);
            h.Password(rabbit.Password);
        });

        cfg.ReceiveEndpoint(rabbit.TaskQueue, e =>
        {
            e.PrefetchCount = 1; // по одному заданию на воркер
            e.ConfigureConsumer<Worker.Consumers.CrackTaskConsumer>(ctx);
        });
    });
});

builder.Services.AddControllers();

var app = builder.Build();
app.MapGet("/health", () => "OK");
app.Run();