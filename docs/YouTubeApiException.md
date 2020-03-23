# YouTubeApiException

Represents an exception that occurs when interacting with the YouTube Data API. It enriches the standard `Exception` with contextual information such as the affected channel, the API error code, the HTTP status code, and convenience flags for common failure modes like token expiration or quota exhaustion. This type is used throughout the `youtube-shorts-automator` project to propagate API errors in a structured way.

## API

### Properties

- **`public int? ChannelId`**  
  Gets the YouTube channel ID associated with the failed API request, if available. May be `null` when the error is not channel‑specific.

- **`public string? ApiErrorCode`**  
  Gets the error code returned by the YouTube API (e.g., `"quotaExceeded"`, `"accessNotConfigured"`). Returns `null` if no API error code was provided.

- **`public int? HttpStatusCode`**  
  Gets the HTTP status code of the API response (e.g., 403, 429). Returns `null` if the response was not received or the status code is unknown.

- **`public bool IsTokenExpired`**  
  Gets a value indicating whether the error is caused by an expired or invalid OAuth 2.0 token. This is typically derived from the API error code or HTTP status.

- **`public bool IsQuotaExceeded`**  
  Gets a value indicating whether the error is caused by exceeding the YouTube Data API quota for the current project.

### Constructors

- **`public YouTubeApiException()`**  
  Initializes a new instance of the `YouTubeApiException` class with no error message.

- **`public YouTubeApiException(string message)`**  
  Initializes a new instance with a specified error message.  
  *Parameters:*  
  - `message` – A human‑readable description of the error.

- **`public YouTubeApiException(string message, Exception innerException)`**  
  Initializes a new instance with a specified error message and a reference to the inner exception that is the cause of this exception.  
  *Parameters:*  
  - `message` – A human‑readable description of the error.  
  - `innerException` – The exception that is the cause of the current exception.

## Usage

### Example 1: Handling token expiration

```csharp
try
{
    var video = await youtubeService.UploadAsync(filePath, metadata);
}
catch (YouTubeApiException ex) when (ex.IsTokenExpired)
{
    Console.WriteLine("Token expired. Refreshing...");
    await authProvider.RefreshTokenAsync();
    // Retry the upload
}
```

### Example 2: Logging detailed error information

```csharp
try
{
    var shorts = await youtubeService.GetShortsAsync(channelId);
}
catch (YouTubeApiException ex)
{
    logger.LogError(
        "YouTube API error for channel {ChannelId}: HTTP {StatusCode}, API code {ApiCode}",
        ex.ChannelId,
        ex.HttpStatusCode,
        ex.ApiErrorCode);

    if (ex.IsQuotaExceeded)
    {
        // Implement backoff or notify the user
        throw new RetryableException("Quota exceeded, retry later.", ex);
    }
}
```

## Notes

- All properties (`ChannelId`, `ApiErrorCode`, `HttpStatusCode`, `IsTokenExpired`, `IsQuotaExceeded`) are read‑only after construction. The exception is immutable and safe to read from multiple threads concurrently.
- `ChannelId`, `ApiErrorCode`, and `HttpStatusCode` may be `null` if the corresponding information was not present in the API response. Always check for `null` before using these values.
- `IsTokenExpired` and `IsQuotaExceeded` are computed based on the error data provided at construction time. They are not dynamically updated.
- The exception inherits from `System.Exception`; all standard members (e.g., `Message`, `StackTrace`, `InnerException`) are available.
