# DomainException

The `DomainException` serves as the base class for all custom domain-specific exceptions within the `youtube-shorts-automator` project. It provides a structured way to report errors occurring within the business logic layer, allowing for consistent error handling and diagnostic metadata across different operational contexts, such as API interactions, video processing, and credential management.

## API

### `DomainException`
Constructor for the base exception. Used when throwing generic domain errors.

### `DomainException`
Parameterized constructor to initialize a `DomainException`.

### `void AddContext(string key, object value)`
Adds additional metadata to the `Context` dictionary.
*   **Parameters**: `key` (string) - The name of the context item. `value` (object) - The data to store.

### `string? ErrorCode`
A nullable string representing a machine-readable error code identifying the specific type of failure.

### `Dictionary<string, object>? Context`
A collection of key-value pairs providing additional diagnostic information related to the exception.

### `List<string> ValidationErrors`
A list of strings containing specific validation error messages, primarily used when model or input validation fails.

---

### `VideoValidationException`
Inherits from `DomainException`. Thrown when video content fails validation rules prior to processing or upload.

### `ProcessingJobException`
Inherits from `DomainException`. Thrown when an error occurs during the execution of a background processing job.
*   **`Guid JobId`**: The unique identifier of the job that failed.

### `UploadException`
Inherits from `DomainException`. Thrown when an error occurs during the upload of media to YouTube.
*   **`string? YouTubeErrorCode`**: The specific error code returned by the YouTube API, if available.

### `CredentialException(string message) : base(message, "CREDENTIAL_INVALID")`
Inherits from `DomainException`. Thrown when authentication or authorization credentials are invalid.
*   **`Guid CredentialId`**: The unique identifier of the credential that caused the failure.

### `OAuthTokenExpiredException`
Inherits from `DomainException`. Thrown when an OAuth2 token has expired and requires refreshing.

### `ApiException`
Inherits from `DomainException`. Thrown when an interaction with an external HTTP-based API fails.
*   **`int? HttpStatusCode`**: The HTTP status code returned by the API.
*   **`string? ApiResponse`**: The raw response body from the API, if available.

### `QuotaExceededException`
Inherits from `DomainException`. Thrown when an API usage quota has been reached.
*   **`long CurrentUsageBytes`**: The amount of usage accumulated before the quota was reached.
*   **`long QuotaBytes`**: The maximum allowed quota limit.

## Usage

### Basic Domain Exception Handling
```csharp
try 
{
    // Business logic
    throw new DomainException("Operation failed due to invalid state.");
}
catch (DomainException ex)
{
    ex.AddContext("UserId", "12345");
    logger.LogError(ex, "Domain error: {Code}", ex.ErrorCode);
}
```

### Handling Specialized API Exceptions
```csharp
try 
{
    await _apiClient.UploadVideoAsync(videoData);
}
catch (QuotaExceededException ex)
{
    logger.LogWarning("Quota exceeded. Used: {Used}, Limit: {Limit}", 
        ex.CurrentUsageBytes, ex.QuotaBytes);
}
catch (ApiException ex)
{
    logger.LogError("API Error {Status}: {Response}", 
        ex.HttpStatusCode, ex.ApiResponse);
}
```

## Notes

*   **Thread Safety**: These exception classes are inherently thread-safe as they primarily act as data containers for error state. They should be treated as immutable once thrown.
*   **Serialization**: When using these exceptions across service boundaries or serialization layers, ensure the `Context` dictionary can be properly serialized by the configured serializer (e.g., `System.Text.Json`).
*   **ErrorCode**: Always populate `ErrorCode` where possible to allow consuming services to implement logic based on error types without parsing string messages.
