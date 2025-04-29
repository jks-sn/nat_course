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

// -------------------------------------------------------------------
// //  Logging
// // -------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration( 
    builder.Configuration.GetSection("Logging"));
builder.Logging.AddSimpleConsole(o =>
{
    o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
    o.SingleLine      = true;
});

// -------------------------------------------------------------------
// //  Configuration & Options
// // -------------------------------------------------------------------
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<ManagerOptions>(builder.Configuration.GetSection("ManagerOptions"));
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));

// -------------------------------------------------------------------
// //  PostgreSQL  (multihost, retry-on-failure)
// // -------------------------------------------------------------------
var cs = builder.Configuration.GetConnectionString("CrackDb")!;
builder.Services.AddDbContext<CrackDbContext>((sp, opt) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    opt.UseNpgsql(cfg.GetConnectionString("CrackDb"),
        npg => npg.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null));
});

// -------------------------------------------------------------------
// //  MassTransit (RabbitMQ)
// // -------------------------------------------------------------------
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

// -------------------------------------------------------------------
// //  Hosted services & DI helpers
// // -------------------------------------------------------------------
builder.Services.AddSingleton<TaskDispatcherService>();
builder.Services.AddSingleton<ITaskDispatcher>(sp =>
    sp.GetRequiredService<TaskDispatcherService>());
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<TaskDispatcherService>());
builder.Services.AddHostedService<RequestTimeoutService>();
builder.Services.AddSingleton<ResultWriter>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ResultWriter>());

// -------------------------------------------------------------------
//  MVC
// -------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrackDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();
app.Run();