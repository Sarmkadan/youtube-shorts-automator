# AnalyticsControllerValidation

Provides validation utilities for analytics-related controller inputs and configuration, ensuring required fields and constraints are met before processing requests.

## API

### `Validate(AnalyticsConfig config)`
Validates an `AnalyticsConfig` instance.
- **Parameters**: `config` – The analytics configuration to validate.
- **Returns**: A list of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `config` is `null`.

### `Validate(AnalyticsRequest request)`
Validates an `AnalyticsRequest` instance.
- **Parameters**: `request` – The analytics request to validate.
- **Returns**: A list of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `request` is `null`.

### `Validate(string videoId, string? channelId, DateTime? startDate, DateTime? endDate)`
Validates analytics query parameters.
- **Parameters**:
  - `videoId` – The video identifier (required).
  - `channelId` – Optional channel identifier.
  - `startDate` – Optional start date for analytics range.
  - `endDate` – Optional end date for analytics range.
- **Returns**: A list of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `videoId` is `null`.

### `Validate(VideoAnalyticsSettings settings)`
Validates video analytics settings.
- **Parameters**: `settings` – The video analytics settings to validate.
- **Returns**: A list of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `settings` is `null`.

### `Validate(ChannelAnalyticsSettings settings)`
Validates channel analytics settings.
- **Parameters**: `settings` – The channel analytics settings to validate.
- **Returns**: A list of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `settings` is `null`.

### `IsValid(AnalyticsConfig config)`
Determines whether an `AnalyticsConfig` instance is valid.
- **Parameters**: `config` – The analytics configuration to check.
- **Returns**: `true` if valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `config` is `null`.

### `IsValid(AnalyticsRequest request)`
Determines whether an `AnalyticsRequest` instance is valid.
- **Parameters**: `request` – The analytics request to check.
- **Returns**: `true` if valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `request` is `null`.

### `IsValid(string videoId, string? channelId, DateTime? startDate, DateTime? endDate)`
Determines whether analytics query parameters are valid.
- **Parameters**:
  - `videoId` – The video identifier (required).
  - `channelId` – Optional channel identifier.
  - `startDate` – Optional start date for analytics range.
  - `endDate` – Optional end date for analytics range.
- **Returns**: `true` if valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `videoId` is `null`.

### `IsValid(VideoAnalyticsSettings settings)`
Determines whether video analytics settings are valid.
- **Parameters**: `settings` – The video analytics settings to check.
- **Returns**: `true` if valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `settings` is `null`.

### `IsValid(ChannelAnalyticsSettings settings)`
Determines whether channel analytics settings are valid.
- **Parameters**: `settings` – The channel analytics settings to check.
- **Returns**: `true` if valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `settings` is `null`.

### `EnsureValid(AnalyticsConfig config)`
Ensures an `AnalyticsConfig` instance is valid, throwing if invalid.
- **Parameters**: `config` – The analytics configuration to validate.
- **Throws**: `ArgumentNullException` if `config` is `null`; `InvalidOperationException` if validation fails.

### `EnsureValid(AnalyticsRequest request)`
Ensures an `AnalyticsRequest` instance is valid, throwing if invalid.
- **Parameters**: `request` – The analytics request to validate.
- **Throws**: `ArgumentNullException` if `request` is `null`; `InvalidOperationException` if validation fails.

### `EnsureValid(string videoId, string? channelId, DateTime? startDate, DateTime? endDate)`
Ensures analytics query parameters are valid, throwing if invalid.
- **Parameters**:
  - `videoId` – The video identifier (required).
  - `channelId` – Optional channel identifier.
  - `startDate` – Optional start date for analytics range.
  - `endDate` – Optional end date for analytics range.
- **Throws**: `ArgumentNullException` if `videoId` is `null`; `InvalidOperationException` if validation fails.

### `EnsureValid(VideoAnalyticsSettings settings)`
Ensures video analytics settings are valid, throwing if invalid.
- **Parameters**: `settings` – The video analytics settings to validate.
- **Throws**: `ArgumentNullException` if `settings` is `null`; `InvalidOperationException` if validation fails.

### `EnsureValid(ChannelAnalyticsSettings settings)`
Ensures channel analytics settings are valid, throwing if invalid.
- **Parameters**: `settings` – The channel analytics settings to validate.
- **Throws**: `ArgumentNullException` if `settings` is `null`; `InvalidOperationException` if validation fails.

## Usage

```csharp
// Example 1: Validating analytics request parameters
var errors = AnalyticsControllerValidation.Validate("abc123", "channel456", DateTime.Now.AddDays(-7), DateTime.Now);
if (errors.Any())
{
    Console.WriteLine("Validation failed: " + string.Join(", ", errors));
}

// Example 2: Ensuring valid analytics configuration
var config = new AnalyticsConfig { ApiKey = "key", MaxRetries = 3 };
AnalyticsControllerValidation.EnsureValid(config);
```

## Notes

- All methods are thread-safe and do not mutate input parameters.
- Validation rules are consistent across overloads; `IsValid` and `EnsureValid` share the same underlying logic.
- `EnsureValid` throws `InvalidOperationException` with a descriptive message on failure; the message includes the first encountered error.
- Date ranges are validated such that `startDate` must not be after `endDate` if both are provided.
