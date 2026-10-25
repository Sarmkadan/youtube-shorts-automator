// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Schedule settings, bound from the <c>Schedule</c> configuration section.
/// </summary>
public sealed class ScheduleOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Schedule";

    /// <summary>Seconds between checks for due scheduled uploads.</summary>
    public int CheckIntervalSeconds { get; set; } = ApplicationConstants.OptionLimits.DefaultScheduleCheckIntervalSeconds;
}
