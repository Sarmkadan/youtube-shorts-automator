// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Error handling example: Robust error management
/// Demonstrates:
/// 1. Exception handling strategies
/// 2. Retry mechanisms
/// 3. Fallback operations
/// 4. Graceful degradation
/// </summary>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class ErrorHandlingExample
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public ErrorHandlingExample()
    {
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Main example: Demonstrate error handling patterns
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== Error Handling Example ===\n");

            // Example 1: Handle missing resources
            Console.WriteLine("Example 1: Handling missing resources...");
            await HandleMissingResourceAsync();

            // Example 2: Retry with exponential backoff
            Console.WriteLine("\nExample 2: Retry with exponential backoff...");
            await RetryWithBackoffAsync();

            // Example 3: Graceful degradation
            Console.WriteLine("\nExample 3: Graceful degradation...");
            await GracefulDegradationAsync();

            // Example 4: Error aggregation
            Console.WriteLine("\nExample 4: Processing multiple items with error aggregation...");
            await ProcessMultipleWithErrorAggregationAsync();

            Console.WriteLine("\n✓ Example completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Unhandled error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle case where resource doesn't exist
    /// </summary>
    private async Task HandleMissingResourceAsync()
    {
        try
        {
            var response = await _client.GetAsync($"{BASE_URL}/schedule/jobs/999999");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("  Job not found - creating new job instead");
                // Fallback: Create new job instead
                await CreateNewJobAsync();
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"  Network error: {ex.Message}");
            Console.WriteLine("  Retrying with fallback server...");
        }
    }

    /// <summary>
    /// Implement retry with exponential backoff
    /// </summary>
    private async Task RetryWithBackoffAsync()
    {
        const int maxRetries = 3;
        int attempt = 0;
        int delayMs = 1000;

        while (attempt < maxRetries)
        {
            try
            {
                attempt++;
                Console.WriteLine($"  Attempt {attempt}/{maxRetries}...");

                // Simulate operation that might fail
                var response = await _client.GetAsync($"{BASE_URL}/health");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"  ✓ Success on attempt {attempt}");
                    return;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                {
                    throw new Exception("Service temporarily unavailable");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error: {ex.Message}");

                if (attempt >= maxRetries)
                {
                    Console.WriteLine($"  ✗ Failed after {maxRetries} attempts");
                    throw;
                }

                Console.WriteLine($"  Waiting {delayMs}ms before retry...");
                await Task.Delay(delayMs);
                delayMs *= 2; // Exponential backoff
            }
        }
    }

    /// <summary>
    /// Demonstrate graceful degradation
    /// </summary>
    private async Task GracefulDegradationAsync()
    {
        try
        {
            Console.WriteLine("  Attempting to fetch analytics...");
            var analytics = await FetchAnalyticsAsync();
            Console.WriteLine("  ✓ Analytics retrieved");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠ Analytics unavailable: {ex.Message}");
            Console.WriteLine("  Falling back to cached analytics...");
            var cachedAnalytics = GetCachedAnalytics();
            Console.WriteLine("  ✓ Using cached data");
        }
    }

    /// <summary>
    /// Process multiple items and aggregate errors
    /// </summary>
    private async Task ProcessMultipleWithErrorAggregationAsync()
    {
        var videoIds = new[] { 1, 2, 3, 999, 5 }; // 999 doesn't exist
        var results = new List<(int id, bool success, string? error)>();

        foreach (var videoId in videoIds)
        {
            try
            {
                Console.WriteLine($"  Processing video {videoId}...");
                var response = await _client.GetAsync($"{BASE_URL}/analytics/videos/{videoId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"    ✓ Success");
                    results.Add((videoId, true, null));
                }
                else
                {
                    var error = $"HTTP {response.StatusCode}";
                    Console.WriteLine($"    ✗ Failed: {error}");
                    results.Add((videoId, false, error));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                results.Add((videoId, false, ex.Message));
            }
        }

        // Summary
        Console.WriteLine("\n  Processing Summary:");
        var successful = results.FindAll(r => r.success).Count;
        var failed = results.FindAll(r => !r.success).Count;
        Console.WriteLine($"    Successful: {successful}/{videoIds.Length}");
        Console.WriteLine($"    Failed: {failed}/{videoIds.Length}");

        if (failed > 0)
        {
            Console.WriteLine("\n  Failed Items:");
            foreach (var (id, _, error) in results.FindAll(r => !r.success))
            {
                Console.WriteLine($"    Video {id}: {error}");
            }
        }
    }

    /// <summary>
    /// Create new job as fallback
    /// </summary>
    private async Task<int> CreateNewJobAsync()
    {
        var uploadJob = new
        {
            videoShortId = 1,
            youtubeChannelId = 1,
            title = "Fallback Upload",
            description = "Created as fallback",
            tags = new[] { "fallback" },
            scheduledUploadTime = DateTime.UtcNow.AddHours(1).ToString("O")
        };

        var json = JsonSerializer.Serialize(uploadJob);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await _client.PostAsync($"{BASE_URL}/schedule/jobs", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                var jobId = doc.RootElement.GetProperty("data").GetProperty("jobId").GetInt32();
                Console.WriteLine($"  ✓ Created fallback job: {jobId}");
                return jobId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Could not create fallback: {ex.Message}");
        }

        return -1;
    }

    /// <summary>
    /// Fetch analytics (might fail)
    /// </summary>
    private async Task<JsonElement> FetchAnalyticsAsync()
    {
        var response = await _client.GetAsync($"{BASE_URL}/analytics/metrics");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Analytics service returned {response.StatusCode}");

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("data").Clone();
    }

    /// <summary>
    /// Get cached analytics (fallback)
    /// </summary>
    private JsonElement GetCachedAnalytics()
    {
        // Simulate cached data
        var cachedJson = """
            {
                "totalViews": 45000,
                "totalLikes": 2200,
                "totalComments": 450,
                "avgEngagementRate": 0.062
            }
            """;

        using var doc = JsonDocument.Parse(cachedJson);
        return doc.RootElement.Clone();
    }

    static async Task Main(string[] args)
    {
        var example = new ErrorHandlingExample();
        await example.RunAsync();
    }
}
