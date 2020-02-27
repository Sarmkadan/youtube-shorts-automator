// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Batch processing example: Upload multiple videos concurrently
/// Demonstrates:
/// 1. Processing multiple videos in parallel
/// 2. Batch scheduling uploads
/// 3. Tracking multiple jobs
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class BatchProcessingExample
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public BatchProcessingExample()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    /// <summary>
    /// Main example: Process and upload multiple videos
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== Batch Processing Example ===\n");

            // Define videos to process
            var videosToProcess = new[]
            {
                (fileName: "video1.mp4", title: "Morning Motivation"),
                (fileName: "video2.mp4", title: "Workout Tips"),
                (fileName: "video3.mp4", title: "Healthy Recipes"),
                (fileName: "video4.mp4", title: "Evening Reflection")
            };

            Console.WriteLine($"Processing {videosToProcess.Length} videos...\n");

            // Get or create processing profile
            var profileId = await GetOrCreateProfileAsync();

            // Upload all videos in parallel
            Console.WriteLine("Uploading videos (parallel processing)...");
            var uploadTasks = videosToProcess.Select(v =>
                UploadVideoAsync(v.fileName, v.title, profileId)
            ).ToList();

            var uploadedVideos = await Task.WhenAll(uploadTasks);
            Console.WriteLine($"✓ All {uploadedVideos.Length} videos uploaded\n");

            // Schedule uploads with staggered timing
            Console.WriteLine("Scheduling uploads with time spacing...");
            var scheduleTasks = uploadedVideos
                .Select((vid, index) =>
                    ScheduleVideoAsync(vid, videosToProcess[index].title, index)
                ).ToList();

            var jobIds = await Task.WhenAll(scheduleTasks);
            Console.WriteLine($"✓ Scheduled {jobIds.Length} uploads\n");

            // Monitor all jobs
            Console.WriteLine("Monitoring all upload jobs...");
            await MonitorAllJobsAsync(jobIds);

            Console.WriteLine("\n✓ Batch processing completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get existing profile or create new one
    /// </summary>
    private async Task<int> GetOrCreateProfileAsync()
    {
        try
        {
            var response = await _client.GetAsync($"{BASE_URL}/processing/profiles");
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var profiles = doc.RootElement.GetProperty("data");

            if (profiles.GetArrayLength() > 0)
                return profiles[0].GetProperty("id").GetInt32();
        }
        catch { }

        // Create new profile if none exist
        var profile = new
        {
            name = "Batch Profile",
            videoWidth = 1080,
            videoHeight = 1920,
            videoBitrate = 3500,
            frameRate = 30
        };

        var json = JsonSerializer.Serialize(profile);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response2 = await _client.PostAsync($"{BASE_URL}/processing/profiles", content);
        var responseJson2 = await response2.Content.ReadAsStringAsync();

        using var doc2 = JsonDocument.Parse(responseJson2);
        return doc2.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    /// <summary>
    /// Upload a single video and return video ID
    /// </summary>
    private async Task<int> UploadVideoAsync(string videoPath, string title, int profileId)
    {
        if (!File.Exists(videoPath))
        {
            Console.WriteLine($"  ⚠ File not found: {videoPath}");
            return -1;
        }

        try
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(videoPath);
            form.Add(new StreamContent(fileStream), "videoFile", Path.GetFileName(videoPath));
            form.Add(new StringContent(profileId.ToString()), "profileId");

            var response = await _client.PostAsync($"{BASE_URL}/processing/videos", form);
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var videoId = doc.RootElement.GetProperty("data").GetProperty("videoId").GetInt32();

            Console.WriteLine($"  ✓ {title}: Video ID {videoId}");
            return videoId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ {title}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Schedule video with time staggering (e.g., one per hour)
    /// </summary>
    private async Task<int> ScheduleVideoAsync(int videoId, string title, int index)
    {
        if (videoId == -1)
            return -1;

        var scheduledTime = DateTime.UtcNow.AddHours(index + 1);

        var uploadJob = new
        {
            videoShortId = videoId,
            youtubeChannelId = 1,
            title = title,
            description = "Batch processed video",
            tags = new[] { "batch", "automated" },
            scheduledUploadTime = scheduledTime.ToString("O")
        };

        var json = JsonSerializer.Serialize(uploadJob);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{BASE_URL}/schedule/jobs", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);
        var jobId = doc.RootElement.GetProperty("data").GetProperty("jobId").GetInt32();

        Console.WriteLine($"  ✓ {title}: Scheduled for {scheduledTime:g} (Job ID: {jobId})");
        return jobId;
    }

    /// <summary>
    /// Monitor multiple jobs in parallel
    /// </summary>
    private async Task MonitorAllJobsAsync(int[] jobIds)
    {
        var jobStatuses = jobIds.ToDictionary(id => id, id => "Scheduled");

        while (jobStatuses.Values.Any(s => s != "Uploaded" && s != "Failed"))
        {
            Console.Clear();
            Console.WriteLine("=== Batch Processing Monitor ===\n");
            Console.WriteLine("Job Status:");

            foreach (var jobId in jobIds)
            {
                try
                {
                    var response = await _client.GetAsync($"{BASE_URL}/schedule/jobs/{jobId}");
                    var responseJson = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(responseJson);
                    var status = doc.RootElement.GetProperty("data").GetProperty("status").GetString();
                    var title = doc.RootElement.GetProperty("data").GetProperty("title").GetString();

                    jobStatuses[jobId] = status;
                    Console.WriteLine($"  [{jobId}] {title}: {status}");
                }
                catch { }
            }

            if (jobStatuses.Values.All(s => s == "Uploaded" || s == "Failed"))
                break;

            await Task.Delay(3000);
        }

        Console.WriteLine("\n✓ All jobs completed");
    }

    static async Task Main(string[] args)
    {
        var example = new BatchProcessingExample();
        await example.RunAsync();
    }
}
