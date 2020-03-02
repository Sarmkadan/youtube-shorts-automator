// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace YouTubeShortsAutomator.Metrics;

/// <summary>
/// Collects application metrics and performance data
/// Tracks requests, processing times, error rates, and resource usage
/// </summary>
public interface IMetricsCollector
{
    void RecordProcessingDuration(string processType, TimeSpan duration);
    void RecordUploadSuccess(long fileSizeBytes, TimeSpan duration);
    void RecordUploadFailure(string errorCode);
    void RecordApiCall(string endpoint, int statusCode, TimeSpan duration);
    Task<MetricsSnapshot> GetMetricsAsync();
}

public class MetricsCollector : IMetricsCollector
{
    private readonly Dictionary<string, ProcessingMetric> _processingMetrics;
    private readonly Dictionary<string, int> _errorCounts;
    private readonly List<ApiCallMetric> _apiMetrics;
    private readonly object _lockObj = new();
    private readonly ILogger<MetricsCollector> _logger;

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
        _processingMetrics = new Dictionary<string, ProcessingMetric>();
        _errorCounts = new Dictionary<string, int>();
        _apiMetrics = new List<ApiCallMetric>();
    }

    public void RecordProcessingDuration(string processType, TimeSpan duration)
    {
        try
        {
            lock (_lockObj)
            {
                if (!_processingMetrics.ContainsKey(processType))
                {
                    _processingMetrics[processType] = new ProcessingMetric { ProcessType = processType };
                }

                var metric = _processingMetrics[processType];
                metric.Count++;
                metric.TotalDurationMs += duration.TotalMilliseconds;
                metric.LastRecordedUtc = DateTime.UtcNow;

                if (duration.TotalMilliseconds < metric.MinDurationMs || metric.MinDurationMs == 0)
                    metric.MinDurationMs = duration.TotalMilliseconds;

                if (duration.TotalMilliseconds > metric.MaxDurationMs)
                    metric.MaxDurationMs = duration.TotalMilliseconds;

                _logger.LogDebug("Recorded processing duration. ProcessType: {ProcessType}, DurationMs: {DurationMs}, Count: {Count}",
                    processType, duration.TotalMilliseconds, metric.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording processing duration. ProcessType: {ProcessType}", processType);
        }
    }

    public void RecordUploadSuccess(long fileSizeBytes, TimeSpan duration)
    {
        try
        {
            lock (_lockObj)
            {
                const string key = "uploads_success";
                if (!_processingMetrics.ContainsKey(key))
                {
                    _processingMetrics[key] = new ProcessingMetric { ProcessType = key };
                }

                var metric = _processingMetrics[key];
                metric.Count++;
                metric.TotalDurationMs += duration.TotalMilliseconds;
                metric.TotalBytesProcessed += fileSizeBytes;
                metric.LastRecordedUtc = DateTime.UtcNow;

                _logger.LogInformation("Recorded upload success. FileSizeBytes: {FileSizeBytes}, DurationMs: {DurationMs}, TotalCount: {Count}",
                    fileSizeBytes, duration.TotalMilliseconds, metric.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording upload success. FileSizeBytes: {FileSizeBytes}", fileSizeBytes);
        }
    }

    public void RecordUploadFailure(string errorCode)
    {
        try
        {
            lock (_lockObj)
            {
                if (!_errorCounts.ContainsKey(errorCode))
                    _errorCounts[errorCode] = 0;

                _errorCounts[errorCode]++;

                _logger.LogWarning("Recorded upload failure. ErrorCode: {ErrorCode}, Count: {Count}",
                    errorCode, _errorCounts[errorCode]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording upload failure. ErrorCode: {ErrorCode}", errorCode);
        }
    }

    public void RecordApiCall(string endpoint, int statusCode, TimeSpan duration)
    {
        try
        {
            lock (_lockObj)
            {
                _apiMetrics.Add(new ApiCallMetric
                {
                    Endpoint = endpoint,
                    StatusCode = statusCode,
                    DurationMs = duration.TotalMilliseconds,
                    RecordedAtUtc = DateTime.UtcNow
                });

                // Keep only last 1000 API calls in memory
                if (_apiMetrics.Count > 1000)
                {
                    _apiMetrics.RemoveRange(0, _apiMetrics.Count - 1000);
                }

                if (statusCode >= 400)
                {
                    _logger.LogWarning("API call recorded with error status. Endpoint: {Endpoint}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                        endpoint, statusCode, duration.TotalMilliseconds);
                }
                else
                {
                    _logger.LogDebug("API call recorded. Endpoint: {Endpoint}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                        endpoint, statusCode, duration.TotalMilliseconds);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording API call. Endpoint: {Endpoint}, StatusCode: {StatusCode}",
                endpoint, statusCode);
        }
    }

    public async Task<MetricsSnapshot> GetMetricsAsync()
    {
        try
        {
            lock (_lockObj)
            {
                _logger.LogDebug("Retrieving metrics snapshot. TotalMetrics: {TotalMetrics}, TotalErrors: {TotalErrors}, TotalApiCalls: {TotalApiCalls}",
                    _processingMetrics.Count, _errorCounts.Count, _apiMetrics.Count);

                var snapshot = new MetricsSnapshot
                {
                    CapturedAtUtc = DateTime.UtcNow,
                    ProcessingMetrics = _processingMetrics.Values.ToList(),
                    ErrorCounts = _errorCounts,
                    ApiCallMetrics = _apiMetrics.TakeLast(100).ToList(),
                    TotalApiCalls = _apiMetrics.Count
                };

                return snapshot;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics snapshot");
            throw;
        }
    }
}

public class MetricsSnapshot
{
    public DateTime CapturedAtUtc { get; set; }
    public List<ProcessingMetric> ProcessingMetrics { get; set; } = new();
    public Dictionary<string, int> ErrorCounts { get; set; } = new();
    public List<ApiCallMetric> ApiCallMetrics { get; set; } = new();
    public int TotalApiCalls { get; set; }
}

public class ProcessingMetric
{
    public string ProcessType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double TotalDurationMs { get; set; }
    public double MinDurationMs { get; set; }
    public double MaxDurationMs { get; set; }
    public long TotalBytesProcessed { get; set; }
    public DateTime LastRecordedUtc { get; set; }

    public double AverageDurationMs => Count > 0 ? TotalDurationMs / Count : 0;
}

public class ApiCallMetric
{
    public string Endpoint { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public double DurationMs { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}
