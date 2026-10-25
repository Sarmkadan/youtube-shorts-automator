// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

/// <summary>
/// Output quality presets for processed videos, from lowest to highest resolution and bitrate.
/// </summary>
public enum VideoQuality
{
    /// <summary>
    /// 360p output at roughly 500 kbps.
    /// </summary>
    Low = 1,           // 360p, 500kbps

    /// <summary>
    /// 720p output at roughly 2000 kbps.
    /// </summary>
    Medium = 2,        // 720p, 2000kbps

    /// <summary>
    /// 1080p output at roughly 5000 kbps.
    /// </summary>
    High = 3,          // 1080p, 5000kbps

    /// <summary>
    /// 1440p output at roughly 10000 kbps.
    /// </summary>
    UltraHD = 4        // 1440p, 10000kbps
}
