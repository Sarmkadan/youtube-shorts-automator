// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// Provides validation helpers for HealthController and HealthStatus objects
/// </summary>
public static class HealthControllerValidation
{
    /// <summary>
    /// Validates a HealthStatus object and returns any validation problems
    /// </summary>
    /// <param name="value">The HealthStatus object to validate</param>
    /// <returns>Collection of human-readable validation problems, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this YouTubeShortsAutomator.Controllers.HealthStatus? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            problems.Add("Status must not be null or whitespace");
        }
        else if (value.Status.Length > 100)
        {
            problems.Add("Status must be 100 characters or less");
        }

        // Validate Database
        if (string.IsNullOrWhiteSpace(value.Database))
        {
            problems.Add("Database must not be null or whitespace");
        }
        else if (value.Database.Length > 100)
        {
            problems.Add("Database must be 100 characters or less");
        }

        // Validate Configuration
        if (string.IsNullOrWhiteSpace(value.Configuration))
        {
            problems.Add("Configuration must not be null or whitespace");
        }
        else if (value.Configuration.Length > 100)
        {
            problems.Add("Configuration must be 100 characters or less");
        }

        // Validate ConfigurationErrors
        if (value.ConfigurationErrors is not null)
        {
            if (value.ConfigurationErrors.Count == 0)
            {
                problems.Add("ConfigurationErrors collection must be null or contain items, not empty");
            }
            else if (value.ConfigurationErrors.Count > 1000)
            {
                problems.Add("ConfigurationErrors collection must contain 1000 items or less");
            }
            else
            {
                for (int i = 0; i < value.ConfigurationErrors.Count; i++)
                {
                    var error = value.ConfigurationErrors[i];
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        problems.Add($"ConfigurationErrors[{i}] must not be null or whitespace");
                    }
                    else if (error.Length > 500)
                    {
                        problems.Add($"ConfigurationErrors[{i}] must be 500 characters or less");
                    }
                }
            }
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            problems.Add("Timestamp must not be default(DateTime)");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Timestamp must not be in the future");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("Timestamp must not be older than 1 year");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("Version must not be null or whitespace");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(value.Version, "^\\d+\\.\\d+\\.\\d+$"))
        {
            problems.Add("Version must be in format 'major.minor.patch' (e.g., '1.0.0')");
        }
        else if (value.Version.Length > 20)
        {
            problems.Add("Version must be 20 characters or less");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a HealthStatus object is valid
    /// </summary>
    /// <param name="value">The HealthStatus object to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this YouTubeShortsAutomator.Controllers.HealthStatus? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a HealthStatus object is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The HealthStatus object to validate</param>
    /// <exception cref="ArgumentException">Thrown when value is not valid</exception>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static void EnsureValid(this YouTubeShortsAutomator.Controllers.HealthStatus? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HealthStatus is not valid. Problems:\n{string.Join("\n", problems)}",
                nameof(value));
        }
    }
}