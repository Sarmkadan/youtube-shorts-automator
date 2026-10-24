// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Tests;

/// <summary>
/// Clock whose current time only moves when the test advances it
/// </summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public ManualTimeProvider(DateTimeOffset start)
    {
        _now = start;
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}

/// <summary>
/// Upload double that records every call in order and fails for videos marked with <see cref="FailFor"/>
/// </summary>
internal sealed class RecordingVideoUploader : IVideoUploader
{
    private readonly HashSet<Guid> _failingVideos = new();

    public List<Guid> UploadedVideoIds { get; } = new();

    public void FailFor(Guid videoId) => _failingVideos.Add(videoId);

    public async Task<UploadResult> UploadWithRetryAsync(Guid videoId, Guid userId)
    {
        UploadedVideoIds.Add(videoId);
        await Task.Yield();

        if (_failingVideos.Contains(videoId))
        {
            throw new UploadException("Simulated YouTube failure");
        }

        return new UploadResult { VideoId = videoId, Status = UploadStatus.Completed };
    }
}
