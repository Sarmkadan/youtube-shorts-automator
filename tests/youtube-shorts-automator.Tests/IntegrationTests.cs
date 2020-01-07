// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortAutomator.Configuration;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

public class IntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _testDirectory;

    public IntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"integration-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        var services = new ServiceCollection();

        var appSettings = new AppSettings
        {
            LogDirectory = Path.Combine(_testDirectory, "logs"),
            ProcessingDirectory = Path.Combine(_testDirectory, "processing"),
            OutputDirectory = Path.Combine(_testDirectory, "output"),
            DatabasePath = Path.Combine(_testDirectory, "test.db")
        };

        services.AddSingleton(appSettings);
        services.AddLogging(config => config.AddConsole());

        services.AddScoped<VideoShortRepository>();
        services.AddScoped<UploadJobRepository>();
        services.AddScoped<AnalyticsRepository>();
        services.AddScoped<UploadHistoryRepository>();

        services.AddScoped<FileValidationService>();
        services.AddScoped<VideoProcessingService>();
        services.AddScoped<SchedulingService>();
        services.AddScoped<AnalyticsService>();

        _serviceProvider = services.BuildServiceProvider();

        InitializeDirectories(appSettings);
    }

    private void InitializeDirectories(AppSettings appSettings)
    {
        var directories = new[] { appSettings.LogDirectory, appSettings.ProcessingDirectory, appSettings.OutputDirectory };
        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    [Fact]
    public async Task EndToEnd_ScheduleUpload_CompletesSuccessfully()
    {
        var schedulingService = _serviceProvider.GetRequiredService<SchedulingService>();
        var uploadJobRepository = _serviceProvider.GetRequiredService<UploadJobRepository>();

        var scheduledTime = DateTime.UtcNow.AddHours(1);
        var job = await schedulingService.ScheduleUploadAsync(1, scheduledTime);

        job.Should().NotBeNull();
        job.Id.Should().BeGreaterThan(0);
        job.Status.Should().Be(UploadStatus.Pending);
        job.VideoShortId.Should().Be(1);

        var retrievedJob = await uploadJobRepository.GetByIdAsync(job.Id);
        retrievedJob.Should().NotBeNull();
        retrievedJob.ScheduledAt.Should().Be(scheduledTime);
    }

    [Fact]
    public async Task EndToEnd_CreateVideo_CreateAnalytics_SyncMetrics()
    {
        var videoProcessingService = _serviceProvider.GetRequiredService<VideoProcessingService>();
        var analyticsService = _serviceProvider.GetRequiredService<AnalyticsService>();
        var videoRepository = _serviceProvider.GetRequiredService<VideoShortRepository>();

        var videoShort = new VideoShort
        {
            Title = "Test Video",
            Description = "Test Description",
            FilePath = "/test/video.mp4",
            Duration = TimeSpan.FromSeconds(30),
            FileSizeBytes = 50 * 1024 * 1024,
            Quality = VideoQuality.High,
            ProcessingProfileId = 1,
            YouTubeChannelId = 1
        };

        var createdVideo = await videoProcessingService.CreateProcessingTaskAsync(videoShort);
        createdVideo.Should().NotBeNull();
        createdVideo.Id.Should().BeGreaterThan(0);

        var analyticsRecord = await analyticsService.CreateAnalyticsRecordAsync(createdVideo.Id);
        analyticsRecord.Should().NotBeNull();
        analyticsRecord.VideoShortId.Should().Be(createdVideo.Id);
        analyticsRecord.ViewCount.Should().Be(0);
    }

    [Fact]
    public async Task ConcurrencyTest_MultipleSchedulesSimultaneously()
    {
        var schedulingService = _serviceProvider.GetRequiredService<SchedulingService>();

        var tasks = Enumerable.Range(1, 10)
            .Select(i => schedulingService.ScheduleUploadAsync(
                i,
                DateTime.UtcNow.AddHours(i)))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(10);
        results.Should().AllSatisfy(job =>
        {
            job.Should().NotBeNull();
            job.Status.Should().Be(UploadStatus.Pending);
        });
    }

    [Fact]
    public async Task ConcurrencyTest_MultipleVideoCreationsSimultaneously()
    {
        var videoProcessingService = _serviceProvider.GetRequiredService<VideoProcessingService>();

        var tasks = Enumerable.Range(1, 5)
            .Select(i => new VideoShort
            {
                Title = $"Video {i}",
                Description = $"Description {i}",
                FilePath = $"/test/video{i}.mp4",
                Duration = TimeSpan.FromSeconds(30),
                FileSizeBytes = 50 * 1024 * 1024,
                Quality = VideoQuality.High,
                ProcessingProfileId = 1,
                YouTubeChannelId = 1
            })
            .Select(v => videoProcessingService.CreateProcessingTaskAsync(v))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(5);
        results.Should().AllSatisfy(video =>
        {
            video.Should().NotBeNull();
            video.Status.Should().Be(ProcessingStatus.Pending);
        });

        var uniqueIds = results.Select(v => v.Id).Distinct();
        uniqueIds.Should().HaveCount(5);
    }

    [Fact]
    public async Task ConfigurationTest_DifferentProcessingProfiles()
    {
        var videoProcessingService = _serviceProvider.GetRequiredService<VideoProcessingService>();

        var profiles = new[]
        {
            new ProcessingProfile
            {
                Name = "High Quality",
                VideoWidth = 1080,
                VideoHeight = 1920,
                VideoBitrate = 5000,
                AudioBitrate = 192,
                FrameRate = 30,
                VideoCodec = "h264",
                AudioCodec = "aac",
                Container = "mp4",
                IsActive = true
            },
            new ProcessingProfile
            {
                Name = "Standard Quality",
                VideoWidth = 720,
                VideoHeight = 1280,
                VideoBitrate = 2500,
                AudioBitrate = 128,
                FrameRate = 24,
                VideoCodec = "h264",
                AudioCodec = "aac",
                Container = "mp4",
                IsActive = true
            },
            new ProcessingProfile
            {
                Name = "Mobile Optimized",
                VideoWidth = 540,
                VideoHeight = 960,
                VideoBitrate = 1500,
                AudioBitrate = 96,
                FrameRate = 24,
                VideoCodec = "h264",
                AudioCodec = "aac",
                Container = "mp4",
                IsActive = true
            }
        };

        var results = new List<ProcessingTask>();
        foreach (var profile in profiles)
        {
            var video = new VideoShort
            {
                Title = $"Test Video for {profile.Name}",
                Description = "Test",
                FilePath = "/test/video.mp4",
                Duration = TimeSpan.FromSeconds(30),
                FileSizeBytes = 50 * 1024 * 1024,
                Quality = VideoQuality.High,
                ProcessingProfileId = profile.Id,
                YouTubeChannelId = 1
            };

            var task = await videoProcessingService.ProcessVideoAsync(video, profile);
            results.Add(task);
        }

        results.Should().HaveCount(3);
        results[0].OutputWidth.Should().Be(1080);
        results[1].OutputWidth.Should().Be(720);
        results[2].OutputWidth.Should().Be(540);
    }

    [Fact]
    public async Task SchedulingWorkflow_CreateScheduleAndRetrieveUpcoming()
    {
        var schedulingService = _serviceProvider.GetRequiredService<SchedulingService>();

        var now = DateTime.UtcNow;
        var job1 = await schedulingService.ScheduleUploadAsync(1, now.AddHours(2));
        var job2 = await schedulingService.ScheduleUploadAsync(2, now.AddHours(6));
        var job3 = await schedulingService.ScheduleUploadAsync(3, now.AddHours(30));

        var upcoming = await schedulingService.GetUpcomingJobsAsync(24);

        upcoming.Should().HaveCount(2);
        upcoming.Should().Contain(j => j.Id == job1.Id);
        upcoming.Should().Contain(j => j.Id == job2.Id);
        upcoming.Should().NotContain(j => j.Id == job3.Id);
    }

    [Fact]
    public async Task SchedulingWorkflow_RescheduleAndVerify()
    {
        var schedulingService = _serviceProvider.GetRequiredService<SchedulingService>();
        var uploadJobRepository = _serviceProvider.GetRequiredService<UploadJobRepository>();

        var originalTime = DateTime.UtcNow.AddHours(1);
        var job = await schedulingService.ScheduleUploadAsync(1, originalTime);
        job.ScheduledAt.Should().Be(originalTime);

        var newTime = DateTime.UtcNow.AddHours(3);
        var rescheduleSuccess = await schedulingService.RescheduleUploadAsync(job.Id, newTime);

        rescheduleSuccess.Should().BeTrue();

        var updatedJob = await uploadJobRepository.GetByIdAsync(job.Id);
        updatedJob.ScheduledAt.Should().Be(newTime);
    }

    [Fact]
    public async Task SchedulingWorkflow_CancelUpload()
    {
        var schedulingService = _serviceProvider.GetRequiredService<SchedulingService>();
        var uploadJobRepository = _serviceProvider.GetRequiredService<UploadJobRepository>();

        var job = await schedulingService.ScheduleUploadAsync(1, DateTime.UtcNow.AddHours(2));
        job.Status.Should().Be(UploadStatus.Pending);

        var cancelSuccess = await schedulingService.CancelUploadAsync(job.Id);

        cancelSuccess.Should().BeTrue();

        var cancelledJob = await uploadJobRepository.GetByIdAsync(job.Id);
        cancelledJob.Status.Should().Be(UploadStatus.Cancelled);
    }

    [Fact]
    public async Task AnalyticsWorkflow_CreateAndGenerateReport()
    {
        var analyticsService = _serviceProvider.GetRequiredService<AnalyticsService>();

        var videoId = 1;
        var analytics = await analyticsService.CreateAnalyticsRecordAsync(videoId);

        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;
        var report = await analyticsService.GeneratePeriodReportAsync(startDate, endDate);

        report.Should().NotBeNull();
        report.PeriodStart.Should().Be(startDate);
        report.PeriodEnd.Should().Be(endDate);
        report.TotalVideos.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task FileValidationWorkflow_ValidateAndHash()
    {
        var fileValidationService = _serviceProvider.GetRequiredService<FileValidationService>();
        var testDir = Path.Combine(_testDirectory, "file-test");
        Directory.CreateDirectory(testDir);

        var testFile = Path.Combine(testDir, "test.mp4");
        var testBytes = new byte[5 * 1024 * 1024];
        new Random().NextBytes(testBytes);
        File.WriteAllBytes(testFile, testBytes);

        try
        {
            var isValid = fileValidationService.ValidateVideoFile(testFile);
            isValid.Should().BeTrue();

            var hash1 = fileValidationService.GetFileHash(testFile);
            var hash2 = fileValidationService.GetFileHash(testFile);

            hash1.Should().Be(hash2);
            hash1.Length.Should().Be(64);

            var duration = fileValidationService.GetVideoDuration(testFile);
            duration.Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(testFile)) File.Delete(testFile);
            if (Directory.Exists(testDir)) Directory.Delete(testDir);
        }
    }

    [Fact]
    public async Task MainUseCase_ProcessVideoAndScheduleUpload()
    {
        var videoProcessingService = _serviceProvider.GetRequiredService<VideoProcessingService>();
        var schedulingService = _serviceProvider.GetRequiredService<SchedulingService>();
        var analyticsService = _serviceProvider.GetRequiredService<AnalyticsService>();

        var video = new VideoShort
        {
            Title = "My YouTube Short",
            Description = "A quick test video",
            FilePath = "/tmp/test.mp4",
            Duration = TimeSpan.FromSeconds(45),
            FileSizeBytes = 100 * 1024 * 1024,
            Quality = VideoQuality.High,
            ProcessingProfileId = 1,
            YouTubeChannelId = 1
        };

        var processedVideo = await videoProcessingService.CreateProcessingTaskAsync(video);
        processedVideo.Should().NotBeNull();
        processedVideo.Status.Should().Be(ProcessingStatus.Pending);

        var scheduledTime = DateTime.UtcNow.AddHours(2);
        var uploadJob = await schedulingService.ScheduleUploadAsync(processedVideo.Id, scheduledTime);
        uploadJob.Should().NotBeNull();
        uploadJob.Status.Should().Be(UploadStatus.Pending);

        var analytics = await analyticsService.CreateAnalyticsRecordAsync(processedVideo.Id);
        analytics.Should().NotBeNull();
        analytics.VideoShortId.Should().Be(processedVideo.Id);

        var upcoming = await schedulingService.GetUpcomingJobsAsync(24);
        upcoming.Should().Contain(j => j.Id == uploadJob.Id);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
            }
        }
    }
}
