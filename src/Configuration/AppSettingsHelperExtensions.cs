namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Extension methods for <see cref="AppSettings"/> to provide helper functionality.
/// </summary>
public static class AppSettingsHelperExtensions
{
    /// <summary>
    /// Determines whether YouTube API credentials are configured.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>True if YouTube API key, client ID, and client secret are all non-empty; otherwise false.</returns>
    public static bool IsYouTubeConfigured(this AppSettings settings)
    {
        return !string.IsNullOrWhiteSpace(settings.YouTubeApiKey) &&
               !string.IsNullOrWhiteSpace(settings.YouTubeClientId) &&
               !string.IsNullOrWhiteSpace(settings.YouTubeClientSecret);
    }

    /// <summary>
    /// Determines whether analytics synchronization is enabled and configured with a valid interval.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>True if analytics syncing is enabled and interval is greater than zero; otherwise false.</returns>
    public static bool IsAnalyticsEnabled(this AppSettings settings)
    {
        return settings.EnableAnalyticsSyncing && settings.AnalyticsSyncIntervalHours > 0;
    }

    /// <summary>
    /// Gets the upload timeout as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>The upload timeout as a TimeSpan.</returns>
    public static TimeSpan GetUploadTimeout(this AppSettings settings)
    {
        return TimeSpan.FromSeconds(settings.UploadTimeoutSeconds);
    }

    /// <summary>
    /// Gets the schedule check interval as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>The schedule check interval as a TimeSpan.</returns>
    public static TimeSpan GetScheduleCheckInterval(this AppSettings settings)
    {
        return TimeSpan.FromSeconds(settings.ScheduleCheckIntervalSeconds);
    }

    /// <summary>
    /// Determines whether a watermark is enabled and a watermark image path is specified.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>True if watermark is enabled and a non-empty watermark image path is provided; otherwise false.</returns>
    public static bool HasWatermark(this AppSettings settings)
    {
        return settings.EnableWatermark && !string.IsNullOrWhiteSpace(settings.WatermarkImagePath);
    }
}