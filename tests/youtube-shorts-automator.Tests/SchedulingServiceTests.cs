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

/// <summary>
/// Contains unit tests for the <see cref="SchedulingService"/> class.
/// Tests scheduling functionality including upload scheduling, rescheduling, cancellation,
/// and various job retrieval operations.
/// </summary>
public class SchedulingServiceTests
{
    /// <summary>
    /// Mock repository for testing upload job operations.
    /// </summary>
    private readonly Mock<UploadJobRepository> _mockRepository;

    /// <summary>
    /// Mock logger for testing service logging behavior.
    /// </summary>
    private readonly Mock<ILogger<SchedulingService>> _mockLogger;

    /// <summary>
    /// Instance of the service under test.
    /// </summary>
    private readonly SchedulingService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulingServiceTests"/> class.
    /// Sets up mock dependencies for testing scheduling service functionality.
    /// </summary>
    public SchedulingServiceTests()
    {
        _mockRepository = new Mock<UploadJobRepository>();
        _mockLogger = new Mock<ILogger<SchedulingService>>();
        _service = new SchedulingService(_mockRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that scheduling an upload with a future time creates a job with correct properties.
    /// Verifies that the created job has the expected video short ID, pending status,
    /// and scheduled time.
    /// </summary>
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

    /// <summary>
    /// Tests that scheduling an upload with a past time throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that the service prevents scheduling uploads for times that have already passed.
    /// </summary>
    [Fact]
    public async Task ScheduleUploadAsync_WithPastTime_ThrowsSchedulingException()
    {
        var scheduledTime = DateTime.UtcNow.AddHours(-1);

        Func<Task> act = () => _service.ScheduleUploadAsync(1, scheduledTime);

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    /// <summary>
    /// Tests that scheduling an upload with the current time succeeds without throwing exceptions.
    /// Verifies that scheduling at the exact current time is allowed.
    /// </summary>
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

    /// <summary>
    /// Tests that scheduling an upload sets the maximum retry count to the default value.
    /// Verifies that newly created upload jobs have the correct default retry configuration.
    /// </summary>
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

    /// <summary>
    /// Tests that getting upcoming jobs returns only pending jobs within the specified time window.
    /// Verifies that completed jobs are filtered out and only pending jobs are returned.
    /// </summary>
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

    /// <summary>
    /// Tests that getting upcoming jobs filters by the specified time window in hours.
    /// Verifies that only jobs scheduled within the specified hours ahead are returned,
    /// and jobs beyond that window are excluded.
    /// </summary>
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

    /// <summary>
    /// Tests that getting upcoming jobs with no scheduled jobs returns an empty collection.
    /// Verifies that the service handles the case where no jobs exist in the repository.
    /// </summary>
    [Fact]
    public async Task GetUpcomingJobsAsync_WithNoJobs_ReturnsEmpty()
    {
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UploadJob>());

        var result = await _service.GetUpcomingJobsAsync();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that getting upcoming jobs returns jobs ordered by scheduled time (earliest first).
    /// Verifies that the result collection is sorted chronologically by scheduled time.
    /// </summary>
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

    /// <summary>
    /// Tests that getting overdue jobs returns only pending jobs with past scheduled times.
    /// Verifies that completed jobs with past times are not included in the result,
    /// and only pending jobs that should have been processed already are returned.
    /// </summary>
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

    /// <summary>
    /// Tests that getting overdue jobs ignores non-pending jobs even with past scheduled times.
    /// Verifies that only jobs with <see cref="UploadStatus.Pending"/> status are considered overdue,
    /// regardless of their scheduled time.
    /// </summary>
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

    /// <summary>
    /// Tests that rescheduling an upload with a valid job updates the scheduled time successfully.
    /// Verifies that the service retrieves the job, updates its scheduled time, and persists the changes.
    /// </summary>
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

    /// <summary>
    /// Tests that rescheduling a non-existent job throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that the service validates job existence before attempting to reschedule.
    /// </summary>
    [Fact]
    public async Task RescheduleUploadAsync_WithNonExistentJob_ThrowsSchedulingException()
    {
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UploadJob?)null);

        Func<Task> act = () => _service.RescheduleUploadAsync(999, DateTime.UtcNow.AddHours(1));

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    /// <summary>
    /// Tests that rescheduling a completed job throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that only pending jobs can be rescheduled, as completed jobs cannot be modified.
    /// </summary>
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

    /// <summary>
    /// Tests that rescheduling an uploading job throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that only pending jobs can be rescheduled, as jobs currently being uploaded
    /// cannot have their schedule changed.
    /// </summary>
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

    /// <summary>
    /// Tests that rescheduling an upload with a past time throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that the service prevents rescheduling jobs to times that have already passed,
    /// maintaining the constraint that scheduled times must be in the future.
    /// </summary>
    [Fact]
    public async Task RescheduleUploadAsync_WithPastTime_ThrowsSchedulingException()
    {
        Func<Task> act = () => _service.RescheduleUploadAsync(1, DateTime.UtcNow.AddHours(-1));

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    /// <summary>
    /// Tests that canceling an upload with a pending job successfully cancels the job.
    /// Verifies that the service retrieves the job, updates its status, and persists the cancellation.
    /// </summary>
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

    /// <summary>
    /// Tests that canceling a non-existent job throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that the service validates job existence before attempting to cancel.
    /// </summary>
    [Fact]
    public async Task CancelUploadAsync_WithNonExistentJob_ThrowsSchedulingException()
    {
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UploadJob?)null);

        Func<Task> act = () => _service.CancelUploadAsync(999);

        await act.Should().ThrowAsync<Services.SchedulingException>();
    }

    /// <summary>
    /// Tests that canceling an uploading job throws a <see cref="Services.SchedulingException"/>.
    /// Verifies that only pending jobs can be canceled, as jobs currently being uploaded
    /// cannot be interrupted.
    /// </summary>
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

    /// <summary>
    /// Tests that getting queued job count returns the correct number of queued jobs.
    /// Verifies that only jobs with <see cref="UploadStatus.Queued"/> status are counted,
    /// and other statuses like Pending are excluded from the count.
    /// </summary>
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

    /// <summary>
    /// Tests that calculating optimal upload time with valid input returns a time span.
    /// Verifies that the calculated time span is always greater than or equal to the minimum
    /// allowed upload window of 5 minutes.
    /// </summary>
    /// <param name="createdAt">The creation timestamp of the video short.</param>
    /// <param name="estimatedProcessingMinutes">The estimated processing time in minutes.</param>
    [Fact]
    public void CalculateOptimalUploadTime_WithValidInput_ReturnsTimeSpan()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-30);
        var estimatedProcessingMinutes = 15;

        var timeSpan = _service.CalculateOptimalUploadTime(createdAt, estimatedProcessingMinutes);

        timeSpan.TotalMinutes.Should().BeGreaterThanOrEqualTo(5);
    }

    /// <summary>
    /// Tests that calculating optimal upload time with future created time returns a positive time span.
    /// Verifies that even when the video short was created in the future, the method returns
    /// a valid positive time span rather than failing or returning a negative value.
    /// </summary>
    /// <param name="createdAt">The creation timestamp of the video short (in the future).</param>
    /// <param name="estimatedProcessingMinutes">The estimated processing time in minutes.</param>
    [Fact]
    public void CalculateOptimalUploadTime_WithFutureCreatedTime_ReturnsPositiveTimeSpan()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(10);
        var estimatedProcessingMinutes = 20;

        var timeSpan = _service.CalculateOptimalUploadTime(createdAt, estimatedProcessingMinutes);

        timeSpan.TotalMinutes.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that a morning hour (10 AM) is within the optimal upload window.
    /// Verifies that the optimal upload window includes typical business hours
    /// when traffic and engagement are typically higher.
    /// </summary>
    /// <param name="scheduleTime">The scheduled time to test (10 AM).</param>
    [Fact]
    public void IsWithinOptimalUploadWindow_WithMorningHour_ReturnsTrue()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(10); // 10 AM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that an evening hour (8 PM) is within the optimal upload window.
    /// Verifies that the optimal upload window includes evening hours when users
    /// are typically more active on social media platforms.
    /// </summary>
    /// <param name="scheduleTime">The scheduled time to test (8 PM).</param>
    [Fact]
    public void IsWithinOptimalUploadWindow_WithEveningHour_ReturnsTrue()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(20); // 8 PM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a night hour (3 AM) is outside the optimal upload window.
    /// Verifies that the optimal upload window excludes late night hours when engagement
    /// and traffic are typically low, preventing suboptimal upload times.
    /// </summary>
    /// <param name="scheduleTime">The scheduled time to test (3 AM).</param>
    [Fact]
    public void IsWithinOptimalUploadWindow_WithNightHour_ReturnsFalse()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(3); // 3 AM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that a late night hour (11:59 PM) is outside the optimal upload window.
    /// Verifies that the optimal upload window excludes late night hours when user engagement
    /// is typically minimal, ensuring uploads are scheduled for times with better audience reach.
    /// </summary>
    /// <param name="scheduleTime">The scheduled time to test (11:59 PM).</param>
    [Fact]
    public void IsWithinOptimalUploadWindow_WithLateNightHour_ReturnsFalse()
    {
        var scheduleTime = DateTime.UtcNow.Date.AddHours(23).AddMinutes(59); // 11:59 PM

        var result = _service.IsWithinOptimalUploadWindow(scheduleTime);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the optimal upload window boundaries (9 AM and 11 PM) are included.
    /// Verifies that the window boundaries are inclusive, allowing scheduling at exactly
    /// the start and end times of the optimal window.
    /// </summary>
    [Fact]
    public void IsWithinOptimalUploadWindow_WithOptimalBoundaries_ReturnsTrue()
    {
        var scheduleTime9Am = DateTime.UtcNow.Date.AddHours(9);
        var scheduleTime11Pm = DateTime.UtcNow.Date.AddHours(23);

        _service.IsWithinOptimalUploadWindow(scheduleTime9Am).Should().BeTrue();
        _service.IsWithinOptimalUploadWindow(scheduleTime11Pm).Should().BeTrue();
    }
}
