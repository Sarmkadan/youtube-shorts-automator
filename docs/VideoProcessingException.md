# VideoProcessingException

The `VideoProcessingException` class represents a specialized error condition occurring during the automated processing of video content within the `youtube-shorts-automator` pipeline. It extends the standard exception hierarchy to include contextual metadata such as the specific video identifier, the associated processing task ID, and a machine-readable error code, facilitating precise diagnostics and automated retry logic when video rendering, encoding, or upload tasks fail.

## API

### `VideoId`
```csharp
public int? VideoId { get; }
```
Retrieves the unique integer identifier of the video that triggered the exception. This property is nullable; it may be null if the error occurred before a valid video ID was assigned or resolved during the pipeline execution.

### `ProcessingTaskId`
```csharp
public string? ProcessingTaskId { get; }
```
Retrieves the unique string identifier for the background task or job instance responsible for processing the video. This allows correlation of the error with specific logs or queue messages. This property is nullable if the failure occurred outside a tracked task context.

### `ErrorCode`
```csharp
public string? ErrorCode { get; }
```
Retrieves a standardized, machine-readable string code representing the specific category of the processing failure (e.g., `ENCODING_FAILED`, `SOURCE_MISSING`). This property is nullable and should be used for programmatic error handling rather than parsing the exception message.

### `VideoProcessingException(string message)`
```csharp
public VideoProcessingException(string message) : base(message)
```
Initializes a new instance of the `VideoProcessingException` class with a specified error message.
*   **Parameters**:
    *   `message`: A string describing the reason for the exception.
*   **Remarks**: This constructor initializes the base exception message but leaves `VideoId`, `ProcessingTaskId`, and `ErrorCode` as null. It is typically used for general errors where specific context is not yet available.

### `VideoProcessingException` (Overload 1)
```csharp
public VideoProcessingException(string message, int videoId, string taskId, string errorCode)
```
Initializes a new instance of the `VideoProcessingException` class with a specified error message and full contextual metadata.
*   **Parameters**:
    *   `message`: A string describing the reason for the exception.
    *   `videoId`: The integer ID of the video being processed.
    *   `taskId`: The string ID of the processing task.
    *   `errorCode`: The string code categorizing the error.
*   **Remarks**: Use this constructor when the failure context is fully known at the throw site to ensure maximum observability.

### `VideoProcessingException` (Overload 2)
```csharp
public VideoProcessingException(string message, Exception innerException)
```
Initializes a new instance of the `VideoProcessingException` class with a specified error message and a reference to the inner exception that is the cause of this exception.
*   **Parameters**:
    *   `message`: A string describing the reason for the exception.
    *   `innerException`: The exception that caused the current exception, or null if no inner exception is specified.
*   **Remarks**: Preserves the stack trace of the underlying system error (e.g., FFmpeg failures, IO exceptions) while wrapping it in the domain-specific type. Contextual properties (`VideoId`, etc.) remain null.

### `VideoProcessingException` (Overload 3)
```csharp
public VideoProcessingException(string message, int videoId, string taskId, string errorCode, Exception innerException)
```
Initializes a new instance of the `VideoProcessingException` class with a specified error message, full contextual metadata, and a reference to the inner exception.
*   **Parameters**:
    *   `message`: A string describing the reason for the exception.
    *   `videoId`: The integer ID of the video being processed.
    *   `taskId`: The string ID of the processing task.
    *   `errorCode`: The string code categorizing the error.
    *   `innerException`: The exception that caused the current exception.
*   **Remarks**: This is the most verbose constructor, providing complete diagnostic information including the root cause stack trace and pipeline context.

## Usage

### Example 1: Throwing with Full Context
This example demonstrates throwing the exception when a specific encoding step fails, providing all available identifiers to aid in debugging.

```csharp
try
{
    await encoder.RenderAsync(videoId, taskContext.Id);
}
catch (FFmpegExecutionException ex)
{
    throw new VideoProcessingException(
        message: "Failed to render short due to codec incompatibility.",
        videoId: videoId,
        taskId: taskContext.Id,
        errorCode: "RENDER_CODEC_ERROR",
        innerException: ex
    );
}
```

### Example 2: Catching and Inspecting Metadata
This example shows how a global error handler might inspect the custom properties to route the error to the correct monitoring dashboard or retry queue.

```csharp
try
{
    await processingService.ExecuteTaskAsync(task);
}
catch (VideoProcessingException ex)
{
    if (ex.ErrorCode == "SOURCE_MISSING")
    {
        logger.LogWarning("Source file missing for Video {VideoId}", ex.VideoId);
        await notificationService.SendAlertAsync(ex.ProcessingTaskId, "Source Unavailable");
    }
    else
    {
        logger.LogError(ex, "Critical processing failure for task {TaskId}", ex.ProcessingTaskId);
        await retryQueue.EnqueueAsync(ex.ProcessingTaskId, ex.VideoId);
    }
}
```

## Notes

*   **Nullability**: Consumers must handle null values for `VideoId`, `ProcessingTaskId`, and `ErrorCode` gracefully, as the parameterless or simplified constructor overloads may be used in scenarios where context is not yet established.
*   **Thread Safety**: This exception class is immutable after construction; all public properties are read-only getters. Therefore, instances of `VideoProcessingException` are thread-safe for reading across multiple threads once thrown and caught.
*   **Serialization**: When serializing this exception for logging or transmission across process boundaries, ensure the serializer is configured to include custom properties (`VideoId`, `ProcessingTaskId`, `ErrorCode`), as standard exception serialization mechanisms may only capture the message and stack trace by default.
*   **Error Code Consistency**: The `ErrorCode` string should adhere to a predefined set of constants within the `youtube-shorts-automator` project to ensure reliable switching logic in error handlers. Avoid generating dynamic error codes at runtime.
