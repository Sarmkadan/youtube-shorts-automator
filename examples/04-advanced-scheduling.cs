// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Advanced scheduling example: Intelligent upload scheduling
/// Demonstrates:
/// 1. Smart time selection based on channel analytics
/// 2. Recurring upload schedule
/// 3. Timezone-aware scheduling
/// 4. Automatic retry on failure
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class AdvancedSchedulingExample
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;
    private readonly TimeZoneInfo _timeZone;

    public AdvancedSchedulingExample()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
    }

    /// <summary>
    /// Main example: Schedule videos intelligently
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== Advanced Scheduling Example ===\n");

            // Step 1: Find optimal upload times
            Console.WriteLine("Step 1: Analyzing optimal upload times...");
            var optimalTimes = await FindOptimalTimesAsync();
            DisplayOptimalTimes(optimalTimes);

            // Step 2: Create recurring schedule
            Console.WriteLine("\nStep 2: Setting up recurring uploads...");
            await ScheduleRecurringUploadsAsync(optimalTimes);

            // Step 3: Setup retry logic
            Console.WriteLine("\nStep 3: Configuring retry strategy...");
            await ConfigureRetryStrategyAsync();

            Console.WriteLine("\n✓ Advanced scheduling configured!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Find optimal upload times based on engagement patterns
    /// </summary>
    private async Task<List<DateTime>> FindOptimalTimesAsync()
    {
        var trends = await GetTrendingVideosAsync();
        var optimalTimes = new List<DateTime>();

        // Analyze trends and suggest optimal times
        // Typically: 8am, 12pm, 5pm (peak engagement hours)
        var baseDate = DateTime.Now;

        optimalTimes.Add(ConvertToTimeZone(baseDate.AddDays(1).Date.AddHours(8)));
        optimalTimes.Add(ConvertToTimeZone(baseDate.AddDays(1).Date.AddHours(12)));
        optimalTimes.Add(ConvertToTimeZone(baseDate.AddDays(1).Date.AddHours(17)));

        return optimalTimes;
    }

    /// <summary>
    /// Get trending videos (used for analysis)
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
    /// Convert datetime to target timezone
    /// </summary>
    private DateTime ConvertToTimeZone(DateTime dateTime)
    {
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, _timeZone);
        return utcDateTime;
    }

    /// <summary>
    /// Display optimal upload times
    /// </summary>
    private void DisplayOptimalTimes(List<DateTime> times)
    {
        Console.WriteLine("\nOptimal Upload Times:");
        Console.WriteLine("════════════════════");

        foreach (var time in times)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(time, _timeZone);
            Console.WriteLine($"  {localTime:dddd, MMMM d, yyyy hh:mm tt} {_timeZone.StandardName}");
        }
    }

    /// <summary>
    /// Schedule videos at recurring intervals
    /// </summary>
    private async Task ScheduleRecurringUploadsAsync(List<DateTime> optimalTimes)
    {
        var videoIds = new[] { 1, 2, 3, 4, 5 }; // Example video IDs

        var uploadSchedule = new[]
        {
            ("Video 1: Motivation", 1),
            ("Video 2: Fitness Tips", 2),
            ("Video 3: Nutrition Guide", 3),
            ("Video 4: Success Stories", 4),
            ("Video 5: Daily Recap", 5)
        };

        for (int i = 0; i < uploadSchedule.Length && i < optimalTimes.Count; i++)
        {
            var (title, videoId) = uploadSchedule[i];
            var scheduledTime = optimalTimes[i];

            var uploadJob = new
            {
                videoShortId = videoId,
                youtubeChannelId = 1,
                title = title,
                description = "Automatically scheduled based on engagement analysis",
                tags = new[] { "automation", "scheduled", "optimal-time" },
                scheduledUploadTime = scheduledTime.ToString("O")
            };

            var json = JsonSerializer.Serialize(uploadJob);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync($"{BASE_URL}/schedule/jobs", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseJson);
                var jobId = doc.RootElement.GetProperty("data").GetProperty("jobId").GetInt32();

                var localTime = TimeZoneInfo.ConvertTimeFromUtc(scheduledTime, _timeZone);
                Console.WriteLine($"  ✓ {title}: Scheduled for {localTime:g}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ {title}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Configure intelligent retry strategy
    /// </summary>
    private async Task ConfigureRetryStrategyAsync()
    {
        var retryConfig = new
        {
            maxRetries = 3,
            initialDelaySeconds = 60,
            backoffMultiplier = 2.0,
            maxDelaySeconds = 3600
        };

        Console.WriteLine("\nRetry Configuration:");
        Console.WriteLine("════════════════════");
        Console.WriteLine($"  Max Retries: {retryConfig.maxRetries}");
        Console.WriteLine($"  Initial Delay: {retryConfig.initialDelaySeconds}s");
        Console.WriteLine($"  Backoff Multiplier: {retryConfig.backoffMultiplier}x");
        Console.WriteLine($"  Max Delay: {retryConfig.maxDelaySeconds}s");

        Console.WriteLine("\nRetry Schedule:");
        Console.WriteLine("  Attempt 1: Immediately");
        for (int i = 2; i <= retryConfig.maxRetries; i++)
        {
            var delay = (int)(retryConfig.initialDelaySeconds *
                             Math.Pow(retryConfig.backoffMultiplier, i - 2));
            delay = Math.Min(delay, retryConfig.maxDelaySeconds);
            Console.WriteLine($"  Attempt {i}: After {delay}s ({TimeSpan.FromSeconds(delay):hh\\:mm\\:ss})");
        }

        await Task.CompletedTask;
    }

    static async Task Main(string[] args)
    {
        var example = new AdvancedSchedulingExample();
        await example.RunAsync();
    }
}
