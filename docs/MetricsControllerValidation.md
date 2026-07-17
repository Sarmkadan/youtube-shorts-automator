# MetricsControllerValidation

Provides static validation methods for metric-related parameters used in the YouTube Shorts Automator. The class centralizes validation logic for common metric inputs such as identifiers, counts, dates, and durations, returning error messages or throwing exceptions when inputs are invalid. Multiple overloads exist for each method to handle different data types.

## API

### Validate
Seven overloads, each accepting a different metric parameter. Returns a read-only list of validation error messages. An empty list indicates the input is valid.

- **Parameters** – Vary by overload. Common types include `string`, `int`, `DateTime`, `TimeSpan`, and `bool`.
- **Returns** – `IReadOnlyList<string>` – empty if valid; otherwise one or more error messages describing the violation.
- **Throws** – None. Validation failures are returned as messages, not exceptions.

### IsValid
Seven overloads, corresponding to the same parameter sets as `Validate`. Returns a boolean indicating whether the provided metric parameter passes validation.

- **Parameters** – Same as the corresponding `Validate` overload.
- **Returns** – `bool` – `true` if the parameter is valid; `false` otherwise.
- **Throws** – None.

### EnsureValid
Seven overloads, corresponding to the same parameter sets as `Validate`. Validates the parameter and throws an exception if validation fails.

- **Parameters** – Same as the corresponding `Validate` overload.
- **Returns** – `void`.
- **Throws** – `ArgumentException` (or a derived exception) if the parameter is invalid. The exception message includes the validation errors.

## Usage

### Example 1: Collecting validation errors with `Validate`

```csharp
using System.Collections.Generic;
using YouTubeShortsAutomator.Metrics;

public class MetricsService
{
    public bool TrySetVideoId(string videoId)
    {
        IReadOnlyList<string> errors = MetricsControllerValidation.Validate(videoId);
        if (errors.Count > 0)
        {
            foreach (string error in errors)
                Console.WriteLine($"Validation error: {error}");
            return false;
        }
        // Use valid videoId
        return true;
    }
}
```

### Example 2: Guarding input with `EnsureValid`

```csharp
using YouTubeShortsAutomator.Metrics;

public class MetricsController
{
    public void RecordViewCount(int viewCount)
    {
        MetricsControllerValidation.EnsureValid(viewCount);
        // viewCount is guaranteed valid after this call
        // Proceed with recording
    }
}
```

## Notes

- **Edge cases** – Each overload defines its own validation rules. For example, string overloads typically reject `null` or empty values; numeric overloads may reject negative numbers or values exceeding a maximum; date overloads may require a valid range or non-future dates. Always refer to the specific overload’s behavior.
- **Thread safety** – All methods are static and stateless. They do not modify any shared state, so they are safe to call concurrently from multiple threads.
- **Overload resolution** – When calling `Validate`, `IsValid`, or `EnsureValid`, the compiler selects the overload based on the argument types. Ensure the argument type matches one of the defined overloads to avoid ambiguity.
