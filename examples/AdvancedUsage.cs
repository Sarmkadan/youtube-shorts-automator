// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

/// <summary>
/// Demonstrates advanced usage of the YouTube Shorts Automator API.
/// </summary>
public class AdvancedUsage
{
    /// <summary>
    /// The base URL of the API.
    /// </summary>
    private const string BASE_URL = "http://localhost:5000/api";

    /// <summary>
    /// The API key used for authentication.
    /// </summary>
    private const string API_KEY = "your-api-key-here";

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedUsage"/> class.
    /// </summary>
    public AdvancedUsage()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Runs the advanced usage example.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        Console.WriteLine("=== Advanced Usage: Scheduled Upload with Config ===");

        try
        {
            // Custom scheduling options
            var jobRequest = new
            {
                /// <summary>
                /// The ID of the video short to upload.
                /// </summary>
                VideoShortId = 1,
                /// <summary>
                /// The title of the video.
                /// </summary>
                Title = "Advanced Automation",
                /// <summary>
                /// The description of the video.
                /// </summary>
                Description = "This is an automated upload with custom config.",
                /// <summary>
                /// The tags associated with the video.
                /// </summary>
                Tags = new List<string> { "dotnet", "shorts", "automation" },
                /// <summary>
                /// The scheduled upload time.
                /// </summary>
                ScheduledUploadTime = DateTime.UtcNow.AddHours(24),
                /// <summary>
                /// The retry strategy to use.
                /// </summary>
                RetryStrategy = "ExponentialBackoff"
            };

            Console.WriteLine("Scheduling job...");
            var response = await _client.PostAsJsonAsync($"{BASE_URL}/schedule/jobs", jobRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var job = await response.Content.ReadFromJsonAsync<dynamic>();
                Console.WriteLine($"✓ Job scheduled successfully: {job}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✗ Scheduling failed: {response.StatusCode} - {error}");
            }
        }
        catch (HttpRequestException ex)
        {
            /// <summary>
            /// The network error that occurred.
            /// </summary>
            Console.WriteLine($"✗ Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            /// <summary>
            /// The unexpected error that occurred.
            /// </summary>
            Console.WriteLine($"✗ Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// The HTTP client used for API requests.
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The static main entry point of the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static async Task Main(string[] args)
    {
        var example = new AdvancedUsage();
        await example.RunAsync();
    }
}
