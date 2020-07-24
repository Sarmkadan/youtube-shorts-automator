// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ConfigurationController response models
// Provides validation for configuration API response data integrity
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for ConfigurationController response models
/// Validates configuration response data integrity
/// </summary>
public static class ConfigurationControllerValidation
{
    /// <summary>
    /// Validates a ConfigurationInfo instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ConfigurationInfo instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ConfigurationInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("Version cannot be null or whitespace.");
        }
        else if (value.Version == "0.0.0" || value.Version == "1.0.0-dev")
        {
            problems.Add("Version appears to be a default or development value.");
        }

        // Validate Environment
        if (string.IsNullOrWhiteSpace(value.Environment))
        {
            problems.Add("Environment cannot be null or whitespace.");
        }
        else if (!string.Equals(value.Environment, "Development", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(value.Environment, "Staging", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(value.Environment, "Production", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("Environment must be Development, Staging, or Production.");
        }

        // Validate MaxUploadSizeMb
        if (value.MaxUploadSizeMb <= 0)
        {
            problems.Add("MaxUploadSizeMb must be greater than 0.");
        }
        else if (value.MaxUploadSizeMb > 102400) // 100GB
        {
            problems.Add("MaxUploadSizeMb exceeds reasonable maximum of 102400 MB (100 GB).");
        }

        // Validate SupportedVideoFormats
        if (value.SupportedVideoFormats is null)
        {
            problems.Add("SupportedVideoFormats cannot be null.");
        }
        else if (value.SupportedVideoFormats.Length == 0)
        {
            problems.Add("SupportedVideoFormats must contain at least one format.");
        }
        else
        {
            for (var i = 0; i < value.SupportedVideoFormats.Length; i++)
            {
                var format = value.SupportedVideoFormats[i];
                if (string.IsNullOrWhiteSpace(format))
                {
                    problems.Add($"SupportedVideoFormats[{i}] cannot be null or whitespace.");
                }
            }
        }

        // Validate ProcessingProfiles
        if (value.ProcessingProfiles is not null)
        {
            for (var i = 0; i < value.ProcessingProfiles.Length; i++)
            {
                var profile = value.ProcessingProfiles[i];
                if (string.IsNullOrWhiteSpace(profile))
                {
                    problems.Add($"ProcessingProfiles[{i}] cannot be null or whitespace.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a StorageConfig instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The StorageConfig instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this StorageConfig value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate TempDirectoryPath
        if (string.IsNullOrWhiteSpace(value.TempDirectoryPath))
        {
            problems.Add("TempDirectoryPath cannot be null or whitespace.");
        }
        else if (value.TempDirectoryPath.Length > 260)
        {
            problems.Add("TempDirectoryPath exceeds maximum path length of 260 characters.");
        }

        // Validate VideoDirectoryPath
        if (string.IsNullOrWhiteSpace(value.VideoDirectoryPath))
        {
            problems.Add("VideoDirectoryPath cannot be null or whitespace.");
        }
        else if (value.VideoDirectoryPath.Length > 260)
        {
            problems.Add("VideoDirectoryPath exceeds maximum path length of 260 characters.");
        }

        // Validate MaxStorageGb
        if (value.MaxStorageGb <= 0)
        {
            problems.Add("MaxStorageGb must be greater than 0.");
        }
        else if (value.MaxStorageGb > 100000) // 100TB
        {
            problems.Add("MaxStorageGb exceeds reasonable maximum of 100000 GB (100 TB).");
        }

        // Validate CurrentUsageGb
        if (value.CurrentUsageGb < 0)
        {
            problems.Add("CurrentUsageGb cannot be negative.");
        }
        else if (value.CurrentUsageGb > value.MaxStorageGb)
        {
            problems.Add("CurrentUsageGb cannot exceed MaxStorageGb.");
        }

        // Validate RetentionDays
        if (value.RetentionDays <= 0)
        {
            problems.Add("RetentionDays must be greater than 0.");
        }
        else if (value.RetentionDays > 3650) // 10 years
        {
            problems.Add("RetentionDays exceeds reasonable maximum of 3650 days (10 years).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a ProcessingSettings instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ProcessingSettings instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ProcessingSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ParallelJobLimit
        if (value.ParallelJobLimit <= 0)
        {
            problems.Add("ParallelJobLimit must be greater than 0.");
        }
        else if (value.ParallelJobLimit > 1000)
        {
            problems.Add("ParallelJobLimit exceeds reasonable maximum of 1000.");
        }

        // Validate TimeoutMinutes
        if (value.TimeoutMinutes <= 0)
        {
            problems.Add("TimeoutMinutes must be greater than 0.");
        }
        else if (value.TimeoutMinutes > 1440) // 24 hours
        {
            problems.Add("TimeoutMinutes exceeds reasonable maximum of 1440 minutes (24 hours).");
        }

        // Validate RetryAttempts
        if (value.RetryAttempts < 0)
        {
            problems.Add("RetryAttempts cannot be negative.");
        }

        // Validate DefaultProfile
        if (string.IsNullOrWhiteSpace(value.DefaultProfile))
        {
            problems.Add("DefaultProfile cannot be null or whitespace.");
        }
        else if (value.DefaultProfile.Length > 50)
        {
            problems.Add("DefaultProfile must be 50 characters or less.");
        }

        // Validate FfmpegPath
        if (string.IsNullOrWhiteSpace(value.FfmpegPath))
        {
            problems.Add("FfmpegPath cannot be null or whitespace.");
        }
        else if (value.FfmpegPath.Length > 260)
        {
            problems.Add("FfmpegPath exceeds maximum path length of 260 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a YouTubeIntegrationStatus instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The YouTubeIntegrationStatus instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this YouTubeIntegrationStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate AuthMethod
        if (string.IsNullOrWhiteSpace(value.AuthMethod))
        {
            problems.Add("AuthMethod cannot be null or whitespace.");
        }
        else if (value.AuthMethod.Length > 50)
        {
            problems.Add("AuthMethod must be 50 characters or less.");
        }

        // Validate LastSyncUtc
        if (value.LastSyncUtc == default)
        {
            problems.Add("LastSyncUtc must be a valid DateTime.");
        }

        // Validate ConnectedChannels
        if (value.ConnectedChannels < 0)
        {
            problems.Add("ConnectedChannels cannot be negative.");
        }

        // Validate UploadQuotaRemaining
        if (value.UploadQuotaRemaining < 0)
        {
            problems.Add("UploadQuotaRemaining cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a HealthStatus instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The HealthStatus instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this HealthStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Uptime format
        if (string.IsNullOrWhiteSpace(value.Uptime))
        {
            problems.Add("Uptime cannot be null or whitespace.");
        }
        else if (!TimeSpan.TryParseExact(value.Uptime, (@"hh\:mm\:ss"), CultureInfo.InvariantCulture, out _))
        {
            problems.Add("Uptime must be in the format hh:mm:ss.");
        }

        // Validate LastCheckUtc
        if (value.LastCheckUtc == default)
        {
            problems.Add("LastCheckUtc must be a valid DateTime.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a FormatsInfo instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The FormatsInfo instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this FormatsInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate VideoFormats collection
        if (value.VideoFormats is null)
        {
            problems.Add("VideoFormats collection is null.");
        }
        else
        {
            for (var i = 0; i < value.VideoFormats.Length; i++)
            {
                var format = value.VideoFormats[i];
                if (format is null)
                {
                    problems.Add($"VideoFormats[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(format.Name))
                {
                    problems.Add($"VideoFormats[{i}].Name cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(format.Extension))
                {
                    problems.Add($"VideoFormats[{i}].Extension cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(format.MimeType))
                {
                    problems.Add($"VideoFormats[{i}].MimeType cannot be null or whitespace.");
                }
            }
        }

        // Validate AudioFormats collection
        if (value.AudioFormats is not null)
        {
            for (var i = 0; i < value.AudioFormats.Length; i++)
            {
                var format = value.AudioFormats[i];
                if (format is null)
                {
                    problems.Add($"AudioFormats[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(format.Name))
                {
                    problems.Add($"AudioFormats[{i}].Name cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(format.Extension))
                {
                    problems.Add($"AudioFormats[{i}].Extension cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(format.MimeType))
                {
                    problems.Add($"AudioFormats[{i}].MimeType cannot be null or whitespace.");
                }
            }
        }

        // Validate Codecs collection
        if (value.Codecs is not null)
        {
            for (var i = 0; i < value.Codecs.Length; i++)
            {
                var codec = value.Codecs[i];
                if (string.IsNullOrWhiteSpace(codec))
                {
                    problems.Add($"Codecs[{i}] cannot be null or whitespace.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified ConfigurationInfo instance is valid
    /// </summary>
    /// <param name="value">The ConfigurationInfo instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this ConfigurationInfo value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified StorageConfig instance is valid
    /// </summary>
    /// <param name="value">The StorageConfig instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this StorageConfig value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified ProcessingSettings instance is valid
    /// </summary>
    /// <param name="value">The ProcessingSettings instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this ProcessingSettings value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified YouTubeIntegrationStatus instance is valid
    /// </summary>
    /// <param name="value">The YouTubeIntegrationStatus instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this YouTubeIntegrationStatus value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified HealthStatus instance is valid
    /// </summary>
    /// <param name="value">The HealthStatus instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this HealthStatus value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether the specified FormatsInfo instance is valid
    /// </summary>
    /// <param name="value">The FormatsInfo instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this FormatsInfo value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified ConfigurationInfo instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The ConfigurationInfo instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this ConfigurationInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ConfigurationInfo validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified StorageConfig instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The StorageConfig instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this StorageConfig value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"StorageConfig validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified ProcessingSettings instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The ProcessingSettings instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this ProcessingSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingSettings validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified YouTubeIntegrationStatus instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The YouTubeIntegrationStatus instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this YouTubeIntegrationStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"YouTubeIntegrationStatus validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified HealthStatus instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The HealthStatus instance to check</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this HealthStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HealthStatus validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified FormatsInfo instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The FormatsInfo instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this FormatsInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FormatsInfo validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}