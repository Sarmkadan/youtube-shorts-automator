// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    public MetricsCollector()
    {
        _processingMetrics = new Dictionary<string, ProcessingMetric>();
        _errorCounts = new Dictionary<string, int>();
        _apiMetrics = new List<ApiCallMetric>();
    }

    public void RecordProcessingDuration(string processType, TimeSpan duration)
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
        }
    }

    public void RecordUploadSuccess(long fileSizeBytes, TimeSpan duration)
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
        }
    }

    public void RecordUploadFailure(string errorCode)
    {
        lock (_lockObj)
        {
            if (!_errorCounts.ContainsKey(errorCode))
                _errorCounts[errorCode] = 0;

            _errorCounts[errorCode]++;
        }
    }

    public void RecordApiCall(string endpoint, int statusCode, TimeSpan duration)
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
        }
    }

    public async Task<MetricsSnapshot> GetMetricsAsync()
    {
        lock (_lockObj)
        {
            var snapshot = new MetricsSnapshot
            {
                CapturedAtUtc = DateTime.UtcNow,
                ProcessingMetrics = _processingMetrics.Values.ToList(),
                ErrorCounts = _errorCounts,
                ApiCallMetrics = _apiMetrics.TakeLast(100).ToList(),
                TotalApiCalls = _apiMetrics.Count
            };

            return Task.FromResult(snapshot).Result;
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
