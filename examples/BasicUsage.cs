// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Basic usage of the API to upload a video
public class BasicUsage
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public BasicUsage()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Basic Usage: Upload Video ===");

        try
        {
            var videoPayload = new { FilePath = "sample_video.mp4" };
            
            Console.WriteLine("Uploading video...");
            var response = await _client.PostAsJsonAsync($"{BASE_URL}/videos/upload", videoPayload);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Upload successful!");
            }
            else
            {
                Console.WriteLine($"✗ Upload failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task Main(string[] args)
    {
        var example = new BasicUsage();
        await example.RunAsync();
    }
}
