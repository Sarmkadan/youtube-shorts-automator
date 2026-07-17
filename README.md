// ... (rest of README.md content remains unchanged)

## IHttpClientFactory

`IHttpClientFactory` is a factory interface for creating and managing instances of `HttpClient` with predefined configurations. It provides a centralized way to create HTTP clients for different services such as YouTube API, storage, webhooks, and analytics. This abstraction helps manage the lifecycle of HTTP clients and ensures consistent configurations across the application.

### Usage Example

```csharp
var httpClientFactory = new DefaultHttpClientFactory(configuration, logger);

var youTubeApiClient = httpClientFactory.CreateYouTubeApiClient();
var storageClient = httpClientFactory.CreateStorageClient();
var webhookClient = httpClientFactory.CreateWebhookClient();
var analyticsClient = httpClientFactory.CreateAnalyticsClient();

// Use the created HTTP clients for making requests
var youtubeResponse = await youTubeApiClient.GetAsync("https://api.youtube.com");
var storageResponse = await storageClient.GetAsync("https://storage.example.com");
var webhookResponse = await webhookClient.PostAsync("https://webhook.example.com", content);
var analyticsResponse = await analyticsClient.GetAsync("https://analytics.example.com");
```

## IGoogleApiClient

`IGoogleApiClient` is an interface for interacting with the YouTube API, providing methods for uploading videos, retrieving video analytics, updating video metadata, and listing available YouTube channels. It abstracts YouTube-specific operations and provides strongly-typed models for video analytics data.

### Usage Example

```csharp
// Initialize the Google API client
var googleApiClient = new GoogleApiClient(httpClientFactory, logger, configuration);

// Upload a new video to YouTube
var uploadId = await googleApiClient.UploadVideoAsync(
    "./videos/my-short.mp4",
    new VideoMetadata
    {
        Title = "My Awesome Short Video",
        Description = "Check out this amazing short video!",
        Tags = new List<string> { "shorts", "funny", "entertainment" },
        CategoryId = "22" // Entertainment category
    }
);

if (uploadId != null)
{
    Console.WriteLine($"Video uploaded successfully with ID: {uploadId}");
    
    // Retrieve analytics for the uploaded video
    var analytics = await googleApiClient.GetVideoAnalyticsAsync(uploadId);
    
    if (analytics != null)
    {
        Console.WriteLine($"Video Analytics for {analytics.VideoId}:");
        Console.WriteLine($"  Views: {analytics.ViewCount}");
        Console.WriteLine($"  Likes: {analytics.LikeCount}");
        Console.WriteLine($"  Comments: {analytics.CommentCount}");
        Console.WriteLine($"  Shares: {analytics.ShareCount}");
        Console.WriteLine($"  Engagement Rate: {analytics.EngagementRate:P}");
        Console.WriteLine($"  Fetched at: {analytics.FetchedAtUtc:u}");
        
        // Update video metadata
        var updateSuccess = await googleApiClient.UpdateVideoMetadataAsync(
            uploadId,
            new VideoMetadata
            {
                Title = "My Awesome Short Video - Updated",
                Description = "Check out this amazing short video! Updated description.",
                Tags = new List<string> { "shorts", "funny", "entertainment", "updated" }
            }
        );
        
        Console.WriteLine($"Metadata update successful: {updateSuccess}");
    }
}

// List all available YouTube channels
var channels = await googleApiClient.ListChannelsAsync();
Console.WriteLine($"Available channels: {string.Join(", ", channels)}");
```

## ApiResult

`ApiResult` is a generic wrapper class for API operations that encapsulates success/failure state with optional data and error details. It provides a consistent way to handle API responses throughout the application, supporting both simple operations and operations that return data.

### Members

- `IsSuccess` - Indicates whether the operation was successful
- `Message` - Human-readable status message
- `ErrorCode` - Optional error code for programmatic error handling
- `Errors` - Dictionary of validation errors (when applicable)

- Static factory methods: `Success()`, `Failure()`, `ValidationFailure()`

### Usage Examples

#### Basic ApiResult (non-generic)

```csharp
// Successful operation
var successResult = ApiResult.Success("Video uploaded successfully");
Console.WriteLine($"Success: {successResult.IsSuccess}, Message: {successResult.Message}");

// Failed operation
var failureResult = ApiResult.Failure("Failed to process video", "VIDEO_PROCESSING_ERROR");
Console.WriteLine($"Success: {failureResult.IsSuccess}, Error: {failureResult.Message}, Code: {failureResult.ErrorCode}");

// Validation failure
var validationErrors = new Dictionary<string, string>
{
    { "title", "Title is required" },
    { "description", "Description must be at least 50 characters" }
};
var validationResult = ApiResult.ValidationFailure(validationErrors);
Console.WriteLine($"Validation failed with {validationResult.Errors?.Count} errors");
```

#### Generic ApiResult<T>

```csharp
// Successful operation with data
var videoData = new { Id = "abc123", Title = "My Short Video" };
var successWithData = ApiResult<VideoData>.Success(videoData, "Video retrieved successfully");

if (successWithData.IsSuccess)
{
    Console.WriteLine($"Video ID: {successWithData.Data?.Id}, Title: {successWithData.Data?.Title}");
}

// Failed operation with data type
var notFoundResult = ApiResult<VideoData>.NotFound("Video with ID xyz789 not found");
Console.WriteLine($"Not found: {notFoundResult.IsSuccess}, Message: {notFoundResult.Message}");

// Using Map to transform data
var mappedResult = successWithData.Map(data => data?.Title?.ToUpper());
Console.WriteLine($"Mapped title: {mappedResult}");

// Async mapping
var asyncMappedResult = await successWithData.MapAsync(async data => 
    (await Task.FromResult(data?.Id)) ?? "unknown");
Console.WriteLine($"Async mapped ID: {asyncMappedResult}");

// Conflict scenario
var conflictResult = ApiResult<VideoData>.Conflict("Video with this title already exists");
Console.WriteLine($"Conflict detected: {conflictResult.IsSuccess}, Code: {conflictResult.ErrorCode}");
```

