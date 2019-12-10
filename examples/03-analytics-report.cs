// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Analytics example: Generate performance reports
/// Demonstrates:
/// 1. Retrieving video metrics
/// 2. Analyzing trending videos
/// 3. Generating CSV/JSON reports
/// 4. Performance comparison
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class AnalyticsReportExample
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public AnalyticsReportExample()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Main example: Generate analytics report
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== Analytics Report Example ===\n");

            // Step 1: Get overall metrics
            Console.WriteLine("Step 1: Retrieving overall metrics...");
            var metrics = await GetOverallMetricsAsync();
            DisplayMetrics(metrics);

            // Step 2: Get trending videos
            Console.WriteLine("\nStep 2: Analyzing trending videos...");
            var trends = await GetTrendingVideosAsync();
            DisplayTrends(trends);

            // Step 3: Generate CSV report
            Console.WriteLine("\nStep 3: Generating CSV report...");
            await ExportReportAsync("csv", "analytics_report.csv");

            // Step 4: Generate JSON report
            Console.WriteLine("Step 4: Generating JSON report...");
            await ExportReportAsync("json", "analytics_report.json");

            Console.WriteLine("\n✓ Reports generated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get overall metrics for date range
    /// </summary>
    private async Task<JsonElement> GetOverallMetricsAsync()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var url = $"{BASE_URL}/analytics/metrics?" +
                  $"startDate={startDate:yyyy-MM-dd}&" +
                  $"endDate={endDate:yyyy-MM-dd}";

        var response = await _client.GetAsync(url);
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("data").Clone();
    }

    /// <summary>
    /// Get trending videos for last 7 days
    /// </summary>
    private async Task<JsonElement[]> GetTrendingVideosAsync()
    {
        var response = await _client.GetAsync($"{BASE_URL}/analytics/trends?days=7&limit=10");
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(e => e.Clone())
            .ToArray();
    }

    /// <summary>
    /// Display metrics in formatted output
    /// </summary>
    private void DisplayMetrics(JsonElement metrics)
    {
        Console.WriteLine("\nOverall Metrics (Last 30 Days):");
        Console.WriteLine("════════════════════════════════");

        if (metrics.TryGetProperty("totalViews", out var views))
            Console.WriteLine($"  Total Views: {views.GetInt32():N0}");

        if (metrics.TryGetProperty("totalLikes", out var likes))
            Console.WriteLine($"  Total Likes: {likes.GetInt32():N0}");

        if (metrics.TryGetProperty("totalComments", out var comments))
            Console.WriteLine($"  Total Comments: {comments.GetInt32():N0}");

        if (metrics.TryGetProperty("totalShares", out var shares))
            Console.WriteLine($"  Total Shares: {shares.GetInt32():N0}");

        if (metrics.TryGetProperty("avgEngagementRate", out var engagement))
            Console.WriteLine($"  Avg Engagement Rate: {engagement.GetDouble():P2}");

        if (metrics.TryGetProperty("topPerformer", out var topPerformer))
        {
            Console.WriteLine("\n  Top Performer:");
            var title = topPerformer.GetProperty("title").GetString();
            var topViews = topPerformer.GetProperty("views").GetInt32();
            Console.WriteLine($"    {title}");
            Console.WriteLine($"    {topViews:N0} views");
        }
    }

    /// <summary>
    /// Display trending videos
    /// </summary>
    private void DisplayTrends(JsonElement[] trends)
    {
        Console.WriteLine("\nTrending Videos (Last 7 Days):");
        Console.WriteLine("════════════════════════════════");

        foreach (var trend in trends.Take(5))
        {
            if (!trend.TryGetProperty("rank", out var rank)) continue;
            if (!trend.TryGetProperty("title", out var title)) continue;
            if (!trend.TryGetProperty("views", out var views)) continue;

            var rankNum = rank.GetInt32();
            var titleStr = title.GetString();
            var viewCount = views.GetInt32();

            Console.WriteLine($"  #{rankNum}: {titleStr}");
            Console.WriteLine($"         {viewCount:N0} views");
        }
    }

    /// <summary>
    /// Export analytics data in specified format
    /// </summary>
    private async Task ExportReportAsync(string format, string filename)
    {
        var exportRequest = new
        {
            format = format,
            startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"),
            endDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            includeMetrics = new[] { "views", "likes", "comments", "shares", "engagement" },
            groupBy = "day"
        };

        var json = JsonSerializer.Serialize(exportRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{BASE_URL}/analytics/export", content);

        if (response.IsSuccessStatusCode)
        {
            var fileContent = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(filename, fileContent);
            Console.WriteLine($"  ✓ Exported to {filename} ({fileContent.Length:N0} bytes)");
        }
    }

    static async Task Main(string[] args)
    {
        var example = new AnalyticsReportExample();
        await example.RunAsync();
    }
}
