# IGoogleApiClient

`IGoogleApiClient` serves as the primary abstraction for interacting with YouTube Google APIs within the `youtube-shorts-automator` application, providing essential functionality for video management, metadata updates, and performance analytics.

## API

*   `public GoogleApiClient`
    The primary implementation class for the `IGoogleApiClient` interface.

*   `public async Task<string?> UploadVideoAsync(string filePath, string title, string description)`
    Uploads a video to YouTube. Returns the unique `VideoId` on success, or `null` if the operation fails.

*   `public async Task<VideoAnalytics?> GetVideoAnalyticsAsync(string videoId)`
    Retrieves comprehensive analytics for the specified video. Returns a `VideoAnalytics` object, or `null` if the data is unavailable.

*   `public async Task<bool> UpdateVideoMetadataAsync(string videoId, string title, string description)`
    Updates the title and description of the specified video. Returns `true` if the update is successful, `false` otherwise.

*   `public async Task<List<string>> ListChannelsAsync()`
    Retrieves a list of identifiers for channels associated with the authenticated Google account.

*   `public string VideoId`
    The unique identifier for the video currently being processed or analyzed.

*   `public int ViewCount`
    The current total view count for the associated video.

*   `public int LikeCount`
    The current total like count for the associated video.

*   `public int CommentCount`
    The current total comment count for the associated video.

*   `public int ShareCount`
    The current total share count for the associated video.

*   `public double EngagementRate`
    The calculated engagement rate for the video, based on interaction metrics.

*   `public DateTime FetchedAtUtc`
    The UTC timestamp indicating the last successful synchronization of metrics from the YouTube API.

## Usage

```csharp
// Example 1: Uploading a video
var client = new GoogleApiClient();
string? videoId = await client.UploadVideoAsync("path/to/video.mp4", "New Short", "Description");
if (videoId != null)
{
    Console.WriteLine($"Video uploaded successfully: {videoId}");
}

// Example 2: Retrieving video analytics
var analytics = await client.GetVideoAnalyticsAsync("existingVideoId");
if (analytics != null)
{
    Console.WriteLine($"Views: {analytics.ViewCount}, Likes: {analytics.LikeCount}");
}
```

## Notes

*   **Thread Safety**: Implementations of `IGoogleApiClient` are expected to handle concurrent `async` operations safely. Ensure proper lifecycle management of the client instance, particularly if reusing the underlying HTTP client.
*   **Exception Handling**: Methods performing network operations, such as `UploadVideoAsync` and `GetVideoAnalyticsAsync`, should be wrapped in `try-catch` blocks to handle potential exceptions arising from network failures, authentication issues, or API quota exhaustion.
*   **Property Synchronization**: Properties such as `ViewCount` and `FetchedAtUtc` represent state that is only guaranteed to be accurate as of the last successful `GetVideoAnalyticsAsync` call.
