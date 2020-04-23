# ValidationUtility

The `ValidationUtility` static class provides a suite of robust, centralized validation methods designed to ensure data integrity within the `youtube-shorts-automator` project. It encapsulates common validation logic for YouTube-related entities, user inputs, and configuration parameters, promoting consistency and reducing code duplication throughout the application's domain services and data layers.

## API

All methods are `public static`. Methods returning a tuple `(bool IsValid, string? Error)` provide a boolean flag indicating success and an optional string containing a descriptive error message if validation fails.

### Validation Methods (Tuple Return)

*   **`ValidateEmail(string email)`**
    *   **Purpose:** Validates the format of an email address string.
    *   **Returns:** `(bool IsValid, string? Error)` indicating if the format conforms to standard email requirements.
*   **`ValidateUrl(string url)`**
    *   **Purpose:** Validates the structure of a general URL.
    *   **Returns:** `(bool IsValid, string? Error)` indicating if the URL is well-formed.
*   **`ValidateYouTubeChannelId(string channelId)`**
    *   **Purpose:** Validates the format of a YouTube Channel ID.
    *   **Returns:** `(bool IsValid, string? Error)` indicating if the ID conforms to YouTube's specifications.
*   **`ValidateYouTubeVideoId(string videoId)`**
    *   **Purpose:** Validates the format of a YouTube Video ID.
    *   **Returns:** `(bool IsValid, string? Error)` indicating if the ID conforms to YouTube's specifications.
*   **`ValidateVideoTitle(string title)`**
    *   **Purpose:** Validates a video title against required length and character constraints.
    *   **Returns:** `(bool IsValid, string? Error)` detailing any constraint violations.
*   **`ValidateVideoDescription(string description)`**
    *   **Purpose:** Validates a video description against allowed length and content constraints.
    *   **Returns:** `(bool IsValid, string? Error)` detailing any constraint violations.
*   **`ValidateVideoTags(string tags)`**
    *   **Purpose:** Validates the formatting and constraints of video tag strings.
    *   **Returns:** `(bool IsValid, string? Error)` indicating if the tags are properly formatted.
*   **`ValidateVideoFile(string filePath)`**
    *   **Purpose:** Validates the existence and accessibility of a video file, and optionally verifies the file format.
    *   **Returns:** `(bool IsValid, string? Error)` indicating success or the nature of the file error.
*   **`ValidateScheduleTime(string time)`**
    *   **Purpose:** Validates that a string representing a scheduled time conforms to the required format.
    *   **Returns:** `(bool IsValid, string? Error)` indicating if the scheduled time is valid.

### Check Methods (Boolean Return)

*   **`IsValidTimeSpan(string timeSpan)`**
    *   **Purpose:** Determines if a string can be successfully parsed into a `TimeSpan`.
    *   **Returns:** `true` if valid, `false` otherwise.
*   **`IsValidJsonString(string json)`**
    *   **Purpose:** Determines if a string is a valid JSON representation.
    *   **Returns:** `true` if valid, `false` otherwise.

## Usage

### Example 1: Validating User Input for Video Upload

```csharp
string videoId = "dQw4w9WgXcQ";
string title = "My Awesome Short";

var idResult = ValidationUtility.ValidateYouTubeVideoId(videoId);
var titleResult = ValidationUtility.ValidateVideoTitle(title);

if (idResult.IsValid && titleResult.IsValid)
{
    // Proceed with upload
}
else
{
    Console.WriteLine($"Validation failed: {idResult.Error ?? titleResult.Error}");
}
```

### Example 2: Configuring Scheduler

```csharp
string rawTime = "14:30:00";
string rawDuration = "00:01:00";

if (ValidationUtility.ValidateScheduleTime(rawTime).IsValid && 
    ValidationUtility.IsValidTimeSpan(rawDuration))
{
    // Schedule the task
}
```

## Notes

*   **Thread Safety:** The methods in `ValidationUtility` are `static` and stateless. They are inherently thread-safe and can be invoked concurrently from multiple threads without locking.
*   **Input Handling:** Unless otherwise specified for a specific method, `null` or `string.Empty` inputs are generally treated as invalid and will result in `IsValid` being `false` with an appropriate error message.
*   **Exceptions:** These validation methods are designed to encapsulate logic without throwing exceptions for common validation failures. Exceptional cases (such as underlying IO failures in `ValidateVideoFile`) are handled internally to return a failed validation state.
