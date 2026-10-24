// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Services;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Settings for <see cref="VideoProcessor"/>. The FFmpeg tool paths are read from the shared
/// <c>Processing</c> section; the remaining values come from the <c>VideoProcessor</c> section.
/// Validated at startup by <see cref="VideoProcessorOptionsValidation"/>.
/// </summary>
public sealed class VideoProcessorOptions
{
    /// <summary>Configuration section that holds these settings.</summary>
    public const string SectionName = "VideoProcessor";

    /// <summary>
    /// Path to the ffmpeg executable, or a bare command name that is resolved through PATH.
    /// Bound from <c>Processing:FFmpegPath</c>. Defaults to <c>ffmpeg</c>.
    /// </summary>
    public string FfmpegPath { get; set; } = "ffmpeg";

    /// <summary>
    /// Path to the ffprobe executable, or a bare command name that is resolved through PATH.
    /// Bound from <c>Processing:FFprobePath</c>. Defaults to <c>ffprobe</c>.
    /// </summary>
    public string FfprobePath { get; set; } = "ffprobe";

    /// <summary>Directory that processed shorts are written to. Must exist or be creatable, and be writable.</summary>
    public string OutputDirectory { get; set; } = Constants.Constants.OUTPUT_DIRECTORY;

    /// <summary>
    /// Maximum output duration in seconds, between 1 and 60. Defaults to 59 so that shorts stay under 60 seconds.
    /// </summary>
    public int MaxDurationSeconds { get; set; } = VideoProcessor.MaxOutputSeconds;

    /// <summary>Output width in pixels. Together with <see cref="TargetHeight"/> must form a 9:16 size.</summary>
    public int TargetWidth { get; set; } = VideoProcessor.OutputWidth;

    /// <summary>Output height in pixels. Together with <see cref="TargetWidth"/> must form a 9:16 size.</summary>
    public int TargetHeight { get; set; } = VideoProcessor.OutputHeight;
}
