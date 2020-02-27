// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

public class SchedulingServiceTests
{
    private readonly Mock<UploadJobRepository> _mockRepository;
    private readonly Mock<ILogger<SchedulingService>> _mockLogger;
    private readonly SchedulingService _service;

    public SchedulingServiceTests()
    {
        _mockRepository = new Mock<UploadJobRepository>();
        _mockLogger = new Mock<ILogger<SchedulingService>>();
        _service = new SchedulingService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ScheduleUploadAsync_WithFutureTime_CreatesScheduledJob()
    {
        var videoShortId = 1;
        var scheduledTime = DateTime.UtcNow.AddHours(2);
        var createdJob = new UploadJob
        {
            Id = 1,
            VideoShortId = videoShortId,
            Status = UploadStatus.Pending,
            ScheduledAt = scheduledTime
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdJob);

        var result = await _service.ScheduleUploadAsync(videoShortId, scheduledTime);

        result.Should().NotBeNull();
        result.VideoShortId.Should().Be(videoShortId);
        result.Status.Should().Be(UploadStatus.Pending);
        result.ScheduledAt.Should().Be(scheduledTime);
    }

    [Fact]
    public async Task ScheduleUploadAsync_WithPastTime_ThrowsSchedulingException()
    {
        var scheduledTime = DateTime.UtcNow.AddHours(-1);

        Func<Task> act = () => _service.ScheduleUploadAsync(1, scheduledTime);

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task ScheduleUploadAsync_WithNowTime_Succeeds()
    {
        var videoShortId = 1;
        var scheduledTime = DateTime.UtcNow;
        var createdJob = new UploadJob
        {
            Id = 1,
            VideoShortId = videoShortId,
            Status = UploadStatus.Pending,
            ScheduledAt = scheduledTime
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdJob);

        Func<Task> act = () => _service.ScheduleUploadAsync(videoShortId, scheduledTime);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ScheduleUploadAsync_SetsMaxRetriesCorrectly()
    {
        var videoShortId = 1;
        var scheduledTime = DateTime.UtcNow.AddHours(1);
        UploadJob capturedJob = null!;

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()))
            .Callback<UploadJob, CancellationToken>((job, _) => capturedJob = job)
            .ReturnsAsync(new UploadJob { Id = 1, VideoShortId = videoShortId });

        await _service.ScheduleUploadAsync(videoShortId, scheduledTime);

        capturedJob.MaxRetries.Should().Be(Constants.Constants.DEFAULT_RETRY_COUNT);
    }

    [Fact]
    public async Task GetUpcomingJobsAsync_WithPendingJobs_ReturnsScheduledJobs()
    {
        var now = DateTime.UtcNow;
        var jobs = new List<UploadJob>
        {
            new() { Id = 1, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(2) },
            new() { Id = 2, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(6) },
            new() { Id = 3, Status = UploadStatus.Completed, ScheduledAt = now.AddHours(12) }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        var result = await _service.GetUpcomingJobsAsync(24);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(j => j.Status.Should().Be(UploadStatus.Pending));
    }

    [Fact]
    public async Task GetUpcomingJobsAsync_WithHoursAhead_FiltersByTimeWindow()
    {
        var now = DateTime.UtcNow;
        var jobs = new List<UploadJob>
        {
            new() { Id = 1, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(2) },
            new() { Id = 2, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(6) },
            new() { Id = 3, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(30) }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        var result = await _service.GetUpcomingJobsAsync(12);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(j => j.ScheduledAt.Should().BeOnOrBefore(now.AddHours(12)));
    }

    [Fact]
    public async Task GetUpcomingJobsAsync_WithNoJobs_ReturnsEmpty()
    {
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UploadJob>());

        var result = await _service.GetUpcomingJobsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpcomingJobsAsync_ReturnsJobsOrderedByScheduledTime()
    {
        var now = DateTime.UtcNow;
        var jobs = new List<UploadJob>
        {
            new() { Id = 1, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(5) },
            new() { Id = 2, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(2) },
            new() { Id = 3, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(10) }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        var result = await _service.GetUpcomingJobsAsync();

        result.First().Id.Should().Be(2);
        result.Last().Id.Should().Be(3);
    }

    [Fact]
    public async Task GetOverdueJobsAsync_WithPastScheduledTime_ReturnsPendingJob()
    {
        var now = DateTime.UtcNow;
        var jobs = new List<UploadJob>
        {
            new() { Id = 1, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(-1) },
            new() { Id = 2, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(2) }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        var result = await _service.GetOverdueJobsAsync();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetOverdueJobsAsync_IgnoresNonPendingJobs()
    {
        var now = DateTime.UtcNow;
        var jobs = new List<UploadJob>
        {
            new() { Id = 1, Status = UploadStatus.Completed, ScheduledAt = now.AddHours(-1) },
            new() { Id = 2, Status = UploadStatus.Pending, ScheduledAt = now.AddHours(-1) }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        var result = await _service.GetOverdueJobsAsync();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task RescheduleUploadAsync_WithValidJob_UpdatesScheduledTime()
    {
        var jobId = 1;
        var newScheduledTime = DateTime.UtcNow.AddHours(3);
        var job = new UploadJob { Id = jobId, Status = UploadStatus.Pending, ScheduledAt = DateTime.UtcNow.AddHours(1) };

        _mockRepository
            .Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var result = await _service.RescheduleUploadAsync(jobId, newScheduledTime);

        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RescheduleUploadAsync_WithNonExistentJob_ThrowsSchedulingException()
    {
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UploadJob?)null);

        Func<Task> act = () => _service.RescheduleUploadAsync(999, DateTime.UtcNow.AddHours(1));

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task RescheduleUploadAsync_WithCompletedJob_ThrowsSchedulingException()
    {
        var job = new UploadJob { Id = 1, Status = UploadStatus.Completed };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        Func<Task> act = () => _service.RescheduleUploadAsync(1, DateTime.UtcNow.AddHours(2));

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task RescheduleUploadAsync_WithUploadingJob_ThrowsSchedulingException()
    {
        var job = new UploadJob { Id = 1, Status = UploadStatus.Uploading };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        Func<Task> act = () => _service.RescheduleUploadAsync(1, DateTime.UtcNow.AddHours(2));

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task RescheduleUploadAsync_WithPastTime_ThrowsSchedulingException()
    {
        Func<Task> act = () => _service.RescheduleUploadAsync(1, DateTime.UtcNow.AddHours(-1));

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task CancelUploadAsync_WithPendingJob_CancelsJob()
    {
        var job = new UploadJob { Id = 1, Status = UploadStatus.Pending };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var result = await _service.CancelUploadAsync(1);

        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UploadJob>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelUploadAsync_WithNonExistentJob_ThrowsSchedulingException()
    {
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UploadJob?)null);

        Func<Task> act = () => _service.CancelUploadAsync(999);

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task CancelUploadAsync_WithUploadingJob_ThrowsSchedulingException()
    {
        var job = new UploadJob { Id = 1, Status = UploadStatus.Uploading };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        Func<Task> act = () => _service.CancelUploadAsync(1);

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    [Fact]
    public async Task GetQueuedJobCountAsync_ReturnsCountOfQueuedJobs()
    {
        var queuedJobs = new List<UploadJob>
        {
            new() { Id = 1, Status = UploadStatus.Queued },
            new() { Id = 2, Status = UploadStatus.Queued },
            new() { Id = 3, Status = UploadStatus.Pending }
        };

        _mockRepository
            .Setup(r => r.GetByStatusAsync(UploadStatus.Queued, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queuedJobs.Where(j => j.Status == UploadStatus.Queued));

        var count = await _service.GetQueuedJobCountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public void CalculateOptimalUploadTime_WithValidInput_ReturnsTimeSpan()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-30);
        var estimatedProcessingMinutes = 15;

        var timeSpan = _service.CalculateOptimalUploadTime(createdAt, estimatedProcessingMinutes);

        timeSpan.TotalMinutes.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void CalculateOptimalUploadTime_WithFutureCreatedTime_ReturnsPositiveTimeSpan()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(10);
        var estimatedProcessingMinutes = 20;

        var timeSpan = _service.CalculateOptimalUploadTime(createdAt, estimatedProcessingMinutes);

        timeSpan.TotalMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IsWithinOptimalUploadWindow_WithMorningHour_ReturnsTrue()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(10); // 10 AM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinOptimalUploadWindow_WithEveningHour_ReturnsTrue()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(20); // 8 PM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinOptimalUploadWindow_WithNightHour_ReturnsFalse()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(3); // 3 AM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinOptimalUploadWindow_WithLateNightHour_ReturnsFalse()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(23).AddMinutes(59); // 11:59 PM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinOptimalUploadWindow_WithOptimalBoundaries_ReturnsTrue()
    {
        var scheduleTime9Am = DateTime.UtcNow.Date.AddHours(9);
        var scheduleTime11Pm = DateTime.UtcNow.Date.AddHours(23);

        _service.IsWithinOptimalUploadWindow(scheduleTime9Am).Should().BeTrue();
        _service.IsWithinOptimalUploadWindow(scheduleTime11Pm).Should().BeTrue();
    }
}
