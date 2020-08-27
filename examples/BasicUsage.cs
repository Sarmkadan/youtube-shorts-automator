// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

/// <summary>
/// Demonstrates basic usage of the API to upload a video.
/// </summary>
public class BasicUsage
{
    /// <summary>
    /// The base URL of the API.
    /// </summary>
    private const string BASE_URL = "http://localhost:5000/api";

    /// <summary>
    /// The API key to use for authentication.
    /// </summary>
    private const string API_KEY = "your-api-key-here";

    /// <summary>
    /// The HTTP client instance used to make API requests.
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicUsage"/> class.
    /// </summary>
    public BasicUsage()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Runs the basic usage example.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        Console.WriteLine("=== Basic Usage: Upload Video ===");

        try
        {
            /// <summary>
            /// The payload for the video upload request.
            /// </summary>
            var videoPayload = new { FilePath = "sample_video.mp4" };

            Console.WriteLine("Uploading video...");
            /// <summary>
            /// Sends a POST request to the API to upload the video.
            /// </summary>
            /// <returns>The HTTP response from the API.</returns>
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

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static async Task Main(string[] args)
    {
        var example = new BasicUsage();
        await example.RunAsync();
    }
}
