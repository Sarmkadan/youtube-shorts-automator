// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Settings for <see cref="Services.ThumbnailGeneratorService"/>. The FFmpeg path and timeout are read from the
/// shared <c>Processing</c> section; the default quality comes from the <c>ThumbnailGenerator</c> section.
/// Validated at startup by <see cref="ThumbnailGeneratorOptionsValidation"/>.
/// </summary>
public sealed class ThumbnailGeneratorOptions
{
    /// <summary>Configuration section that holds these settings.</summary>
    public const string SectionName = "ThumbnailGenerator";

    /// <summary>
    /// Path to the ffmpeg executable, or a bare command name that is resolved through PATH.
    /// Bound from <c>Processing:FFmpegPath</c>. Defaults to <c>ffmpeg</c>.
    /// </summary>
    public string FfmpegPath { get; set; } = "ffmpeg";

    /// <summary>
    /// Maximum time in seconds that a single FFmpeg invocation may run.
    /// Bound from <c>Processing:FFmpegTimeoutSeconds</c>. Defaults to <c>120</c>.
    /// </summary>
    public int FfmpegTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Default JPEG/WebP quality, between 1 and 100, used by <see cref="Services.ThumbnailGeneratorService.CreateDefaultRequest"/>.
    /// Defaults to <c>85</c>.
    /// </summary>
    public int DefaultQuality { get; set; } = 85;
}
