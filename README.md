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

