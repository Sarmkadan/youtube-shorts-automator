// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Configuration;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Exceptions;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Extension methods for ConfigurationService providing additional functionality
/// </summary>
public static class ConfigurationServiceExtensions
{
    /// <summary>
    /// Gets a configuration value with validation that the value is not null or whitespace
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Configured value or default</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="DomainException">Thrown when the configured value is null or whitespace and no default provided</exception>
    public static string GetRequiredSetting(
        this ConfigurationService service,
        string key,
        string? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var value = service.GetSetting(key, defaultValue);

        if (string.IsNullOrWhiteSpace(value as string))
        {
            if (defaultValue is null)
                throw new DomainException($"Configuration key '{key}' must not be null or whitespace");

            return defaultValue;
        }

        return value;
    }

    /// <summary>
    /// Gets a configuration value with validation that the value is a positive number
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Configured value or default</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="DomainException">Thrown when the configured value is not a positive number</exception>
    public static int GetPositiveSetting(
        this ConfigurationService service,
        string key,
        int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var value = service.GetSetting(key, defaultValue);

        if (value <= 0)
        {
            return defaultValue;
        }

        return value;
    }

    /// <summary>
    /// Gets a configuration value with validation that the value is a positive long
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Configured value or default</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="DomainException">Thrown when the configured value is not a positive number</exception>
    public static long GetPositiveLongSetting(
        this ConfigurationService service,
        string key,
        long defaultValue = 0L)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var value = service.GetSetting(key, defaultValue);

        if (value <= 0L)
        {
            return defaultValue;
        }

        return value;
    }

    /// <summary>
    /// Checks if any of the specified features are enabled
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <param name="featureNames">Feature names to check</param>
    /// <returns>True if any feature is enabled, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null or featureNames is null</exception>
    public static bool AnyFeatureEnabled(
        this ConfigurationService service,
        params string[] featureNames)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(featureNames);

        if (featureNames.Length == 0)
        {
            return false;
        }

        foreach (var featureName in featureNames)
        {
            if (service.IsFeatureEnabled(featureName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a list of enabled features as a read-only collection
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <returns>Read-only list of enabled feature names</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    public static IReadOnlyList<string> GetEnabledFeaturesReadOnly(
        this ConfigurationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetEnabledFeatures().AsReadOnly();
    }

    /// <summary>
    /// Gets YouTube API configuration with validation that required fields are present
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <returns>Validated YouTube API configuration</returns>
    /// <exception cref="DomainException">Thrown when required YouTube API fields are missing</exception>
    public static YouTubeApiConfig GetValidatedYouTubeApiConfig(
        this ConfigurationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var config = service.GetYouTubeApiConfig();

        if (string.IsNullOrWhiteSpace(config.ClientId) || string.IsNullOrWhiteSpace(config.ClientSecret))
        {
            throw new DomainException(
                "YouTube API configuration is invalid. Both ClientId and ClientSecret must be configured.");
        }

        return config;
    }

    /// <summary>
    /// Gets database connection string with validation that it's not empty
    /// </summary>
    /// <param name="service">The configuration service</param>
    /// <param name="connectionName">Connection name (optional)</param>
    /// <returns>Connection string</returns>
    /// <exception cref="DomainException">Thrown when connection string is null or whitespace</exception>
    public static string GetValidatedConnectionString(
        this ConfigurationService service,
        string connectionName = "DefaultConnection")
    {
        ArgumentNullException.ThrowIfNull(service);

        var connectionString = service.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new DomainException($"Connection string '{connectionName}' is null or whitespace");
        }

        return connectionString;
    }
}