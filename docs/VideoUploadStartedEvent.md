# VideoUploadStartedEvent

Represents the event payload emitted when a video upload operation is initiated within the YouTube Shorts Automator pipeline. This type carries identifying metadata, file characteristics, and initial processing parameters that downstream handlers can use to track, retry, or audit the upload lifecycle.

## API

### VideoId
`public Guid VideoId`

Unique identifier for the video entity within the system. This value is assigned at the start of the upload workflow and remains stable across retries and status transitions.

### FileName
`public string FileName`

Original name of the file being uploaded, including its extension. Derived from the input file path and preserved for display and logging purposes.

### FileSizeBytes
`public long FileSizeBytes`

Size of the input file in bytes before any processing or transcoding. Used for quota checks, progress estimation, and diagnostic comparisons against the output file size.

### Title
`public string Title`

Human-readable title assigned to the video. This value is typically extracted from the processing profile or metadata and is used when creating the YouTube resource.

### YouTubeVideoId
`public string YouTubeVideoId`

The identifier assigned by YouTube after the video resource is created. This field may be empty if the event fires before the YouTube API call completes or if the initial creation attempt fails.

### YouTubeUrl
`public string YouTubeUrl`

Full URL to the uploaded video on YouTube, constructed from the `YouTubeVideoId`. This field is populated only when the YouTube identifier is successfully obtained.

### UploadedAtUtc
`public DateTime UploadedAtUtc`

UTC timestamp marking when the upload operation was initiated. This value is set once at event creation and does not change across retries.

### ErrorMessage
`public string ErrorMessage`

Human-readable description of any error encountered during the upload attempt. This field is `null` or empty when the operation succeeds on the first try.

### ErrorCode
`public string ErrorCode`

Machine-readable code identifying the error category (e.g., `"QuotaExceeded"`, `"NetworkFailure"`). Intended for programmatic retry logic and alert routing.

### RetryCount
`public int RetryCount`

Number of times the upload has been retried at the point this event was raised. A value of zero indicates the initial attempt.

### InputFilePath
`public string InputFilePath`

Absolute or relative path to the source file that was submitted for upload. This path is resolved before the upload begins and remains constant across retries.

### ProcessingProfile
`public string ProcessingProfile`

Name of the processing profile applied to this video (e.g., `"default"`, `"high-quality"`). Determines transcoding settings, resolution targets, and other pipeline behaviors.

### OutputFilePath
`public string OutputFilePath`

Path to the processed output file that is actually uploaded to YouTube. This may differ from `InputFilePath` when transcoding or format conversion occurs.

### OutputFileSizeBytes
`public long OutputFileSizeBytes`

Size of the processed output file in bytes. Compare with `FileSizeBytes` to assess compression ratios or detect unexpected processing results.

### VideoResolution
`public string VideoResolution`

Final resolution of the uploaded video, expressed as a string (e.g., `"1920x1080"`, `"1080p"`). Reflects the output of the processing step, not necessarily the source resolution.

## Usage

### Example 1: Handling a Successful First Attempt

```csharp
public void OnVideoUploadStarted(VideoUploadStartedEvent evt)
{
    if (evt.RetryCount == 0 && string.IsNullOrEmpty(evt.ErrorMessage))
    {
        _logger.Information(
            "Upload initiated for {VideoId} ({FileName}, {FileSizeBytes} bytes) at {UploadedAtUtc}",
            evt.VideoId,
            evt.FileName,
            evt.FileSizeBytes,
            evt.UploadedAtUtc);

        _dashboard.UpdateStatus(evt.VideoId, new
        {
            evt.Title,
            evt.ProcessingProfile,
            OutputResolution = evt.VideoResolution,
            Status = "Uploading"
        });
    }
}
```

### Example 2: Logging and Escalating a Retry with Errors

```csharp
public async Task HandleUploadRetryAsync(VideoUploadStartedEvent evt, CancellationToken ct)
{
    if (evt.RetryCount > 0 && !string.IsNullOrEmpty(evt.ErrorCode))
    {
        _logger.Warning(
            "Retry #{RetryCount} for {VideoId} after error {ErrorCode}: {ErrorMessage}",
            evt.RetryCount,
            evt.VideoId,
            evt.ErrorCode,
            evt.ErrorMessage);

        if (evt.RetryCount >= _maxRetries)
        {
            await _alertService.EscalateAsync(new
            {
                evt.VideoId,
                evt.Title,
                evt.InputFilePath,
                evt.OutputFilePath,
                LastError = evt.ErrorMessage,
                evt.UploadedAtUtc
            }, ct);
        }
    }
}
```

## Notes

- The `VideoId` property appears multiple times in the member listing; consumers should treat all occurrences as referring to the same logical identifier. Any duplication in deserialization or mapping layers must preserve a single consistent value.
- `YouTubeVideoId` and `YouTubeUrl` are populated asynchronously relative to the event’s creation. Handlers must guard against null or empty values when these fields are consumed before the YouTube API response arrives.
- `FileSizeBytes` and `OutputFileSizeBytes` may differ significantly when transcoding changes the bitrate or container format. Always use `OutputFileSizeBytes` for bandwidth and quota calculations related to the actual upload.
- `UploadedAtUtc` is set once and does not update on retries. To track the most recent attempt time, combine this event with subsequent status events rather than relying on this timestamp alone.
- This type is an event payload, not a mutable entity. Once constructed and emitted, its properties should be treated as read-only by all subscribers. Thread safety is the responsibility of the event bus or mediator dispatching the event; individual handlers should avoid mutating the instance.
