// Manager/Program.cs

using Manager.Options;
using Manager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.Configure<ManagerOptions>(builder.Configuration.GetSection("ManagerOptions"));

builder.Services.AddSingleton<RequestStorageService>();
builder.Services.AddHttpClient<WorkerClientService>();
builder.Services.AddControllers();

builder.Services.AddHostedService<RequestTimeoutService>();


builder.Services.AddHttpClient<WorkerClientService>(client =>
{

});
var app = builder.Build();

app.MapControllers();
app.Run();