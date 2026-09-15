# AppSettingsValidation

The `AppSettingsValidation` static class provides extension methods for validating `AppSettings` instances. Callers can inspect all validation problems, perform a Boolean validity check, or enforce validity by throwing an exception.

## API

### `Validate(AppSettings settings)`

```csharp
public static IReadOnlyList<string> Validate(this AppSettings settings)
```

Validates the supplied settings and returns a read-only list containing one message for each failed rule. An empty list indicates that the settings are valid.

- **Parameters:** `settings` (`AppSettings`) — The configuration to validate.
- **Returns:** An `IReadOnlyList<string>` of validation problem messages.
- **Throws:** `ArgumentNullException` when `settings` is `null`.

### `IsValid(AppSettings settings)`

```csharp
public static bool IsValid(this AppSettings settings)
```

Determines whether the supplied settings pass every validation rule.

- **Parameters:** `settings` (`AppSettings`) — The configuration to check.
- **Returns:** `true` when `Validate` returns no problems; otherwise, `false`.
- **Throws:** `ArgumentNullException` when `settings` is `null`.

### `EnsureValid(AppSettings settings)`

```csharp
public static void EnsureValid(this AppSettings settings)
```

Validates the supplied settings and returns normally when they are valid. When validation fails, it throws an exception whose message contains every validation problem on a separate line.

- **Parameters:** `settings` (`AppSettings`) — The configuration to validate.
- **Returns:** None.
- **Throws:**
  - `ArgumentNullException` when `settings` is `null`.
  - `ArgumentException` when one or more validation rules fail.

## Validation rules

- `ConnectionString`, `DatabasePath`, `LogDirectory`, `ProcessingDirectory`, `OutputDirectory`, `YouTubeApiKey`, `YouTubeClientId`, and `YouTubeClientSecret` must not be `null`, empty, or whitespace.
- `WatermarkImagePath` must not be `null`, empty, or whitespace when `EnableWatermark` is `true`.
- `MaxConcurrentUploads`, `MaxConcurrentProcessing`, `UploadTimeoutSeconds`, `ProcessingQueueLimit`, `AnalyticsSyncIntervalHours`, and `ScheduleCheckIntervalSeconds` must be greater than zero.
- `DefaultRetryCount` must be zero or greater.

## Usage

```csharp
using YouTubeShortAutomator.Configuration;

var settings = new AppSettings
{
    ConnectionString = "Data Source=youtube-shorts.db",
    DatabasePath = "youtube-shorts.db",
    YouTubeApiKey = "api-key",
    YouTubeClientId = "client-id",
    YouTubeClientSecret = "client-secret"
};

var problems = settings.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine(problem);
    }
}

if (settings.IsValid())
{
    settings.EnsureValid();
}
```

## Notes

- The methods are extension methods in the `YouTubeShortAutomator.Configuration` namespace and can be called directly on an `AppSettings` instance.
- Validation reports all detected problems rather than stopping at the first failed rule.
- `Validate` returns a read-only collection and never returns `null`.
- `EnableAnalyticsSyncing` does not disable validation of `AnalyticsSyncIntervalHours`; the interval must always be positive.
- `WatermarkImagePath` is optional when `EnableWatermark` is `false`.
