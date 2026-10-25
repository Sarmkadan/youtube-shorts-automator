// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace YouTubeShortAutomator.Constants;

/// <summary>
/// Extension methods for <see cref="VideoQuality"/> providing encoding hints such as resolution and bitrate.
/// </summary>
public static class VideoQualityExtensions
{
    /// <summary>
    /// Returns the short resolution label for the quality level, such as <c>"720p"</c>.
    /// </summary>
    /// <param name="quality">The video quality level.</param>
    /// <returns>A label such as <c>"360p"</c>, <c>"720p"</c>, <c>"1080p"</c> or <c>"1440p"</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is not a defined value.</exception>
    public static string GetResolutionLabel(this VideoQuality quality)
    {
        return quality switch
        {
            VideoQuality.Low => "360p",
            VideoQuality.Medium => "720p",
            VideoQuality.High => "1080p",
            VideoQuality.UltraHD => "1440p",
            _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unknown video quality.")
        };
    }

    /// <summary>
    /// Returns the frame height in pixels for the quality level.
    /// </summary>
    /// <param name="quality">The video quality level.</param>
    /// <returns>The height in pixels: 360, 720, 1080 or 1440.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is not a defined value.</exception>
    public static int GetHeightPixels(this VideoQuality quality)
    {
        return quality switch
        {
            VideoQuality.Low => 360,
            VideoQuality.Medium => 720,
            VideoQuality.High => 1080,
            VideoQuality.UltraHD => 1440,
            _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unknown video quality.")
        };
    }

    /// <summary>
    /// Returns the frame width in pixels for the quality level, assuming a 16:9 landscape frame.
    /// </summary>
    /// <param name="quality">The video quality level.</param>
    /// <returns>The width in pixels: 640, 1280, 1920 or 2560.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is not a defined value.</exception>
    public static int GetWidthPixels(this VideoQuality quality)
    {
        return quality switch
        {
            VideoQuality.Low => 640,
            VideoQuality.Medium => 1280,
            VideoQuality.High => 1920,
            VideoQuality.UltraHD => 2560,
            _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unknown video quality.")
        };
    }

    /// <summary>
    /// Returns the suggested target video bitrate in kilobits per second for the quality level.
    /// </summary>
    /// <param name="quality">The video quality level.</param>
    /// <returns>The bitrate in kbps: 500, 2000, 5000 or 10000.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is not a defined value.</exception>
    public static int GetTargetBitrateKbps(this VideoQuality quality)
    {
        return quality switch
        {
            VideoQuality.Low => 500,
            VideoQuality.Medium => 2000,
            VideoQuality.High => 5000,
            VideoQuality.UltraHD => 10000,
            _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, "Unknown video quality.")
        };
    }

    /// <summary>
    /// Returns the approximate maximum bitrate in kilobits per second for the quality level.
    /// Set to 1.5 times the target bitrate, which leaves headroom for scene complexity spikes.
    /// </summary>
    /// <param name="quality">The video quality level.</param>
    /// <returns>The maximum bitrate in kbps.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is not a defined value.</exception>
    public static int GetMaxBitrateKbps(this VideoQuality quality)
    {
        return quality.GetTargetBitrateKbps() * 3 / 2;
    }

    /// <summary>
    /// Determines whether the quality level is at or above the given threshold.
    /// </summary>
    /// <param name="quality">The video quality level to check.</param>
    /// <param name="threshold">The minimum quality level.</param>
    /// <returns><c>true</c> if <paramref name="quality"/> is at least <paramref name="threshold"/>; otherwise <c>false</c>.</returns>
    public static bool IsAtLeast(this VideoQuality quality, VideoQuality threshold)
    {
        return quality >= threshold;
    }
}
