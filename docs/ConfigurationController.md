# ConfigurationController

Provides endpoints and properties for retrieving and inspecting the application's runtime configuration, storage metrics, and feature flags used by the YouTube Shorts Automator service.

## API

### `public ConfigurationController`

Controller class exposing endpoints for configuration retrieval. Inherits from `ControllerBase`.

### `public IActionResult GetConfigurationInfo()`

Returns a comprehensive configuration summary including version, environment, features, and storage metrics.

- **Returns**: `IActionResult` with JSON-serialized configuration object.
- **Throws**: May throw if configuration cannot be read or serialized.

### `public IActionResult GetStorageConfiguration()`

Returns storage-related settings including directory paths, capacity limits, and current usage.

- **Returns**: `IActionResult` with JSON-serialized storage configuration.
- **Throws**: May throw if storage metrics cannot be retrieved.

### `public IActionResult GetProcessingSettings()`

Returns processing-related settings including parallel job limits, timeouts, retention policies, and supported formats.

- **Returns**: `IActionResult` with JSON-serialized processing settings.
- **Throws**: May throw if processing settings are unavailable.

### `public IActionResult GetYouTubeIntegrationStatus()`

Returns the current YouTube API integration status and supported features.

- **Returns**: `IActionResult` with JSON-serialized integration status.
- **Throws**: May throw if YouTube service status cannot be determined.

### `public IActionResult GetHealthStatus()`

Returns a health status summary including storage usage, configuration validity, and system readiness.

- **Returns**: `IActionResult` with JSON-serialized health status.
- **Throws**: May throw if health checks fail.

### `public IActionResult GetSupportedFormats()`

Returns the list of supported video formats for processing and upload.

- **Returns**: `IActionResult` with JSON-serialized array of supported formats.
- **Throws**: May throw if format list cannot be retrieved.

### `public string Version`

Gets the application version string (e.g., "1.2.3").

### `public string Environment`

Gets the current runtime environment (e.g., "Development", "Production").

### `public object? Features`

Gets a dynamic object representing enabled feature flags and their states.

### `public int MaxUploadSizeMb`

Gets the maximum allowed upload size in megabytes.

### `public string[]? SupportedVideoFormats`

Gets the array of supported video file extensions (e.g., `["mp4", "mov", "avi"]`).

### `public string[]? ProcessingProfiles`

Gets the array of available processing profiles (e.g., `["HighQuality", "MobileOptimized"]`).

### `public string TempDirectoryPath`

Gets the absolute path to the temporary file storage directory.

### `public string VideoDirectoryPath`

Gets the absolute path to the directory where videos are stored.

### `public int MaxStorageGb`

Gets the maximum allowed storage capacity in gigabytes.

### `public int CurrentUsageGb`

Gets the current storage usage in gigabytes.

### `public int RetentionDays`

Gets the number of days to retain processed videos before cleanup.

### `public int ParallelJobLimit`

Gets the maximum number of parallel processing jobs allowed.

### `public int TimeoutMinutes`

Gets the default timeout in minutes for processing operations.

## Usage

```csharp
// Example 1: Fetch and log configuration info
var configController = new ConfigurationController();
var configResult = configController.GetConfigurationInfo();
if (configResult is OkObjectResult okResult)
{
    var config = okResult.Value;
    Console.WriteLine($"App Version: {config.Version}");
    Console.WriteLine($"Environment: {config.Environment}");
}

// Example 2: Check storage health before processing
var healthResult = configController.GetHealthStatus();
if (healthResult is OkObjectResult healthOk && healthOk.Value is dynamic health)
{
    var usagePercent = (health.CurrentUsageGb * 100) / health.MaxStorageGb;
    if (usagePercent > 80)
    {
        Console.WriteLine("Warning: Storage usage exceeds 80%");
    }
}
```

## Notes

- All property getters are thread-safe and return immutable or snapshot values.
- Endpoint methods may throw transient exceptions under high load; consider wrapping calls in retry logic.
- Storage metrics (`CurrentUsageGb`, `MaxStorageGb`) are approximate and updated periodically; real-time accuracy is not guaranteed.
- Feature flags in `Features` may change between requests; do not cache this object for long periods.
- `SupportedVideoFormats` and `ProcessingProfiles` are read-only and reflect server capabilities; client-side validation should not assume persistence.
