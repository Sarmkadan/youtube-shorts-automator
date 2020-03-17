// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Extension methods for <see cref="VideoShortModelTests"/> that provide additional testing utilities
/// for VideoShort model validation and processing scenarios.
/// </summary>
public static class VideoShortModelTestsExtensions
{
    /// <summary>
    /// Creates a video short with valid metadata for testing purposes.
    /// </summary>
    /// <param name="title">The title of the video short.</param>
    /// <param name="durationSeconds">The duration in seconds.</param>
    /// <param name="processingProfileId">The processing profile identifier.</param>
    /// <param name="youTubeChannelId">The YouTube channel identifier.</param>
    /// <returns>A new <see cref="VideoShort"/> instance with valid metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when title is null or whitespace.</exception>
    public static VideoShort CreateValidVideoShort(
        this VideoShortModelTests _,
        string title = "Test Video Short",
        int durationSeconds = 30,
        int processingProfileId = 1,
        int youTubeChannelId = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);

        return new VideoShort
        {
            Title = title,
            Description = "Test description for video short",
            FilePath = "/tmp/test.mp4",
            Duration = TimeSpan.FromSeconds(durationSeconds),
            ProcessingProfileId = processingProfileId,
            YouTubeChannelId = youTubeChannelId,
            Status = Constants.ProcessingStatus.Pending,
            Tags = ["test", "short"],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a video short with invalid metadata for testing validation failures.
    /// </summary>
    /// <param name="invalidTitle">Whether to create a video with an empty title.</param>
    /// <param name="invalidDuration">Whether to create a video with duration beyond 60 seconds.</param>
    /// <param name="missingProfile">Whether to create a video with invalid processing profile ID.</param>
    /// <returns>A new <see cref="VideoShort"/> instance with invalid metadata.</returns>
    public static VideoShort CreateInvalidVideoShort(
        this VideoShortModelTests _,
        bool invalidTitle = true,
        bool invalidDuration = false,
        bool missingProfile = false)
    {
        var video = new VideoShort
        {
            Title = invalidTitle ? string.Empty : "Valid Title",
            Description = "Test description",
            FilePath = "/tmp/test.mp4",
            Duration = invalidDuration ? TimeSpan.FromSeconds(61) : TimeSpan.FromSeconds(30),
            ProcessingProfileId = missingProfile ? 0 : 1,
            YouTubeChannelId = 1,
            Status = Constants.ProcessingStatus.Pending
        };

        return video;
    }

    /// <summary>
    /// Asserts that a video short has the expected processing status.
    /// </summary>
    /// <param name="video">The video short to check.</param>
    /// <param name="expectedStatus">The expected processing status.</param>
    /// <exception cref="ArgumentNullException">Thrown when video is null.</exception>
    public static void ShouldHaveStatus(
        this VideoShortModelTests _,
        VideoShort video,
        ProcessingStatus expectedStatus)
    {
        ArgumentNullException.ThrowIfNull(video);

        video.Status.Should().Be(expectedStatus);
    }

    /// <summary>
    /// Asserts that a video short has a non-null error message.
    /// </summary>
    /// <param name="video">The video short to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when video is null.</exception>
    public static void ShouldHaveError(
        this VideoShortModelTests _,
        VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(video);

        video.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Asserts that a video short has a null or empty error message.
    /// </summary>
    /// <param name="video">The video short to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when video is null.</exception>
    public static void ShouldNotHaveError(
        this VideoShortModelTests _,
        VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(video);

        video.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Asserts that a video short has been processed by checking the ProcessedAt timestamp.
    /// </summary>
    /// <param name="video">The video short to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when video is null.</exception>
    public static void ShouldBeProcessed(
        this VideoShortModelTests _,
        VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(video);

        video.Status.Should().Be(ProcessingStatus.Completed);
        video.ProcessedAt.Should().NotBeNull();
        video.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Gets all validation error messages for a video short.
    /// </summary>
    /// <param name="video">The video short to validate.</param>
    /// <returns>An enumerable of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null.</exception>
    public static IEnumerable<string> GetValidationErrors(
        this VideoShortModelTests _,
        VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(video);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(video.Title) || video.Title.Length > 100)
        {
            errors.Add("Title is invalid");
        }

        if (video.Description.Length > 5000)
        {
            errors.Add("Description exceeds maximum length");
        }

        if (string.IsNullOrWhiteSpace(video.FilePath))
        {
            errors.Add("FilePath is required");
        }

        if (video.Duration.TotalSeconds < 1 || video.Duration.TotalSeconds > 60)
        {
            errors.Add("Duration must be between 1 and 60 seconds");
        }

        if (video.ProcessingProfileId <= 0 || video.YouTubeChannelId <= 0)
        {
            errors.Add("ProcessingProfileId and YouTubeChannelId must be positive");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Creates a collection of video shorts for batch testing scenarios.
    /// </summary>
    /// <param name="count">The number of video shorts to create.</param>
    /// <param name="status">The status to set for all videos.</param>
    /// <returns>An enumerable of video shorts.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0.</exception>
    public static IEnumerable<VideoShort> CreateVideoShortCollection(
        this VideoShortModelTests _,
        int count,
        Constants.ProcessingStatus status = Constants.ProcessingStatus.Pending)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");
        }

        for (var i = 0; i < count; i++)
        {
            yield return new VideoShort
            {
                Title = $"Test Video Short {i}",
                Description = $"Test description {i}",
                FilePath = $"/tmp/test{i}.mp4",
                Duration = TimeSpan.FromSeconds(30),
                ProcessingProfileId = 1,
                YouTubeChannelId = 1,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}