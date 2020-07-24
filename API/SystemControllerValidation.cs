// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for SystemController response models
// Provides validation, IsValid, and EnsureValid methods for system-related data models
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for SystemController response models
/// </summary>
public static class SystemControllerValidation
{
    /// <summary>
    /// Validates a HealthCheckResponse instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="response">The response to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null</exception>
    public static IReadOnlyList<string> Validate(this HealthCheckResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var errors = new List<string>();

        // Validate Status
        if (string.IsNullOrWhiteSpace(response.Status))
        {
            errors.Add("Status must be specified.");
        }
        else if (response.Status.Length > 50)
        {
            errors.Add("Status must be 50 characters or less.");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(response.Version))
        {
            errors.Add("Version must be specified.");
        }
        else if (response.Version.Length > 20)
        {
            errors.Add("Version must be 20 characters or less.");
        }

        // Validate Environment
        if (string.IsNullOrWhiteSpace(response.Environment))
        {
            errors.Add("Environment must be specified.");
        }
        else if (response.Environment.Length > 50)
        {
            errors.Add("Environment must be 50 characters or less.");
        }

        // Validate Timestamp
        if (response.Timestamp == default)
        {
            errors.Add("Timestamp must be a valid DateTime.");
        }
        else if (response.Timestamp.Kind != DateTimeKind.Utc)
        {
            errors.Add("Timestamp must be in UTC format.");
        }
        else if (response.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Timestamp cannot be in the future by more than 5 minutes.");
        }
        else if (response.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("Timestamp cannot be older than 1 year.");
        }

        // Validate Checks
        if (response.Checks == null)
        {
            errors.Add("Checks dictionary must be initialized.");
        }
        else if (response.Checks.Count == 0)
        {
            errors.Add("Checks dictionary must contain at least one health check.");
        }
        else
        {
            foreach (var check in response.Checks)
            {
                if (string.IsNullOrWhiteSpace(check.Key))
                {
                    errors.Add("Health check key must be specified.");
                    break;
                }

                if (check.Value == null)
                {
                    errors.Add($"Health check '{check.Key}' must be initialized.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(check.Value.Status))
                {
                    errors.Add($"Status must be specified for health check '{check.Key}'.");
                }
                else if (check.Value.Status.Length > 20)
                {
                    errors.Add($"Status must be 20 characters or less for health check '{check.Key}'.");
                }

                if (string.IsNullOrWhiteSpace(check.Value.ResponseTime))
                {
                    errors.Add($"ResponseTime must be specified for health check '{check.Key}'.");
                }
                else if (!TimeSpan.TryParse(check.Value.ResponseTime.Replace("ms", string.Empty), CultureInfo.InvariantCulture, out _) &&
                         !TimeSpan.TryParse(check.Value.ResponseTime, CultureInfo.InvariantCulture, out _))
                {
                    errors.Add($"ResponseTime must be a valid time format for health check '{check.Key}'. Expected format: '5ms' or '00:00:05'.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified HealthCheckResponse instance is valid
    /// </summary>
    /// <param name="response">The response to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this HealthCheckResponse response)
    {
        return Validate(response).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified HealthCheckResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="response">The response to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when response is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this HealthCheckResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var errors = Validate(response);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"HealthCheckResponse validation failed:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
        }
    }

    /// <summary>
    /// Validates a VersionInfo instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="info">The version info to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when info is null</exception>
    public static IReadOnlyList<string> Validate(this VersionInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var errors = new List<string>();

        // Validate ApiVersion
        if (string.IsNullOrWhiteSpace(info.ApiVersion))
        {
            errors.Add("ApiVersion must be specified.");
        }
        else if (info.ApiVersion.Length > 20)
        {
            errors.Add("ApiVersion must be 20 characters or less.");
        }

        // Validate AppVersion
        if (string.IsNullOrWhiteSpace(info.AppVersion))
        {
            errors.Add("AppVersion must be specified.");
        }
        else if (info.AppVersion.Length > 20)
        {
            errors.Add("AppVersion must be 20 characters or less.");
        }

        // Validate BuildDate
        if (string.IsNullOrWhiteSpace(info.BuildDate))
        {
            errors.Add("BuildDate must be specified.");
        }
        else if (!DateTime.TryParse(info.BuildDate, CultureInfo.InvariantCulture, out _) &&
                 !DateTime.TryParseExact(info.BuildDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add("BuildDate must be a valid date in format 'yyyy-MM-dd' or ISO 8601.");
        }

        // Validate GitCommit
        if (string.IsNullOrWhiteSpace(info.GitCommit))
        {
            errors.Add("GitCommit must be specified.");
        }
        else if (info.GitCommit.Length > 50)
        {
            errors.Add("GitCommit must be 50 characters or less.");
        }

        // Validate SupportedApiVersions
        if (info.SupportedApiVersions == null)
        {
            errors.Add("SupportedApiVersions array must be initialized.");
        }
        else if (info.SupportedApiVersions.Length == 0)
        {
            errors.Add("SupportedApiVersions must contain at least one version.");
        }
        else
        {
            foreach (var version in info.SupportedApiVersions)
            {
                if (string.IsNullOrWhiteSpace(version))
                {
                    errors.Add("SupportedApiVersions array must not contain null or empty strings.");
                    break;
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified VersionInfo instance is valid
    /// </summary>
    /// <param name="info">The version info to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this VersionInfo info)
    {
        return Validate(info).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified VersionInfo instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="info">The version info to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when info is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this VersionInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var errors = Validate(info);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"VersionInfo validation failed:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
        }
    }

    /// <summary>
    /// Validates a SystemInfoResponse instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="info">The system info to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when info is null</exception>
    public static IReadOnlyList<string> Validate(this SystemInfoResponse info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var errors = new List<string>();

        // Validate OperatingSystem
        if (string.IsNullOrWhiteSpace(info.OperatingSystem))
        {
            errors.Add("OperatingSystem must be specified.");
        }
        else if (info.OperatingSystem.Length > 100)
        {
            errors.Add("OperatingSystem must be 100 characters or less.");
        }

        // Validate DotNetVersion
        if (string.IsNullOrWhiteSpace(info.DotNetVersion))
        {
            errors.Add("DotNetVersion must be specified.");
        }
        else if (info.DotNetVersion.Length > 50)
        {
            errors.Add("DotNetVersion must be 50 characters or less.");
        }

        // Validate ProcessorCount
        if (info.ProcessorCount <= 0)
        {
            errors.Add("ProcessorCount must be a positive integer.");
        }
        else if (info.ProcessorCount > 1024)
        {
            errors.Add("ProcessorCount must be 1024 or less.");
        }

        // Validate TotalMemoryMb
        if (info.TotalMemoryMb <= 0)
        {
            errors.Add("TotalMemoryMb must be a positive integer.");
        }
        else if (info.TotalMemoryMb > 1_048_576) // 1TB
        {
            errors.Add("TotalMemoryMb must be 1TB or less.");
        }

        // Validate Uptime
        if (string.IsNullOrWhiteSpace(info.Uptime))
        {
            errors.Add("Uptime must be specified.");
        }
        else if (info.Uptime.Length > 50)
        {
            errors.Add("Uptime must be 50 characters or less.");
        }

        // Validate TimeZone
        if (string.IsNullOrWhiteSpace(info.TimeZone))
        {
            errors.Add("TimeZone must be specified.");
        }
        else if (info.TimeZone.Length > 100)
        {
            errors.Add("TimeZone must be 100 characters or less.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified SystemInfoResponse instance is valid
    /// </summary>
    /// <param name="info">The system info to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this SystemInfoResponse info)
    {
        return Validate(info).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified SystemInfoResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="info">The system info to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when info is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this SystemInfoResponse info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var errors = Validate(info);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SystemInfoResponse validation failed:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
        }
    }

    /// <summary>
    /// Validates a FeaturesResponse instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="features">The features to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when features is null</exception>
    public static IReadOnlyList<string> Validate(this FeaturesResponse features)
    {
        ArgumentNullException.ThrowIfNull(features);

        var errors = new List<string>();

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified FeaturesResponse instance is valid
    /// </summary>
    /// <param name="features">The features to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this FeaturesResponse features)
    {
        return Validate(features).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified FeaturesResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="features">The features to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when features is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this FeaturesResponse features)
    {
        ArgumentNullException.ThrowIfNull(features);

        var errors = Validate(features);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"FeaturesResponse validation failed:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
        }
    }
}