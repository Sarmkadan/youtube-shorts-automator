# VideoController

The `VideoController` class is an ASP.NET Core controller providing the primary HTTP interface for managing video operations within the `youtube-shorts-automator` system. It acts as an orchestrator, delegating tasks—such as video processing, YouTube API interactions, and status tracking—to underlying services, ensuring that video lifecycle management remains responsive, asynchronous, and consistent.

## API

### VideoController()
Initializes a new instance of the `VideoController` class, typically via dependency injection to provide required services such as video processing engines, logging, and database context.

### Task&lt;IActionResult&gt; GetUserVideos()
Retrieves a list of videos associated with the currently authenticated user.
*   **Returns**: `Ok` containing a collection of video metadata, or an error status if retrieval fails.

### Task&lt;IActionResult&gt; GetVideo(string videoId)
Fetches detailed information for a specific video identified by its unique ID.
*   **Parameters**: `videoId` (string) - The unique identifier of the video.
*   **Returns**: `Ok` containing video details, `NotFound` if the ID does not exist, or `BadRequest` if the input is invalid.

### Task&lt;IActionResult&gt; ProcessVideo(string videoId, ProcessRequest request)
Initiates a background processing job for the specified video, such as transcoding or formatting.
*   **Parameters**: `videoId` (string) - The ID of the video to process; `request` (ProcessRequest) - Configuration settings for the processing job.
*   **Returns**: `Accepted` with a reference to the job ID, or `BadRequest` if processing cannot be initiated.

### Task&lt;IActionResult&gt; UploadVideo(UploadRequest request)
Handles the reception and initial storage of a new video file and its associated metadata.
*   **Parameters**: `request` (UploadRequest) - Encapsulates the video file stream and metadata.
*   **Returns**: `Created` with the new video ID, or `UnprocessableEntity` if validation fails.

### Task&lt;IActionResult&gt; GetAnalytics(string videoId)
Fetches performance metrics (e.g., views, engagement) for a published video.
*   **Parameters**: `videoId` (string) - The ID of the video to query.
*   **Returns**: `Ok` containing the analytics report, or `NotFound` if the video is not published or data is unavailable.

### Task&lt;IActionResult&gt; GetJobStatus(string jobId)
Checks the current status of an ongoing or completed background video processing job.
*   **Parameters**: `jobId` (string) - The identifier for the background job.
*   **Returns**: `Ok` containing the current job state (e.g., Pending, Processing, Completed, Failed).

### Task&lt;IActionResult&gt; PublishVideo(string videoId)
Triggers the final publication of a processed video to the connected YouTube channel.
*   **Parameters**: `videoId` (string) - The ID of the processed video.
*   **Returns**: `Ok` upon successful publication, or `Conflict`/`InternalServerError` if the publication process fails.

## Usage

### Injecting and Using the Controller
```csharp
// Example of accessing the controller in a functional test harness
public class VideoIntegrationTests
{
    private readonly VideoController _controller;

    public VideoIntegrationTests(VideoController controller)
    {
        _controller = controller;
    }

    public async Task TestVideoLifecycle(string videoId)
    {
        // Retrieve video details
        var result = await _controller.GetVideo(videoId);
        // ... assertions ...
    }
}
```

### Invoking Video Processing
```csharp
// Example action within a client application
[HttpPost]
public async Task<IActionResult> StartProcessing(string videoId)
{
    var request = new ProcessRequest { Quality = "1080p", Format = "mp4" };
    var result = await _controller.ProcessVideo(videoId, request);
    
    return result; // Propagates the Accepted or BadRequest response
}
```

## Notes

*   **Asynchronous Execution**: All public methods return `Task<IActionResult>` to ensure non-blocking I/O operations, particularly critical when interacting with external APIs or long-running file processing tasks. These should always be awaited.
*   **Thread Safety**: ASP.NET Core controllers are instantiated per request. Ensure that any injected services are registered with the appropriate lifetime (e.g., Scoped or Singleton) and that the controller instance does not maintain mutable, shared state across different HTTP requests.
*   **Error Handling**: Consumers should be prepared to handle various HTTP status codes returned by these methods, as they encapsulate complex backend logic that may result in `NotFound` (missing resource), `Conflict` (state machine violations), or `InternalServerError` (unexpected API failures).
