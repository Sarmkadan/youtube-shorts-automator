// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Infrastructure.Repositories;

namespace YouTubeShortsAutomator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ConfigurationService _configService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ApplicationDbContext dbContext,
        ConfigurationService configService,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        _logger.LogInformation("Health check requested");

        var healthStatus = new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        };

        try
        {
            // Check database connectivity
            await _dbContext.Database.CanConnectAsync();
            healthStatus.Database = "Connected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed");
            healthStatus.Status = "Unhealthy";
            healthStatus.Database = "Disconnected";
        }

        // Validate configuration
        var (isConfigValid, configErrors) = _configService.ValidateConfiguration();
        healthStatus.Configuration = isConfigValid ? "Valid" : "Invalid";
        if (!isConfigValid)
        {
            healthStatus.ConfigurationErrors = configErrors;
            if (healthStatus.Status == "Healthy")
                healthStatus.Status = "Degraded";
        }

        return Ok(new { success = true, data = healthStatus });
    }

    /// <summary>
    /// Detailed system information
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetSystemInfo()
    {
        _logger.LogInformation("System info requested");

        var systemInfo = new
        {
            success = true,
            data = new
            {
                application = "YouTube Shorts Automator",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                timestamp = DateTime.UtcNow,
                runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                osVersion = Environment.OSVersion,
                processorCount = Environment.ProcessorCount,
                enabledFeatures = _configService.GetEnabledFeatures()
            }
        };

        return Ok(systemInfo);
    }

    /// <summary>
    /// Readiness probe for Kubernetes
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return Ok(new { ready = true });
        }
        catch
        {
            return StatusCode(503, new { ready = false });
        }
    }

    /// <summary>
    /// Liveness probe for Kubernetes
    /// </summary>
    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        return Ok(new { alive = true });
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public List<string>? ConfigurationErrors { get; set; }
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}
