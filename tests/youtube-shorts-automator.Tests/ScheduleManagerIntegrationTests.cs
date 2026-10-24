// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Infrastructure.Repositories;

namespace YouTubeShortsAutomator.Tests;

/// <summary>
/// Integration tests for <see cref="ScheduleManager"/> running against a real EF Core context on a SQLite file.
/// Each test gets its own database file; a "restart" disposes the context and rebuilds the manager on the same file.
/// </summary>
public class ScheduleManagerIntegrationTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private readonly string _directory;
    private readonly string _databasePath;
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _scheduleId = Guid.NewGuid();

    public ScheduleManagerIntegrationTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"schedule-manager-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_directory);
        _databasePath = Path.Combine(_directory, "schedules.db");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_UploadsQueuedVideosInPublishOrder_AsEachBecomesDue()
    {
        await SeedScheduleAsync();
        var uploader = new RecordingVideoUploader();
        var videoAt1h = Guid.NewGuid();
        var videoAt2h = Guid.NewGuid();
        var videoAt3h = Guid.NewGuid();

        await using var context = CreateContext();
        var manager = CreateManager(context, uploader);

        // Enqueued out of publish order on purpose
        await manager.EnqueueUploadAsync(_scheduleId, videoAt3h, Start.AddHours(3).UtcDateTime);
        await manager.EnqueueUploadAsync(_scheduleId, videoAt1h, Start.AddHours(1).UtcDateTime);
        await manager.EnqueueUploadAsync(_scheduleId, videoAt2h, Start.AddHours(2).UtcDateTime);

        _clock.Advance(TimeSpan.FromMinutes(30));
        (await manager.ProcessDueSchedulesAsync()).Should().Be(0);
        uploader.UploadedVideoIds.Should().BeEmpty();

        _clock.Advance(TimeSpan.FromMinutes(60));
        (await manager.ProcessDueSchedulesAsync()).Should().Be(1);
        uploader.UploadedVideoIds.Should().Equal(videoAt1h);

        _clock.Advance(TimeSpan.FromHours(3));
        (await manager.ProcessDueSchedulesAsync()).Should().Be(2);
        uploader.UploadedVideoIds.Should().Equal(videoAt1h, videoAt2h, videoAt3h);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_DoesNotReTriggerUploadsAlreadyPublished()
    {
        await SeedScheduleAsync();
        var uploader = new RecordingVideoUploader();
        var video = Guid.NewGuid();

        await using var context = CreateContext();
        var manager = CreateManager(context, uploader);
        await manager.EnqueueUploadAsync(_scheduleId, video, Start.AddHours(1).UtcDateTime);

        _clock.Advance(TimeSpan.FromHours(2));
        (await manager.ProcessDueSchedulesAsync()).Should().Be(1);

        _clock.Advance(TimeSpan.FromDays(1));
        (await manager.ProcessDueSchedulesAsync()).Should().Be(0);
        (await manager.ProcessDueSchedulesAsync()).Should().Be(0);

        uploader.UploadedVideoIds.Should().Equal(video);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_AfterRestart_ResumesFromPersistedStateWithoutRepeatingUploads()
    {
        await SeedScheduleAsync();
        var uploader = new RecordingVideoUploader();
        var videoAt1h = Guid.NewGuid();
        var videoAt2h = Guid.NewGuid();
        var videoAt3h = Guid.NewGuid();

        await using (var firstSession = CreateContext())
        {
            var manager = CreateManager(firstSession, uploader);
            await manager.EnqueueUploadAsync(_scheduleId, videoAt1h, Start.AddHours(1).UtcDateTime);
            await manager.EnqueueUploadAsync(_scheduleId, videoAt2h, Start.AddHours(2).UtcDateTime);
            await manager.EnqueueUploadAsync(_scheduleId, videoAt3h, Start.AddHours(3).UtcDateTime);

            _clock.Advance(TimeSpan.FromMinutes(90));
            (await manager.ProcessDueSchedulesAsync()).Should().Be(1);
        }

        // Brand-new context and manager over the same database file
        await using (var secondSession = CreateContext())
        {
            var manager = CreateManager(secondSession, uploader);

            _clock.Advance(TimeSpan.FromHours(4));
            (await manager.ProcessDueSchedulesAsync()).Should().Be(2);
        }

        uploader.UploadedVideoIds.Should().Equal(videoAt1h, videoAt2h, videoAt3h);

        await using var verification = CreateContext();
        var persisted = await new ScheduleRepository(verification, NullLogger<ScheduleRepository>.Instance)
            .GetByIdWithUploadsAsync(_scheduleId);

        persisted.Should().NotBeNull();
        persisted!.ScheduledUploads.Should().HaveCount(3)
            .And.OnlyContain(u => u.Status == ScheduledUploadStatus.Completed && u.ExecutedAt != null);
        persisted.TotalExecutions.Should().Be(3);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_AfterRestart_DoesNotRetryFailedUpload()
    {
        await SeedScheduleAsync();
        var uploader = new RecordingVideoUploader();
        var failingVideo = Guid.NewGuid();
        var followingVideo = Guid.NewGuid();
        uploader.FailFor(failingVideo);

        await using (var firstSession = CreateContext())
        {
            var manager = CreateManager(firstSession, uploader);
            await manager.EnqueueUploadAsync(_scheduleId, failingVideo, Start.AddHours(1).UtcDateTime);
            await manager.EnqueueUploadAsync(_scheduleId, followingVideo, Start.AddHours(2).UtcDateTime);

            _clock.Advance(TimeSpan.FromMinutes(90));
            (await manager.ProcessDueSchedulesAsync()).Should().Be(0);
        }

        await using (var secondSession = CreateContext())
        {
            var manager = CreateManager(secondSession, uploader);

            _clock.Advance(TimeSpan.FromHours(2));
            (await manager.ProcessDueSchedulesAsync()).Should().Be(1);
        }

        uploader.UploadedVideoIds.Should().Equal(failingVideo, followingVideo);

        await using var verification = CreateContext();
        var persisted = await new ScheduleRepository(verification, NullLogger<ScheduleRepository>.Instance)
            .GetByIdWithUploadsAsync(_scheduleId);

        persisted!.ScheduledUploads.Single(u => u.VideoId == failingVideo).Status
            .Should().Be(ScheduledUploadStatus.Failed);
        persisted.ScheduledUploads.Single(u => u.VideoId == followingVideo).Status
            .Should().Be(ScheduledUploadStatus.Completed);
    }

    [Fact]
    public async Task EnqueueUploadAsync_RejectsSameVideoTwiceInSchedule()
    {
        await SeedScheduleAsync();
        var video = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var manager = CreateManager(context, new RecordingVideoUploader());
            await manager.EnqueueUploadAsync(_scheduleId, video, Start.AddHours(1).UtcDateTime);

            var act = () => manager.EnqueueUploadAsync(_scheduleId, video, Start.AddHours(5).UtcDateTime);

            var error = await act.Should().ThrowAsync<DomainException>();
            error.Which.ErrorCode.Should().Be("DUPLICATE_SCHEDULE_ENTRY");
        }

        await using var verification = CreateContext();
        var persisted = await new ScheduleRepository(verification, NullLogger<ScheduleRepository>.Instance)
            .GetByIdWithUploadsAsync(_scheduleId);
        persisted!.ScheduledUploads.Should().ContainSingle(u => u.VideoId == video);
    }

    [Fact]
    public async Task EnqueueUploadAsync_RejectsSecondVideoInAlreadyTakenPublishSlot()
    {
        await SeedScheduleAsync();
        var slotOwner = Guid.NewGuid();
        var overlapping = Guid.NewGuid();
        var slot = Start.AddHours(2).UtcDateTime;

        await using (var context = CreateContext())
        {
            var manager = CreateManager(context, new RecordingVideoUploader());
            await manager.EnqueueUploadAsync(_scheduleId, slotOwner, slot);

            var act = () => manager.EnqueueUploadAsync(_scheduleId, overlapping, slot);

            var error = await act.Should().ThrowAsync<DomainException>();
            error.Which.ErrorCode.Should().Be("SCHEDULE_OVERLAP");
        }

        await using var verification = CreateContext();
        var persisted = await new ScheduleRepository(verification, NullLogger<ScheduleRepository>.Instance)
            .GetByIdWithUploadsAsync(_scheduleId);
        persisted!.ScheduledUploads.Should().ContainSingle()
            .Which.VideoId.Should().Be(slotOwner);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private ScheduleManager CreateManager(ApplicationDbContext context, IVideoUploader uploader)
    {
        return new ScheduleManager(
            NullLogger<ScheduleManager>.Instance,
            new ScheduleRepository(context, NullLogger<ScheduleRepository>.Instance),
            uploader,
            _clock);
    }

    private async Task SeedScheduleAsync()
    {
        var schedule = new UploadSchedule
        {
            Id = _scheduleId,
            UserId = _userId,
            ScheduleName = "Shorts daily",
            Frequency = ScheduleFrequency.Daily,
            ScheduledTime = new TimeOnly(9, 0),
            TimeZoneId = "UTC",
            IsActive = true,
            CreatedAt = Start.UtcDateTime
        };
        schedule.NextScheduledTime = schedule.CalculateNextScheduledTime(Start.UtcDateTime);
        schedule.Validate().IsValid.Should().BeTrue();

        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        context.Users.Add(new User
        {
            Id = _userId,
            Email = "creator@example.test",
            DisplayName = "Test Creator",
            ChannelId = "UC-test",
            CreatedAt = Start.UtcDateTime
        });
        context.UploadSchedules.Add(schedule);
        await context.SaveChangesAsync();
    }
}
