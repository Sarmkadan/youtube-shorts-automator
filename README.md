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

