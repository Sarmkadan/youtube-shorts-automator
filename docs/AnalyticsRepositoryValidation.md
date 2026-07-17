# AnalyticsRepositoryValidation

Static utility class that provides validation helpers for analytics‑related data used throughout the YouTube Shorts Automator project. The members expose a simple pattern: query methods return a list of validation error messages, a Boolean indicator reports overall validity, and an action method throws when the data is invalid.

## API

### Validate ()
- **Purpose:** Performs a general validation check and returns any error messages.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` containing zero or more error messages. An empty list indicates success.
- **Exceptions:** None.

### IsValid ()
- **Purpose:** Indicates whether the current validation state is successful.
- **Parameters:** None.
- **Return value:** `true` if no validation errors exist; otherwise `false`.
- **Exceptions:** None.

### EnsureValid ()
- **Purpose:** Asserts that the validation state is successful; throws if any errors are present.
- **Parameters:** None.
- **Return value:** None (void).
- **Exceptions:** Throws `InvalidOperationException` with a message composed of the validation errors when the validation fails.

### ValidateAnalyticsVideoId ()
- **Purpose:** Validates the analytics video identifier.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` of error messages related to the video ID validation; empty if the ID is valid.
- **Exceptions:** None.

### Validate ()
- **Purpose:** Additional general validation overload.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` of error messages; empty if validation passes.
- **Exceptions:** None.

### ValidateAnalyticsRecentMetrics ()
- **Purpose:** Validates recent metrics data.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` of error messages for the recent metrics; empty if valid.
- **Exceptions:** None.

### Validate ()
- **Purpose:** Another general validation overload.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` of error messages; empty if validation passes.
- **Exceptions:** None.

### Validate ()
- **Purpose:** Further general validation overload.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` of error messages; empty if validation passes.
- **Exceptions:** None.

### ValidateAnalyticsVideoViewCounts ()
- **Purpose:** Validates analytics video view counts.
- **Parameters:** None.
- **Return value:** An `IReadOnlyList<string>` of error messages for the view counts validation; empty if valid.
- **Exceptions:** None.

## Usage

```csharp
// Example 1: Checking validity before processing
if (!AnalyticsRepositoryValidation.IsValid)
{
    var errors = AnalyticsRepositoryValidation.Validate();
    Log.Warning("Validation failed: {Errors}", string.Join(", ", errors));
    return;
}

// Proceed with processing knowing the data is valid
ProcessAnalyticsData();
```

```csharp
// Example 2: Using specialized validators and enforcing constraints
var videoIdErrors = AnalyticsRepositoryValidation.ValidateAnalyticsVideoId();
var metricsErrors   = AnalyticsRepositoryValidation.ValidateAnalyticsRecentMetrics();
var viewCountErrors = AnalyticsRepositoryValidation.ValidateAnalyticsVideoViewCounts();

if (videoIdErrors.Any() || metricsErrors.Any() || viewCountErrors.Any())
{
    var allErrors = videoIdErrors
                    .Concat(metricsErrors)
                    .Concat(viewCountErrors)
                    .ToList();
    AnalyticsRepositoryValidation.EnsureValid(); // throws InvalidOperationException with combined messages
}
```

## Notes

- All validation methods are stateless and rely only on static data; therefore they are thread‑safe and can be called concurrently from multiple threads.
- The `Validate` overloads return an empty list to indicate success; they never return `null`.
- `EnsureValid` will throw an `InvalidOperationException` containing a concatenation of all error messages from the validation methods if any exist; callers should handle this exception when they need to abort processing on invalid data.
- Because the members accept no parameters, the validation logic operates on implicit context (e.g., fields or properties elsewhere in the class). Consumers should ensure that the relevant state is set before invoking these methods.
