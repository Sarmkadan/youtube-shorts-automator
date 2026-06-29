// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Advanced usage: configuration, options, and error handling
public class AdvancedUsage
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public AdvancedUsage()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Advanced Usage: Scheduled Upload with Config ===");

        try
        {
            // Custom scheduling options
            var jobRequest = new
            {
                VideoShortId = 1,
                Title = "Advanced Automation",
                Description = "This is an automated upload with custom config.",
                Tags = new List<string> { "dotnet", "shorts", "automation" },
                ScheduledUploadTime = DateTime.UtcNow.AddHours(24),
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
            Console.WriteLine($"✗ Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Unexpected error: {ex.Message}");
        }
    }

    static async Task Main(string[] args)
    {
        var example = new AdvancedUsage();
        await example.RunAsync();
    }
}
