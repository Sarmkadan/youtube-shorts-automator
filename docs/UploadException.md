# UploadException

Represents an error that occurs during the upload process of a video short. This exception carries contextual identifiers for the upload job and the video short, along with a flag indicating whether the operation can be retried. It is designed to be thrown by upload handlers and caught by orchestration logic that decides on retry or failure escalation.

## API

### Constructors

#### `UploadException(string message)`
Initializes a new instance of the `UploadException` class with a specified error message. The `UploadJobId`, `VideoShortId`, and `IsRetryable` properties remain `null` or `false` unless explicitly set after construction.

- **Parameters**:
  - `message` (`string`): The error message that describes the reason for the exception.
- **Return value**: A new `UploadException` instance.
- **Throws**: Nothing directly. The base constructor may throw `ArgumentNullException` if `message` is `null` in certain .NET runtimes, but this is not enforced by the signature itself.

#### `UploadException()`
Parameterless constructor. Creates an instance with a default system-supplied error message. All contextual properties (`UploadJobId`, `VideoShortId`, `IsRetryable`) remain unset.

- **Return value**: A new `UploadException` instance.
- **Throws**: Nothing.

#### `UploadException(string message, Exception innerException)`
Initializes a new instance with a specified error message and a reference to the inner exception that is the cause of this exception.

- **Parameters**:
  - `message` (`string`): The error message.
  - `innerException` (`Exception`): The exception that is the cause of the current exception.
- **Return value**: A new `UploadException` instance.
- **Throws**: Nothing directly.

### Properties

#### `int? UploadJobId`
Gets or sets the identifier of the upload job associated with this exception. A `null` value indicates that the job identifier was not available at the point of failure.

- **Type**: Nullable `int` (`int?`).
- **Default**: `null`.
- **Throws**: No exceptions on get or set.

#### `int? VideoShortId`
Gets or sets the identifier of the video short entity involved in the failed upload. A `null` value means the short identifier could not be determined or was not applicable.

- **Type**: Nullable `int` (`int?`).
- **Default**: `null`.
- **Throws**: No exceptions on get or set.

#### `bool IsRetryable`
Gets or sets a value indicating whether the operation that caused this exception can be safely retried. Consumers should inspect this flag before scheduling a retry attempt.

- **Type**: `bool`.
- **Default**: `false`.
- **Throws**: No exceptions on get or set.

## Usage

### Example 1: Throwing and catching with retry logic

```csharp
public async Task UploadVideoShortAsync(int jobId, int shortId, Stream videoStream)
{
    try
    {
        await _uploadService.SendAsync(videoStream);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
        var uploadEx = new UploadException("Upload service temporarily unavailable", ex)
        {
            UploadJobId = jobId,
            VideoShortId = shortId,
            IsRetryable = true
        };
        throw uploadEx;
    }
    catch (Exception ex)
    {
        var uploadEx = new UploadException("Upload failed with non-retryable error", ex)
        {
            UploadJobId = jobId,
            VideoShortId = shortId,
            IsRetryable = false
        };
        throw uploadEx;
    }
}

// Orchestration layer
try
{
    await UploadVideoShortAsync(42, 101, fileStream);
}
catch (UploadException uex) when (uex.IsRetryable)
{
    _logger.Warning($"Retrying upload for job {uex.UploadJobId}, short {uex.VideoShortId}");
    await _retryQueue.EnqueueAsync(uex.UploadJobId.Value, uex.VideoShortId.Value);
}
catch (UploadException uex)
{
    _logger.Error($"Permanent upload failure for job {uex.UploadJobId}, short {uex.VideoShortId}");
    await _notificationService.NotifyFailureAsync(uex);
}
```

### Example 2: Minimal construction for logging context

```csharp
public void LogUploadFailure(Exception rawException)
{
    var uploadEx = new UploadException("An upload error occurred", rawException);
    // Identifiers are unknown at this point, so they remain null.

    _logger.Error(uploadEx, "Upload failure logged without job context. Retryable: {IsRetryable}", uploadEx.IsRetryable);

    if (uploadEx.InnerException is TimeoutException)
    {
        // Even without IDs, we can infer retryability from the inner exception type.
        uploadEx.IsRetryable = true;
    }
}
```

## Notes

- **Nullable identifiers**: Both `UploadJobId` and `VideoShortId` are nullable. Code that consumes these properties must guard against `null` values before dereferencing, especially when enqueuing retries or correlating with database records.
- **Default retryability**: `IsRetryable` defaults to `false`. Callers that construct the exception must explicitly set this property to `true` for transient failures; otherwise, orchestration logic will treat the error as terminal.
- **Thread safety**: This type is not thread-safe. Instance properties are mutable and intended to be set immediately after construction by a single thread. Concurrent reads and writes without external synchronization may yield inconsistent state.
- **Inheritance**: Derives from `System.Exception`. All standard exception behaviors (stack trace capture, serialization, `InnerException` propagation) apply. Custom serialization is not implemented, so additional properties may not survive cross-domain remoting or binary serialization without a custom serialization constructor.
- **Re-throwing**: When re-throwing with `throw uploadEx;` (rather than `throw;`), the original stack trace is preserved because the constructor follows the standard exception chaining pattern. Use the `(string message, Exception innerException)` constructor to maintain full traceability.
