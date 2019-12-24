// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Metrics;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// API endpoints for metrics and diagnostics
/// Provides system health, performance metrics, and usage statistics
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly ILogger<MetricsController> _logger;
    private readonly IMetricsCollector _metricsCollector;

    public MetricsController(
        ILogger<MetricsController> logger,
        IMetricsCollector metricsCollector)
    {
        _logger = logger;
        _metricsCollector = metricsCollector;
    }

    /// <summary>
    /// Get system metrics and performance data
    /// </summary>
    [HttpGet("system")]
    [ProducesResponseType(typeof(SystemMetricsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemMetricsAsync()
    {
        try
        {
            var snapshot = await _metricsCollector.GetMetricsAsync();

            var response = new SystemMetricsResponse
            {
                CapturedAtUtc = snapshot.CapturedAtUtc,
                TotalApiCalls = snapshot.TotalApiCalls,
                ProcessingMetrics = snapshot.ProcessingMetrics.Select(m => new ProcessingMetricResponse
                {
                    ProcessType = m.ProcessType,
                    Count = m.Count,
                    AverageDurationMs = m.AverageDurationMs,
                    MinDurationMs = m.MinDurationMs,
                    MaxDurationMs = m.MaxDurationMs,
                    TotalBytesProcessed = m.TotalBytesProcessed
                }).ToList(),
                ErrorCounts = snapshot.ErrorCounts
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system metrics");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get API call statistics
    /// </summary>
    [HttpGet("api-calls")]
    [ProducesResponseType(typeof(ApiCallsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiCallMetricsAsync([FromQuery] int limit = 20)
    {
        try
        {
            if (limit < 1 || limit > 100)
                limit = 20;

            var snapshot = await _metricsCollector.GetMetricsAsync();

            var grouped = snapshot.ApiCallMetrics
                .GroupBy(m => m.Endpoint)
                .Select(g => new ApiCallGroup
                {
                    Endpoint = g.Key,
                    CallCount = g.Count(),
                    AverageDurationMs = g.Average(m => m.DurationMs),
                    SuccessRate = ((double)g.Count(m => m.StatusCode >= 200 && m.StatusCode < 300) / g.Count()) * 100
                })
                .OrderByDescending(g => g.CallCount)
                .Take(limit)
                .ToList();

            var response = new ApiCallsResponse
            {
                TotalCalls = snapshot.TotalApiCalls,
                Groups = grouped
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API call metrics");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get error statistics
    /// </summary>
    [HttpGet("errors")]
    [ProducesResponseType(typeof(ErrorStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrorStatsAsync()
    {
        try
        {
            var snapshot = await _metricsCollector.GetMetricsAsync();

            var totalErrors = snapshot.ErrorCounts.Values.Sum();

            var response = new ErrorStatsResponse
            {
                TotalErrors = totalErrors,
                ErrorsByCode = snapshot.ErrorCounts.Select(kvp => new ErrorCount
                {
                    ErrorCode = kvp.Key,
                    Count = kvp.Value,
                    Percentage = totalErrors > 0 ? ((double)kvp.Value / totalErrors) * 100 : 0
                }).OrderByDescending(e => e.Count).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error statistics");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get processing performance metrics
    /// </summary>
    [HttpGet("processing")]
    [ProducesResponseType(typeof(ProcessingPerformanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessingPerformanceAsync()
    {
        try
        {
            var snapshot = await _metricsCollector.GetMetricsAsync();

            var response = new ProcessingPerformanceResponse
            {
                Metrics = snapshot.ProcessingMetrics.Select(m => new ProcessingMetricResponse
                {
                    ProcessType = m.ProcessType,
                    Count = m.Count,
                    AverageDurationMs = m.AverageDurationMs,
                    MinDurationMs = m.MinDurationMs,
                    MaxDurationMs = m.MaxDurationMs,
                    TotalBytesProcessed = m.TotalBytesProcessed
                }).ToList(),
                CapturedAtUtc = snapshot.CapturedAtUtc
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing performance metrics");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

#region Metrics Response Models

public class SystemMetricsResponse
{
    public DateTime CapturedAtUtc { get; set; }
    public int TotalApiCalls { get; set; }
    public List<ProcessingMetricResponse> ProcessingMetrics { get; set; } = new();
    public Dictionary<string, int> ErrorCounts { get; set; } = new();
}

public class ProcessingMetricResponse
{
    public string ProcessType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageDurationMs { get; set; }
    public double MinDurationMs { get; set; }
    public double MaxDurationMs { get; set; }
    public long TotalBytesProcessed { get; set; }
}

public class ApiCallsResponse
{
    public int TotalCalls { get; set; }
    public List<ApiCallGroup> Groups { get; set; } = new();
}

public class ApiCallGroup
{
    public string Endpoint { get; set; } = string.Empty;
    public int CallCount { get; set; }
    public double AverageDurationMs { get; set; }
    public double SuccessRate { get; set; }
}

public class ErrorStatsResponse
{
    public int TotalErrors { get; set; }
    public List<ErrorCount> ErrorsByCode { get; set; } = new();
}

public class ErrorCount
{
    public string ErrorCode { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ProcessingPerformanceResponse
{
    public List<ProcessingMetricResponse> Metrics { get; set; } = new();
    public DateTime CapturedAtUtc { get; set; }
}

#endregion
