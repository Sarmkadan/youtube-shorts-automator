// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;
using YouTubeShortsAutomator.Domain.Exceptions;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Manages application configuration and settings
/// </summary>
public class ConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly Dictionary<string, object> _cachedSettings;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _cachedSettings = new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets a configuration value
    /// </summary>
    public T GetSetting<T>(string key, T defaultValue)
    {
        try
        {
            if (_cachedSettings.TryGetValue(key, out var cachedValue))
                return (T)cachedValue;

            var value = _configuration.GetValue<T>(key, defaultValue);
            _cachedSettings[key] = value ?? defaultValue!;

            _logger.LogDebug($"Retrieved configuration setting: {key}");
            return value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error retrieving configuration setting {key}, using default");
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets YouTube API configuration
    /// </summary>
    public YouTubeApiConfig GetYouTubeApiConfig()
    {
        _logger.LogInformation("Loading YouTube API configuration");

        var config = new YouTubeApiConfig
        {
            ApiKey = GetSetting("YouTubeApi:ApiKey", ""),
            ClientId = GetSetting("YouTubeApi:ClientId", ""),
            ClientSecret = GetSetting("YouTubeApi:ClientSecret", ""),
            RedirectUri = GetSetting("YouTubeApi:RedirectUri", "http://localhost:5000/auth/callback")
        };

        if (string.IsNullOrEmpty(config.ApiKey) && string.IsNullOrEmpty(config.ClientId))
            _logger.LogWarning("YouTube API credentials are not configured");

        return config;
    }

    /// <summary>
    /// Gets database connection string
    /// </summary>
    public string GetConnectionString(string connectionName = "DefaultConnection")
    {
        _logger.LogDebug($"Retrieving connection string: {connectionName}");
        var connectionString = _configuration.GetConnectionString(connectionName);

        if (string.IsNullOrEmpty(connectionString))
            throw new DomainException($"Connection string '{connectionName}' not found in configuration");

        return connectionString;
    }

    /// <summary>
    /// Gets Redis connection string
    /// </summary>
    public string? GetRedisConnectionString()
    {
        return _configuration.GetConnectionString("Redis");
    }

    /// <summary>
    /// Gets max file size from configuration
    /// </summary>
    public long GetMaxFileSize()
    {
        return GetSetting("ApplicationSettings:MaxFileSize", ApplicationConstants.Video.MaxFileSizeBytes);
    }

    /// <summary>
    /// Gets processing timeout in seconds
    /// </summary>
    public int GetProcessingTimeout()
    {
        return GetSetting("ApplicationSettings:ProcessingTimeoutSeconds", ApplicationConstants.Processing.ProcessingTimeoutSeconds);
    }

    /// <summary>
    /// Gets upload timeout in seconds
    /// </summary>
    public int GetUploadTimeout()
    {
        return GetSetting("ApplicationSettings:UploadTimeoutSeconds", ApplicationConstants.Upload.UploadTimeoutSeconds);
    }

    /// <summary>
    /// Gets metrics refresh interval in minutes
    /// </summary>
    public int GetMetricsRefreshInterval()
    {
        return GetSetting("ApplicationSettings:MetricsRefreshIntervalMinutes", ApplicationConstants.Analytics.MetricsRefreshIntervalMinutes);
    }

    /// <summary>
    /// Gets default time zone
    /// </summary>
    public string GetDefaultTimeZone()
    {
        return GetSetting("ApplicationSettings:DefaultTimeZone", ApplicationConstants.TimeZones.DefaultTimeZone);
    }

    /// <summary>
    /// Checks if a feature flag is enabled
    /// </summary>
    public bool IsFeatureEnabled(string featureName)
    {
        _logger.LogDebug($"Checking feature flag: {featureName}");
        return GetSetting($"Features:{featureName}", false);
    }

    /// <summary>
    /// Gets all enabled features
    /// </summary>
    public List<string> GetEnabledFeatures()
    {
        _logger.LogInformation("Retrieving enabled features");

        var featuresSection = _configuration.GetSection("Features");
        var enabledFeatures = new List<string>();

        foreach (var feature in featuresSection.GetChildren())
        {
            if (feature.Value == "true")
                enabledFeatures.Add(feature.Key);
        }

        _logger.LogInformation($"Enabled features: {string.Join(", ", enabledFeatures)}");
        return enabledFeatures;
    }

    /// <summary>
    /// Validates required configuration
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateConfiguration()
    {
        _logger.LogInformation("Validating application configuration");
        var errors = new List<string>();

        try
        {
            var connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                errors.Add("Database connection string is not configured");
        }
        catch
        {
            errors.Add("Database connection string is not configured");
        }

        var youtubeConfig = GetYouTubeApiConfig();
        if (string.IsNullOrEmpty(youtubeConfig.ClientId) || string.IsNullOrEmpty(youtubeConfig.ClientSecret))
            errors.Add("YouTube API credentials are not configured");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Clears the configuration cache
    /// </summary>
    public void ClearCache()
    {
        _logger.LogInformation("Clearing configuration cache");
        _cachedSettings.Clear();
    }
}

/// <summary>
/// YouTube API configuration model
/// </summary>
public class YouTubeApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
