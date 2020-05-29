// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for IntegrationTests that provide reusable test utilities
// and helper methods for common integration testing scenarios.
// =============================================================================

using System.Globalization;
using FluentAssertions;
using YouTubeShortAutomator.Configuration;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for <see cref="IntegrationTests"/> that add reusable test utilities
/// and helper methods for common integration testing scenarios.
/// </summary>
public static class IntegrationTestsExtensions
{
    /// <summary>
    /// Creates a test video short with the specified parameters.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="title">The video title.</param>
    /// <param name="description">The video description.</param>
    /// <param name="durationSeconds">The video duration in seconds.</param>
    /// <param name="fileSizeBytes">The file size in bytes.</param>
    /// <param name="quality">The video quality.</param>
    /// <param name="processingProfileId">The processing profile ID.</param>
    /// <param name="youTubeChannelId">The YouTube channel ID.</param>
    /// <returns>A configured <see cref="VideoShort"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when title or description is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when duration or file size is invalid.</exception>
    public static VideoShort CreateTestVideoShort(
        this IntegrationTests tests,
        string title,
        string description,
        int durationSeconds = 30,
        long fileSizeBytes = 50 * 1024 * 1024,
        VideoQuality quality = VideoQuality.High,
        int processingProfileId = 1,
        int youTubeChannelId = 1)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);

        if (durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be positive.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fileSizeBytes), "File size must be positive.");
        }

        return new VideoShort
        {
            Title = title,
            Description = description,
            FilePath = $"/test/{Guid.NewGuid()}.mp4",
            Duration = TimeSpan.FromSeconds(durationSeconds),
            FileSizeBytes = fileSizeBytes,
            Quality = quality,
            ProcessingProfileId = processingProfileId,
            YouTubeChannelId = youTubeChannelId
        };
    }

    /// <summary>
    /// Creates a test processing profile with the specified parameters.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="name">The profile name.</param>
    /// <param name="videoWidth">The video width in pixels.</param>
    /// <param name="videoHeight">The video height in pixels.</param>
    /// <param name="videoBitrate">The video bitrate in kbps.</param>
    /// <param name="audioBitrate">The audio bitrate in kbps.</param>
    /// <param name="frameRate">The frame rate in FPS.</param>
    /// <param name="isActive">Whether the profile is active.</param>
    /// <returns>A configured <see cref="ProcessingProfile"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when width, height, or bitrate is invalid.</exception>
    public static ProcessingProfile CreateTestProcessingProfile(
        this IntegrationTests tests,
        string name = "Test Profile",
        int videoWidth = 1080,
        int videoHeight = 1920,
        int videoBitrate = 5000,
        int audioBitrate = 192,
        int frameRate = 30,
        bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (videoWidth <= 0 || videoHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(videoWidth), "Width and height must be positive.");
        }

        if (videoBitrate <= 0 || audioBitrate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(videoBitrate), "Bitrate must be positive.");
        }

        if (frameRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameRate), "Frame rate must be positive.");
        }

        return new ProcessingProfile
        {
            Name = name,
            VideoWidth = videoWidth,
            VideoHeight = videoHeight,
            VideoBitrate = videoBitrate,
            AudioBitrate = audioBitrate,
            FrameRate = frameRate,
            VideoCodec = "h264",
            AudioCodec = "aac",
            Container = "mp4",
            IsActive = isActive
        };
    }

    /// <summary>
    /// Creates a test upload schedule with the specified parameters.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="videoShortId">The video short ID.</param>
    /// <param name="scheduledTime">The scheduled upload time.</param>
    /// <param name="status">The upload status.</param>
    /// <returns>A configured <see cref="UploadJob"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when videoShortId is not positive.</exception>
    public static UploadJob CreateTestUploadJob(
        this IntegrationTests tests,
        int videoShortId,
        DateTime? scheduledTime = null,
        UploadStatus status = UploadStatus.Pending)
    {
        if (videoShortId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(videoShortId), "Video short ID must be positive.");
        }

        return new UploadJob
        {
            VideoShortId = videoShortId,
            ScheduledAt = scheduledTime ?? DateTime.UtcNow.AddHours(1),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates and persists a test video short using the video processing service.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="title">The video title.</param>
    /// <param name="description">The video description.</param>
    /// <param name="durationSeconds">The video duration in seconds.</param>
    /// <param name="fileSizeBytes">The file size in bytes.</param>
    /// <param name="quality">The video quality.</param>
    /// <param name="processingProfileId">The processing profile ID.</param>
    /// <param name="youTubeChannelId">The YouTube channel ID.</param>
    /// <returns>The created <see cref="VideoShort"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when title or description is null.</exception>
    public static async Task<VideoShort> CreateAndProcessVideoAsync(
        this IntegrationTests tests,
        string title,
        string description,
        int durationSeconds = 30,
        long fileSizeBytes = 50 * 1024 * 1024,
        VideoQuality quality = VideoQuality.High,
        int processingProfileId = 1,
        int youTubeChannelId = 1)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);

        var video = tests.CreateTestVideoShort(
            title,
            description,
            durationSeconds,
            fileSizeBytes,
            quality,
            processingProfileId,
            youTubeChannelId);

        var serviceProvider = tests.GetServiceProvider();
        var videoProcessingService = (VideoProcessingService)serviceProvider.GetService(typeof(VideoProcessingService))!;
        videoProcessingService.Should().NotBeNull();

        var createdVideo = await videoProcessingService.CreateProcessingTaskAsync(video);

        createdVideo.Should().NotBeNull();
        createdVideo.Title.Should().Be(title);
        createdVideo.Status.Should().Be(ProcessingStatus.Pending);

        return createdVideo;
    }

    /// <summary>
    /// Creates and persists a test upload schedule using the scheduling service.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="videoShortId">The video short ID to schedule.</param>
    /// <param name="scheduledTime">The scheduled upload time.</param>
    /// <param name="status">The upload status.</param>
    /// <returns>The created <see cref="UploadJob"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when videoShortId is not positive.</exception>
    public static async Task<UploadJob> CreateAndScheduleUploadAsync(
        this IntegrationTests tests,
        int videoShortId,
        DateTime? scheduledTime = null,
        UploadStatus status = UploadStatus.Pending)
    {
        if (videoShortId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(videoShortId), "Video short ID must be positive.");
        }

        var serviceProvider = tests.GetServiceProvider();
        var schedulingService = (SchedulingService)serviceProvider.GetService(typeof(SchedulingService))!;
        schedulingService.Should().NotBeNull();

        var uploadJob = tests.CreateTestUploadJob(videoShortId, scheduledTime, status);

        var createdJob = await schedulingService.ScheduleUploadAsync(
            uploadJob.VideoShortId,
            uploadJob.ScheduledAt);

        createdJob.Should().NotBeNull();
        createdJob.VideoShortId.Should().Be(videoShortId);
        createdJob.Status.Should().Be(UploadStatus.Pending);

        return createdJob;
    }

    /// <summary>
    /// Creates a temporary test file with the specified content.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="content">The file content as bytes.</param>
    /// <returns>The full path to the created file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fileName or content is null.</exception>
    public static string CreateTestFile(
        this IntegrationTests tests,
        string fileName,
        byte[] content)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var testDir = tests.GetTestDirectory();
        var filePath = Path.Combine(testDir, "files", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllBytes(filePath, content);

        return filePath;
    }

    /// <summary>
    /// Creates a temporary test file with random content of the specified size.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="sizeBytes">The file size in bytes.</param>
    /// <returns>The full path to the created file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fileName is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when sizeBytes is not positive.</exception>
    public static string CreateTestFile(
        this IntegrationTests tests,
        string fileName,
        int sizeBytes = 1024 * 1024) // 1MB default
    {
        ArgumentNullException.ThrowIfNull(fileName);

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "Size must be positive.");
        }

        var random = new Random();
        var content = new byte[sizeBytes];
        random.NextBytes(content);

        return tests.CreateTestFile(fileName, content);
    }

    /// <summary>
    /// Gets the service provider from the integration tests instance.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>The <see cref="IServiceProvider"/> instance.</returns>
    private static IServiceProvider GetServiceProvider(this IntegrationTests tests)
    {
        if (tests is null)
        {
            throw new ArgumentNullException(nameof(tests));
        }

        // Use reflection to access the private _serviceProvider field
        var field = typeof(IntegrationTests).GetField(
            "_serviceProvider",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (field?.GetValue(tests) is not IServiceProvider serviceProvider)
        {
            throw new InvalidOperationException("Service provider is not available.");
        }

        return serviceProvider;
    }

    /// <summary>
    /// Gets the test directory path from the integration tests instance.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>The test directory path.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the test directory is not available.</exception>
    public static string GetTestDirectory(this IntegrationTests tests)
    {
        if (tests is null)
        {
            throw new ArgumentNullException(nameof(tests));
        }

        // Use reflection to access the private _testDirectory field
        var field = typeof(IntegrationTests).GetField(
            "_testDirectory",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (field?.GetValue(tests) is not string testDirectory)
        {
            throw new InvalidOperationException("Test directory is not available.");
        }

        return testDirectory;
    }

    /// <summary>
    /// Gets the application settings from the integration tests instance.
    /// </summary>
    /// <param name="tests">The integration tests instance.</param>
    /// <returns>The <see cref="AppSettings"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when app settings are not available.</exception>
    public static AppSettings GetAppSettings(this IntegrationTests tests)
    {
        if (tests is null)
        {
            throw new ArgumentNullException(nameof(tests));
        }

        var serviceProvider = tests.GetServiceProvider();
        var appSettings = (AppSettings)serviceProvider.GetService(typeof(AppSettings))!;

        return appSettings ?? throw new InvalidOperationException("AppSettings is not configured.");
    }
}