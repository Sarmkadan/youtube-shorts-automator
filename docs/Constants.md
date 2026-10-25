# Constants

The `Constants` class is a static container of compile-time constants for the console pipeline (`YouTubeShortAutomator.Constants` namespace, `src/Constants/Constants.cs`). It holds the YouTube Shorts video limits, YouTube Data API field limits, default processing and FFmpeg settings, directory and format names, timeouts, scheduling intervals, and file-size bounds.

Every member is a `const`, so each value is inlined at compile time. The class has no instance members and cannot be instantiated. Values are fixed in source; changing one requires a rebuild. The class is referenced by its fully qualified name in the pipeline code, for example `Constants.Constants.MAX_TITLE_LENGTH`.

The web API (`YouTubeShortsAutomator` namespace) does not use this class. It keeps its own defaults in `AppDefaults`.

## API

### Video constraints

*   **`MAX_DURATION_SECONDS = 60`**
    Maximum length of a Short, in seconds.
*   **`MIN_DURATION_SECONDS = 1`**
    Minimum length of a Short, in seconds.
*   **`MAX_FILE_SIZE_MB = 512`**
    Maximum input file size, in mebibytes. Also the source of `MAX_FILE_SIZE_BYTES`.
*   **`MIN_WIDTH_PIXELS = 360`** and **`MIN_HEIGHT_PIXELS = 640`**
    Minimum frame dimensions.
*   **`MAX_WIDTH_PIXELS = 1920`** and **`MAX_HEIGHT_PIXELS = 1080`**
    Maximum frame dimensions.

### YouTube limits

*   **`MAX_TITLE_LENGTH = 100`**
    Maximum title length, in characters.
*   **`MAX_DESCRIPTION_LENGTH = 5000`**
    Maximum description length, in characters.
*   **`MAX_TAGS_COUNT = 500`**
    Maximum number of tags on one video.
*   **`MAX_TAG_LENGTH = 30`**
    Maximum length of a single tag, in characters.

### Processing

*   **`DEFAULT_RETRY_COUNT = 3`**
    Default retry count. Used as `MaxRetries` by `SchedulingService` and `YouTubeUploadService`.
*   **`DEFAULT_UPLOAD_TIMEOUT_SECONDS = 7200`**
    Default upload timeout, which is two hours.
*   **`PROCESSING_QUEUE_LIMIT = 100`**
    Maximum number of items allowed in the processing queue.
*   **`BATCH_PROCESSING_SIZE = 5`**
    Number of items processed per batch.

### FFmpeg defaults

*   **`DEFAULT_VIDEO_CODEC = "libx264"`**
    Default video encoder.
*   **`DEFAULT_AUDIO_CODEC = "aac"`**
    Default audio encoder.
*   **`DEFAULT_CONTAINER = "mp4"`**
    Default container format.
*   **`DEFAULT_BITRATE_KBPS = 2000`**
    Default video bitrate, in kbps.
*   **`DEFAULT_AUDIO_BITRATE_KBPS = 128`**
    Default audio bitrate, in kbps.
*   **`DEFAULT_FPS = 30`**
    Default frame rate.

### Paths and formats

*   **`LOG_DIRECTORY = "logs"`**
    Directory for log files.
*   **`PROCESSING_DIRECTORY = "processing"`**
    Directory for files that are being processed.
*   **`OUTPUT_DIRECTORY = "output"`**
    Directory for finished output files.
*   **`TEMP_DIRECTORY = "temp"`**
    Directory for temporary files. `src/Program.cs` lists it with the log, processing and output directories when it creates the working directories at startup.
*   **`SUPPORTED_INPUT_FORMATS = "mp4,avi,mov,mkv,flv,wmv"`**
    Comma-separated list of accepted input extensions. It is one string, not an array, so callers must split it.
*   **`OUTPUT_FORMAT = "mp4"`**
    Container format for output files.

### Timeouts

*   **`API_TIMEOUT_SECONDS = 30`**
    Timeout for outbound API calls, in seconds.
*   **`DATABASE_TIMEOUT_SECONDS = 30`**
    Timeout for database operations, in seconds.
*   **`FFMPEG_TIMEOUT_SECONDS = 3600`**
    Timeout for an FFmpeg run, which is one hour.

### Batch and scheduling

*   **`SCHEDULE_CHECK_INTERVAL_SECONDS = 60`**
    How often the scheduler checks for due uploads, in seconds.
*   **`ANALYTICS_SYNC_INTERVAL_HOURS = 6`**
    How often analytics are synced, in hours.
*   **`TOKEN_REFRESH_BUFFER_HOURS = 1`**
    How long before expiry an OAuth token is refreshed, in hours.

### File validation

*   **`MAX_FILE_SIZE_BYTES`**
    Equal to `MAX_FILE_SIZE_MB * 1024 * 1024`, which is 536,870,912 bytes (512 MiB). It is a `long`.
*   **`MIN_FILE_SIZE_BYTES = 1024`**
    Minimum file size, which is 1 KB. It is a `long`.

### Performance

These members sit at the end of the class and have no section heading in the source.

*   **`MAX_CONCURRENT_UPLOADS = 3`**
    Maximum number of uploads running at the same time.
*   **`MAX_CONCURRENT_PROCESSING_TASKS = 2`**
    Maximum number of processing tasks running at the same time.
*   **`CACHE_EXPIRATION_MINUTES = 30`**
    Cache entry lifetime, in minutes.

## Usage

### Checking a title against the YouTube limit

`MetadataService` and `YouTubeUploadService` compare titles against `MAX_TITLE_LENGTH` before upload.

```csharp
if (title.Length > Constants.Constants.MAX_TITLE_LENGTH)
{
    // reject or truncate the title
}
```

### Setting a retry policy

`SchedulingService` and `YouTubeUploadService` set `MaxRetries` from `DEFAULT_RETRY_COUNT` when they create an upload job.

```csharp
var job = new UploadJob
{
    MaxRetries = Constants.Constants.DEFAULT_RETRY_COUNT
};
```

### Splitting the supported input formats

`SUPPORTED_INPUT_FORMATS` is one comma-separated string. Split it before checking an extension.

```csharp
var formats = Constants.Constants.SUPPORTED_INPUT_FORMATS.Split(',');
bool supported = formats.Contains(extension.TrimStart('.'));
```

## Notes

*   **Unused members**: Many members have no reference outside `src/Constants/Constants.cs`. These include the frame dimension limits, `DEFAULT_FPS`, the FFmpeg codec and bitrate defaults, `LOG_DIRECTORY`, `PROCESSING_DIRECTORY`, `OUTPUT_FORMAT`, the three timeouts, the scheduling and token intervals, and `CACHE_EXPIRATION_MINUTES`. Check for usage before relying on one of them.
*   **Compile-time inlining**: Because the members are `const`, a change to a value only takes effect after a rebuild of every project that uses it.
*   **Derived value**: `MAX_FILE_SIZE_BYTES` is computed from `MAX_FILE_SIZE_MB`. Change the megabyte value and the byte value follows.
*   **Units**: Units are in the member names, for example `_SECONDS`, `_HOURS`, `_KBPS`, and `_BYTES`. `CACHE_EXPIRATION_MINUTES` is in minutes, and `MAX_FILE_SIZE_MB` is in mebibytes.
*   **Naming**: Constants use `UPPER_SNAKE_CASE`, which differs from the PascalCase rule in `.editorconfig` for public members. Keep this style for new members in this class.
