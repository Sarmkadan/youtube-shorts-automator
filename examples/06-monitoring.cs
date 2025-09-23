// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Monitoring example: System health and performance tracking
/// Demonstrates:
/// 1. Health status monitoring
/// 2. System metrics collection
/// 3. Performance analysis
/// 4. Alert generation
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class MonitoringExample
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public MonitoringExample()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Main example: Monitor system health
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== Monitoring Example ===\n");

            // Run monitoring for 1 minute
            var startTime = DateTime.Now;
            var duration = TimeSpan.FromMinutes(1);

            Console.WriteLine($"Monitoring for {duration.TotalSeconds}s...\n");

            var healthHistory = new List<HealthSnapshot>();
            var metricsHistory = new List<MetricsSnapshot>();

            while (DateTime.Now - startTime < duration)
            {
                // Collect health status
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Collecting metrics...");

                var health = await GetHealthStatusAsync();
                healthHistory.Add(health);

                var metrics = await GetSystemMetricsAsync();
                metricsHistory.Add(metrics);

                // Display current status
                DisplayHealthStatus(health);
                DisplayMetrics(metrics);

                // Check for alerts
                CheckForAlerts(health, metrics);

                Console.WriteLine();

                // Wait 20 seconds before next check
                await Task.Delay(TimeSpan.FromSeconds(20));
            }

            // Generate summary report
            Console.WriteLine("\n=== Monitoring Summary ===\n");
            GenerateSummaryReport(healthHistory, metricsHistory);

            Console.WriteLine("\n✓ Monitoring complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get health status snapshot
    /// </summary>
    private async Task<HealthSnapshot> GetHealthStatusAsync()
    {
        try
        {
            var response = await _client.GetAsync($"{BASE_URL}/health");
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var data = doc.RootElement.GetProperty("data");

            return new HealthSnapshot
            {
                Timestamp = DateTime.Now,
                Status = data.TryGetProperty("status", out var status) ?
                    status.GetString() ?? "unknown" : "unknown",
                Uptime = data.TryGetProperty("uptime", out var uptime) ?
                    uptime.GetString() ?? "unknown" : "unknown"
            };
        }
        catch
        {
            return new HealthSnapshot
            {
                Timestamp = DateTime.Now,
                Status = "unreachable",
                Uptime = "N/A"
            };
        }
    }

    /// <summary>
    /// Get system metrics
    /// </summary>
    private async Task<MetricsSnapshot> GetSystemMetricsAsync()
    {
        try
        {
            var response = await _client.GetAsync($"{BASE_URL}/metrics");
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var data = doc.RootElement.GetProperty("data");

            return new MetricsSnapshot
            {
                Timestamp = DateTime.Now,
                CpuUsagePercent = data.TryGetProperty("cpuUsagePercent", out var cpu) ?
                    cpu.GetDouble() : 0,
                MemoryUsageMB = data.TryGetProperty("memoryUsageMB", out var mem) ?
                    mem.GetInt32() : 0,
                AvailableMemoryMB = data.TryGetProperty("availableMemoryMB", out var available) ?
                    available.GetInt32() : 0,
                ProcessingQueueCount = data.TryGetProperty("processingQueueCount", out var queue) ?
                    queue.GetInt32() : 0,
                UploadQueueCount = data.TryGetProperty("uploadQueueCount", out var upload) ?
                    upload.GetInt32() : 0,
                FailedJobsCount = data.TryGetProperty("failedJobsCount", out var failed) ?
                    failed.GetInt32() : 0
            };
        }
        catch
        {
            return new MetricsSnapshot { Timestamp = DateTime.Now };
        }
    }

    /// <summary>
    /// Display health status
    /// </summary>
    private void DisplayHealthStatus(HealthSnapshot health)
    {
        var statusIcon = health.Status switch
        {
            "healthy" => "✓",
            "degraded" => "⚠",
            _ => "✗"
        };

        Console.WriteLine($"  {statusIcon} Status: {health.Status}");
        Console.WriteLine($"    Uptime: {health.Uptime}");
    }

    /// <summary>
    /// Display system metrics
    /// </summary>
    private void DisplayMetrics(MetricsSnapshot metrics)
    {
        Console.WriteLine($"  CPU: {metrics.CpuUsagePercent:F1}%");
        Console.WriteLine($"  Memory: {metrics.MemoryUsageMB}MB / {metrics.AvailableMemoryMB}MB");
        Console.WriteLine($"  Processing Queue: {metrics.ProcessingQueueCount} jobs");
        Console.WriteLine($"  Upload Queue: {metrics.UploadQueueCount} jobs");
        if (metrics.FailedJobsCount > 0)
            Console.WriteLine($"  ⚠ Failed Jobs: {metrics.FailedJobsCount}");
    }

    /// <summary>
    /// Check for alerts based on thresholds
    /// </summary>
    private void CheckForAlerts(HealthSnapshot health, MetricsSnapshot metrics)
    {
        var alerts = new List<string>();

        // Health alerts
        if (health.Status == "unhealthy")
            alerts.Add("System unhealthy");
        if (health.Status == "degraded")
            alerts.Add("System degraded");

        // Resource alerts
        if (metrics.CpuUsagePercent > 80)
            alerts.Add($"High CPU usage: {metrics.CpuUsagePercent:F1}%");

        if (metrics.AvailableMemoryMB > 0)
        {
            var memoryUsagePercent = (double)metrics.MemoryUsageMB / (metrics.MemoryUsageMB + metrics.AvailableMemoryMB) * 100;
            if (memoryUsagePercent > 85)
                alerts.Add($"High memory usage: {memoryUsagePercent:F1}%");
        }

        // Queue alerts
        if (metrics.ProcessingQueueCount > 50)
            alerts.Add($"Large processing queue: {metrics.ProcessingQueueCount} jobs");

        if (metrics.FailedJobsCount > 5)
            alerts.Add($"Multiple failed jobs: {metrics.FailedJobsCount}");

        // Display alerts
        if (alerts.Any())
        {
            Console.WriteLine("\n  ⚠ Alerts:");
            foreach (var alert in alerts)
                Console.WriteLine($"    - {alert}");
        }
    }

    /// <summary>
    /// Generate summary report
    /// </summary>
    private void GenerateSummaryReport(List<HealthSnapshot> healthHistory,
                                      List<MetricsSnapshot> metricsHistory)
    {
        if (healthHistory.Count == 0 || metricsHistory.Count == 0)
        {
            Console.WriteLine("No data collected");
            return;
        }

        // Health summary
        var healthyCount = healthHistory.Count(h => h.Status == "healthy");
        var degradedCount = healthHistory.Count(h => h.Status == "degraded");
        var unhealthyCount = healthHistory.Count(h => h.Status == "unhealthy");

        Console.WriteLine("Health Status Summary:");
        Console.WriteLine($"  Healthy: {healthyCount}/{healthHistory.Count} ({healthyCount * 100 / healthHistory.Count}%)");
        if (degradedCount > 0)
            Console.WriteLine($"  Degraded: {degradedCount}/{healthHistory.Count}");
        if (unhealthyCount > 0)
            Console.WriteLine($"  Unhealthy: {unhealthyCount}/{healthHistory.Count}");

        // Metrics summary
        var avgCpu = metricsHistory.Average(m => m.CpuUsagePercent);
        var maxCpu = metricsHistory.Max(m => m.CpuUsagePercent);
        var avgMemory = metricsHistory.Average(m => m.MemoryUsageMB);
        var maxMemory = metricsHistory.Max(m => m.MemoryUsageMB);

        Console.WriteLine("\nResource Usage Summary:");
        Console.WriteLine($"  CPU Average: {avgCpu:F1}%");
        Console.WriteLine($"  CPU Peak: {maxCpu:F1}%");
        Console.WriteLine($"  Memory Average: {avgMemory:F0}MB");
        Console.WriteLine($"  Memory Peak: {maxMemory:F0}MB");

        // Queue summary
        var avgProcessingQueue = metricsHistory.Average(m => m.ProcessingQueueCount);
        var maxProcessingQueue = metricsHistory.Max(m => m.ProcessingQueueCount);
        var totalFailedJobs = metricsHistory.Sum(m => m.FailedJobsCount);

        Console.WriteLine("\nQueue Summary:");
        Console.WriteLine($"  Processing Queue Average: {avgProcessingQueue:F1} jobs");
        Console.WriteLine($"  Processing Queue Peak: {maxProcessingQueue} jobs");
        Console.WriteLine($"  Total Failed Jobs: {totalFailedJobs}");

        // Recommendations
        Console.WriteLine("\nRecommendations:");
        if (avgCpu > 70)
            Console.WriteLine("  - Consider increasing server resources or reducing concurrent jobs");
        if (avgMemory > 512)
            Console.WriteLine("  - Monitor memory usage; consider increasing RAM");
        if (maxProcessingQueue > 30)
            Console.WriteLine("  - Process queue is high; consider increasing MaxConcurrentProcessing");
        if (totalFailedJobs > 5)
            Console.WriteLine("  - Multiple job failures detected; check logs for issues");
    }

    // Helper classes
    private class HealthSnapshot
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
    }

    private class MetricsSnapshot
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsagePercent { get; set; }
        public int MemoryUsageMB { get; set; }
        public int AvailableMemoryMB { get; set; }
        public int ProcessingQueueCount { get; set; }
        public int UploadQueueCount { get; set; }
        public int FailedJobsCount { get; set; }
    }

    static async Task Main(string[] args)
    {
        var example = new MonitoringExample();
        await example.RunAsync();
    }
}
