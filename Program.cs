// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Serilog;
using YouTubeShortsAutomator.Infrastructure.Extensions;
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Extensions;
using YouTubeShortsAutomator.Middleware;
using YouTubeShortsAutomator.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/application-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add infrastructure services
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add Phase 2 application services
    builder.Services.AddApplicationServices();
    builder.Services.AddBackgroundServices();
    builder.Services.AddSingleton<IMetricsCollector, MetricsCollector>();

    // Configure rate limiting
    builder.Services.AddRateLimitingOptions(
        requestsPerWindow: builder.Configuration.GetValue<int>("RateLimit:RequestsPerWindow", 100),
        windowSizeSeconds: builder.Configuration.GetValue<int>("RateLimit:WindowSizeSeconds", 60)
    );

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Apply custom middleware
    app.UseApplicationMiddleware();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Initialize database
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Log.Information("Database initialized successfully");
    }

    Log.Information("Application started successfully with all Phase 2 infrastructure");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
