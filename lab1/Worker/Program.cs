//Worker/Program.cs

using Microsoft.Extensions.Options;
using Worker.Interfaces;
using Worker.Options;
using Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));

// var managerUrl = Environment.GetEnvironmentVariable("MANAGER_API_URL") 
//                  ?? "http://manager:8082";

builder.Services.AddHttpClient<WorkerTaskRunner>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<WorkerOptions>>().Value;
    client.BaseAddress = new Uri(options.ManagerApiUrl);
});

builder.Services.AddSingleton<IHashCrackService, HashCrackService>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();