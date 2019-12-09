// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Basic example: Upload and schedule a video for YouTube
/// This example demonstrates the fundamental workflow:
/// 1. Upload a local video file
/// 2. Create a processing profile
/// 3. Schedule upload to YouTube
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class BasicUploadExample
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public BasicUploadExample()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Main example: Upload video and schedule
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== Basic YouTube Upload Example ===\n");

            // Step 1: Create processing profile
            Console.WriteLine("Step 1: Creating processing profile...");
            var profileId = await CreateProcessingProfileAsync();
            Console.WriteLine($"✓ Profile created with ID: {profileId}\n");

            // Step 2: Upload video
            Console.WriteLine("Step 2: Uploading video file...");
            var videoId = await UploadVideoAsync("sample_video.mp4", profileId);
            Console.WriteLine($"✓ Video uploaded with ID: {videoId}\n");

            // Step 3: Schedule upload to YouTube
            Console.WriteLine("Step 3: Scheduling YouTube upload...");
            var jobId = await ScheduleUploadAsync(videoId, "My First Short Video");
            Console.WriteLine($"✓ Upload scheduled with Job ID: {jobId}\n");

            // Step 4: Monitor progress
            Console.WriteLine("Step 4: Monitoring upload progress...");
            await MonitorUploadAsync(jobId);

            Console.WriteLine("\n✓ Example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new processing profile for video encoding
    /// </summary>
    private async Task<int> CreateProcessingProfileAsync()
    {
        var profile = new
        {
            name = "HD Quality",
            videoWidth = 1080,
            videoHeight = 1920,
            videoBitrate = 4000,
            frameRate = 30
        };

        var json = JsonSerializer.Serialize(profile);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{BASE_URL}/processing/profiles", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    /// <summary>
    /// Upload a video file to the processing system
    /// </summary>
    private async Task<int> UploadVideoAsync(string videoPath, int profileId)
    {
        if (!File.Exists(videoPath))
            throw new FileNotFoundException($"Video file not found: {videoPath}");

        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(videoPath);
        form.Add(new StreamContent(fileStream), "videoFile", Path.GetFileName(videoPath));
        form.Add(new StringContent(profileId.ToString()), "profileId");

        var response = await _client.PostAsync($"{BASE_URL}/processing/videos", form);
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("data").GetProperty("videoId").GetInt32();
    }

    /// <summary>
    /// Schedule video upload to YouTube at specified time
    /// </summary>
    private async Task<int> ScheduleUploadAsync(int videoId, string title)
    {
        var uploadJob = new
        {
            videoShortId = videoId,
            youtubeChannelId = 1,
            title = title,
            description = "Uploaded using YouTube Shorts Automator",
            tags = new[] { "automation", "youtube", "shorts" },
            scheduledUploadTime = DateTime.UtcNow.AddHours(1).ToString("O")
        };

        var json = JsonSerializer.Serialize(uploadJob);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{BASE_URL}/schedule/jobs", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement.GetProperty("data").GetProperty("jobId").GetInt32();
    }

    /// <summary>
    /// Monitor upload job progress until completion
    /// </summary>
    private async Task MonitorUploadAsync(int jobId)
    {
        while (true)
        {
            var response = await _client.GetAsync($"{BASE_URL}/schedule/jobs/{jobId}");
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var status = doc.RootElement.GetProperty("data").GetProperty("status").GetString();

            Console.WriteLine($"  Status: {status}");

            if (status == "Uploaded" || status == "Failed")
                break;

            await Task.Delay(5000); // Check every 5 seconds
        }
    }

    static async Task Main(string[] args)
    {
        var example = new BasicUploadExample();
        await example.RunAsync();
    }
}
