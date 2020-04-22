# ConfigurationService

The `ConfigurationService` class serves as the central authority for managing application settings within the `youtube-shorts-automator` project. It is responsible for loading configuration data from underlying sources, providing strongly-typed access to settings, managing sensitive API credentials, and validating the integrity of the application's configuration state.

## API

### Constructors

*   **`ConfigurationService()`**
    Initializes a new instance of the `ConfigurationService` class, loading and caching initial configuration values from the default application configuration providers.

### Methods

*   **`T GetSetting<T>(string key)`**
    Retrieves a configuration setting of type `T` associated with the specified key. Throws an exception if the key is not found or if the value cannot be cast to the specified type.

*   **`YouTubeApiConfig GetYouTubeApiConfig()`**
    Retrieves the complete configuration object for the YouTube API.

*   **`string GetConnectionString()`**
    Returns the primary database connection string.

*   **`string? GetRedisConnectionString()`**
    Returns the configured Redis connection string, or `null` if Redis is not configured.

*   **`long GetMaxFileSize()`**
    Retrieves the maximum allowed file size for processing in bytes.

*   **`int GetProcessingTimeout()`**
    Returns the configured timeout for processing jobs, in seconds.

*   **`int GetUploadTimeout()`**
    Returns the configured timeout for YouTube upload operations, in seconds.

*   **`int GetMetricsRefreshInterval()`**
    Returns the interval for refreshing analytics metrics, in seconds.

*   **`string GetDefaultTimeZone()`**
    Returns the identifier for the default time zone used by the application.

*   **`bool IsFeatureEnabled(string featureName)`**
    Checks if a specific feature is enabled by name.

*   **`List<string> GetEnabledFeatures()`**
    Returns a list of all currently enabled feature flags.

*   **`(bool IsValid, List<string> Errors) ValidateConfiguration()`**
    Performs a validation check on the current configuration state. Returns a tuple indicating whether the configuration is valid and a list of error messages if validation fails.

*   **`void ClearCache()`**
    Clears any internally cached configuration values, forcing a reload on the next access.

### Properties

*   **`string ApiKey`**
    The API key for authorized services.
*   **`string ClientId`**
    The OAuth2 client identifier.
*   **`string ClientSecret`**
    The OAuth2 client secret.
*   **`string RedirectUri`**
    The configured OAuth2 redirect URI.

## Usage

### Accessing Typed Settings

```csharp
var config = new ConfigurationService();

// Retrieve a typed integer setting
int timeout = config.GetProcessingTimeout();

// Retrieve a generic setting
var customSetting = config.GetSetting<bool>("CustomFeatureFlag");
```

### Validating Configuration on Startup

```csharp
var config = new ConfigurationService();
var validationResult = config.ValidateConfiguration();

if (!validationResult.IsValid)
{
    throw new InvalidOperationException($"Configuration invalid: {string.Join(", ", validationResult.Errors)}");
}
```

## Notes

*   **Thread Safety**: The `ConfigurationService` is designed to be thread-safe for reading operations. However, calling `ClearCache()` while other threads are accessing configuration values may result in unexpected behavior or performance fluctuations as the cache is re-populated.
*   **Performance**: Repeated calls to `GetSetting<T>` or other getters typically rely on internal caching. If configuration values are expected to change dynamically without restarting the application, `ClearCache()` must be invoked to ensure subsequent calls reflect the updated values.
*   **Missing Values**: Methods returning `string` or primitive types (`long`, `int`) will throw exceptions if the requested configuration key is missing. The `GetRedisConnectionString()` method is an exception, as it is designed to return `null` if the Redis configuration is optional.
