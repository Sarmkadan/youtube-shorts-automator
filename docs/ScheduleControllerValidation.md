# ScheduleControllerValidation

Provides static methods for validating schedule controller configurations used in YouTube Shorts automation workflows. These utilities centralize validation logic to ensure schedule controllers are properly configured before being used in scheduling operations.

## API

### `Validate`
```csharp
public static IReadOnlyList<string> Validate(ScheduleControllerConfiguration config)
```
Validates the provided schedule controller configuration and returns a list of validation error messages. An empty list indicates the configuration is valid.

- **Parameters**
  - `config` – The schedule controller configuration to validate.
- **Return value**
  - A read-only list of error messages. If empty, the configuration is valid.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.

### `IsValid`
```csharp
public static bool IsValid(ScheduleControllerConfiguration config)
```
Determines whether the provided schedule controller configuration is valid.

- **Parameters**
  - `config` – The schedule controller configuration to check.
- **Return value**
  - `true` if the configuration is valid; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.

### `EnsureValid`
```csharp
public static void EnsureValid(ScheduleControllerConfiguration config)
```
Validates the provided schedule controller configuration and throws an exception if invalid.

- **Parameters**
  - `config` – The schedule controller configuration to validate.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.
  - Throws `InvalidOperationException` with a descriptive message if the configuration is invalid.

## Usage

### Basic validation with error collection
```csharp
var config = new ScheduleControllerConfiguration
{
    PollingInterval = TimeSpan.FromSeconds(-1), // Invalid
    MaxConcurrentUploads = 0
};

var errors = ScheduleControllerValidation.Validate(config);
if (errors.Any())
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

### Throwing on invalid configuration
```csharp
try
{
    var config = new ScheduleControllerConfiguration
    {
        PollingInterval = TimeSpan.Zero,
        MaxConcurrentUploads = -5
    };

    ScheduleControllerValidation.EnsureValid(config);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid configuration: {ex.Message}");
}
```

## Notes

- All methods are thread-safe and do not modify the input configuration.
- Validation rules include non-null checks, non-negative durations, and positive integer values for concurrency limits.
- The validation logic is intentionally strict to prevent runtime failures during scheduling operations.
- No caching or state is maintained between calls; each validation is performed independently.
