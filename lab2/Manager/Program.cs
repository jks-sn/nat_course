// Manager/Program.cs

using Manager.Consumers;
using Manager.Contracts;
using Manager.Data;
using Manager.Options;
using Manager.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging ----------
builder.Logging.ClearProviders();                     // убираем то, что ASP.NET регистрирует сам
builder.Logging.AddConfiguration(                     // читаем уровни из appsettings*.json
    builder.Configuration.GetSection("Logging"));
builder.Logging.AddSimpleConsole(o =>                 // наш Console-provider
{
    o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
    o.SingleLine      = true;
});

// Configuration
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<ManagerOptions>(builder.Configuration.GetSection("ManagerOptions"));
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));

// DbContext
var cs = builder.Configuration.GetConnectionString("CrackDb")!;
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
builder.Services.AddDbContext<CrackDbContext>(opts =>
    opts.UseNpgsql(cs, npgsql => npgsql.EnableRetryOnFailure()));

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<WorkerResultConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbit = ctx.GetRequiredService<IOptions<RabbitOptions>>().Value;

        cfg.Host(rabbit.Host, h =>
        {
            h.Username(rabbit.Username);
            h.Password(rabbit.Password);
        });

        cfg.ReceiveEndpoint(rabbit.ResultQueue, e =>
        {
            e.PrefetchCount = 16;
            // e.ConcurrentMessageLimit = 4;
            e.ConfigureConsumer<WorkerResultConsumer>(ctx);
        });
    });
});

// Hosted services
builder.Services.AddHostedService<TaskDispatcherService>();
builder.Services.AddHostedService<RequestTimeoutService>();
builder.Services.AddSingleton<ResultWriter>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ResultWriter>());

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null); // camel-case уже в модели


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrackDbContext>();
    db.Database.Migrate();
}

app.MapControllers();
app.Run();