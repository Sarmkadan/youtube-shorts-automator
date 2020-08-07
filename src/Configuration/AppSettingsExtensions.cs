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
}
