# IHttpClientFactory

The `IHttpClientFactory` interface provides a centralized mechanism for instantiating and configuring pre-configured `HttpClient` instances tailored for distinct service endpoints within the `youtube-shorts-automator` application. It abstracts the complexities of managing `HttpMessageHandler` lifetimes, consistent request headers, and base addresses required for YouTube API communication, storage operations, webhooks, and analytics reporting.

## API

*   `DefaultHttpClientFactory`: The concrete class implementing `IHttpClientFactory` that handles the dependency injection-based construction of the specialized clients.
*   `CreateYouTubeApiClient()`: Creates an `HttpClient` configured for interacting with YouTube Data API endpoints. Returns an initialized `HttpClient` instance. Throws `InvalidOperationException` if YouTube API credentials are not correctly configured in the application environment.
*   `CreateStorageClient()`: Creates an `HttpClient` configured for cloud storage provider endpoints. Returns an initialized `HttpClient` instance.
*   `CreateWebhookClient()`: Creates an `HttpClient` configured for outbound webhook dispatching. Returns an initialized `HttpClient` instance.
*   `CreateAnalyticsClient()`: Creates an `HttpClient` configured for backend analytics service endpoints. Returns an initialized `HttpClient` instance.

## Usage

```csharp
// Example 1: Injecting into a service
public class YouTubeService
{
    private readonly HttpClient _httpClient;

    public YouTubeService(IHttpClientFactory httpClientFactory)
    {
        // Obtain a pre-configured YouTube API client
        _httpClient = httpClientFactory.CreateYouTubeApiClient();
    }
}

// Example 2: Using the factory to perform an operation
public async Task SendWebhook(IHttpClientFactory factory, object data)
{
    // Retrieve the webhook-specific client
    var client = factory.CreateWebhookClient();
    
    // Perform the request
    var response = await client.PostAsJsonAsync("/notify", data);
    response.EnsureSuccessStatusCode();
}
```

## Notes

*   `HttpClient` instances returned by these methods are designed to be long-lived and reused to prevent socket exhaustion. Do not dispose of the returned `HttpClient` instances directly.
*   The implementation is thread-safe, allowing safe concurrent calls to the `Create` methods from multiple threads within the application.
*   Configuration settings, such as base URLs and default request timeouts, are managed internally by the `DefaultHttpClientFactory` and should not be modified on the returned `HttpClient` instances.
