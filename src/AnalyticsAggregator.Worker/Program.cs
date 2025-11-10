using AnalyticsAggregator.Core.Interfaces;
using AnalyticsAggregator.Infrastructure.Data;
using AnalyticsAggregator.Infrastructure.Services;
using AnalyticsAggregator.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=analytics;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<IAggregationService, AggregationService>();
builder.Services.AddSingleton<IRabbitMQService>(sp =>
{
    var hostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
    var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();
    return new RabbitMQService(hostName, logger);
});

// Worker
builder.Services.AddHostedService<AnalyticsConsumerWorker>();

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

host.Run();
