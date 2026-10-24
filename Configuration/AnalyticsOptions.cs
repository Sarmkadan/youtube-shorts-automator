// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Analytics settings, bound from the <c>Analytics</c> configuration section.
/// </summary>
public class AnalyticsOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Analytics";

    /// <summary>Minutes between analytics polls of the YouTube API.</summary>
    public int PollingIntervalMinutes { get; set; } = ApplicationConstants.OptionLimits.DefaultAnalyticsPollingIntervalMinutes;
}
