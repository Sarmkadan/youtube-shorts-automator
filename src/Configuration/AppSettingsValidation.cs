namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="AppSettings"/> configuration.
/// </summary>
public static class AppSettingsValidation
{
    /// <summary>
    /// Validates the configuration settings and returns a list of validation problems.
    /// </summary>
    /// <param name="settings">The configuration to validate.</param>
    /// <returns>A read-only list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = new List<string>();

        // Validate required strings
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            problems.Add("ConnectionString is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(settings.DatabasePath))
        {
            problems.Add("DatabasePath is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(settings.LogDirectory))
        {
            problems.Add("LogDirectory is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(settings.ProcessingDirectory))
        {
            problems.Add("ProcessingDirectory is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
        {
            problems.Add("OutputDirectory is required and cannot be empty or whitespace.");
        }

        // Validate YouTube API credentials
        if (string.IsNullOrWhiteSpace(settings.YouTubeApiKey))
        {
            problems.Add("YouTubeApiKey is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(settings.YouTubeClientId))
        {
            problems.Add("YouTubeClientId is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(settings.YouTubeClientSecret))
        {
            problems.Add("YouTubeClientSecret is required and cannot be empty or whitespace.");
        }

        // Validate optional watermark path
        if (settings.EnableWatermark && string.IsNullOrWhiteSpace(settings.WatermarkImagePath))
        {
            problems.Add("WatermarkImagePath is required when EnableWatermark is true.");
        }

        // Validate numeric ranges
        if (settings.MaxConcurrentUploads <= 0)
        {
            problems.Add($"MaxConcurrentUploads must be positive, but was {settings.MaxConcurrentUploads}.");
        }

        if (settings.MaxConcurrentProcessing <= 0)
        {
            problems.Add($"MaxConcurrentProcessing must be positive, but was {settings.MaxConcurrentProcessing}.");
        }

        if (settings.DefaultRetryCount < 0)
        {
            problems.Add($"DefaultRetryCount cannot be negative, but was {settings.DefaultRetryCount}.");
        }

        if (settings.UploadTimeoutSeconds <= 0)
        {
            problems.Add($"UploadTimeoutSeconds must be positive, but was {settings.UploadTimeoutSeconds}.");
        }

        if (settings.ProcessingQueueLimit <= 0)
        {
            problems.Add($"ProcessingQueueLimit must be positive, but was {settings.ProcessingQueueLimit}.");
        }

        if (settings.AnalyticsSyncIntervalHours <= 0)
        {
            problems.Add($"AnalyticsSyncIntervalHours must be positive, but was {settings.AnalyticsSyncIntervalHours}.");
        }

        if (settings.ScheduleCheckIntervalSeconds <= 0)
        {
            problems.Add($"ScheduleCheckIntervalSeconds must be positive, but was {settings.ScheduleCheckIntervalSeconds}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the configuration settings are valid.
    /// </summary>
    /// <param name="settings">The configuration to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static bool IsValid(this AppSettings settings)
    {
        return settings.Validate().Count == 0;
    }

    /// <summary>
    /// Validates the configuration settings and throws an <see cref="ArgumentException"/> if invalid.
    /// </summary>
    /// <param name="settings">The configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = settings.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AppSettings validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}