using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Extension methods that provide useful helpers for <see cref="AppSettings"/>.
/// </summary>
public static class AppSettingsExtensions
{
    /// <summary>
    /// Returns the absolute path to the default log file (named <c>app.log</c>) inside the configured <see cref="AppSettings.LogDirectory"/>.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>The full file path to the log file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <see cref="AppSettings.LogDirectory"/> is <c>null</c> or empty.</exception>
    public static string GetLogFilePath(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrEmpty(settings.LogDirectory);

        // Combine the directory with the default file name and resolve to an absolute path.
        return Path.GetFullPath(Path.Combine(settings.LogDirectory, "app.log"));
    }

    /// <summary>
    /// Retrieves the YouTube OAuth client credentials as a tuple.
    /// </summary>
    /// <param name="settings">The application settings.</param>
    /// <returns>A tuple containing <c>ClientId</c> and <c>ClientSecret</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when either <see cref="AppSettings.YouTubeClientId"/> or <see cref="AppSettings.YouTubeClientSecret"/> is <c>null</c> or empty.
    /// </exception>
    public static (string ClientId, string ClientSecret) GetYouTubeCredentials(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrEmpty(settings.YouTubeClientId);
        ArgumentException.ThrowIfNullOrEmpty(settings.YouTubeClientSecret);

        return (settings.YouTubeClientId, settings.YouTubeClientSecret);
    }

    /// <summary>
    /// Validates the configuration and returns a read‑only list of validation error messages.
    /// An empty list indicates that the settings are valid.
    /// </summary>
    /// <param name="settings">The application settings to validate.</param>
    /// <returns>A read‑only list of error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var errors = new List<string>();

        // String properties that must be non‑empty
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            errors.Add("ConnectionString must not be empty.");

        if (string.IsNullOrWhiteSpace(settings.DatabasePath))
            errors.Add("DatabasePath must not be empty.");

        if (string.IsNullOrWhiteSpace(settings.LogDirectory))
            errors.Add("LogDirectory must not be empty.");

        if (string.IsNullOrWhiteSpace(settings.ProcessingDirectory))
            errors.Add("ProcessingDirectory must not be empty.");

        if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
            errors.Add("OutputDirectory must not be empty.");

        if (string.IsNullOrWhiteSpace(settings.YouTubeApiKey))
            errors.Add("YouTubeApiKey must not be empty.");

        // Numeric ranges
        if (settings.MaxConcurrentUploads <= 0)
            errors.Add("MaxConcurrentUploads must be greater than zero.");

        if (settings.MaxConcurrentProcessing <= 0)
            errors.Add("MaxConcurrentProcessing must be greater than zero.");

        if (settings.DefaultRetryCount < 0)
            errors.Add("DefaultRetryCount cannot be negative.");

        if (settings.UploadTimeoutSeconds <= 0)
            errors.Add("UploadTimeoutSeconds must be greater than zero.");

        if (settings.ProcessingQueueLimit <= 0)
            errors.Add("ProcessingQueueLimit must be greater than zero.");

        if (settings.AnalyticsSyncIntervalHours <= 0)
            errors.Add("AnalyticsSyncIntervalHours must be greater than zero.");

        if (settings.ScheduleCheckIntervalSeconds <= 0)
            errors.Add("ScheduleCheckIntervalSeconds must be greater than zero.");

        // Watermark validation
        if (settings.EnableWatermark && string.IsNullOrWhiteSpace(settings.WatermarkImagePath))
            errors.Add("WatermarkImagePath must be provided when EnableWatermark is true.");

        return errors.AsReadOnly();
    }
}
