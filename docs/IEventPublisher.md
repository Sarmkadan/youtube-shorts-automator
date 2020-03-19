# IEventPublisher

Defines a generic publish–subscribe contract for domain events within the `youtube-shorts-automator` application. Implementations allow components to subscribe to and unsubscribe from events of a specific type, and to publish events asynchronously to all registered handlers. The interface also exposes a concrete `EventPublisher` instance for advanced scenarios that require direct access to the underlying publisher.

## API

### `EventPublisher`

- **Type:** `EventPublisher` (property)
- **Description:** Gets the underlying `EventPublisher` instance associated with this interface. This property can be used to access publisher-specific functionality that is not exposed through the generic methods, such as configuration or direct event publishing bypassing the generic abstraction.
- **Return value:** An instance of the `EventPublisher` class.
- **Exceptions:** None.

### `void Subscribe<TEvent>`

- **Description:** Registers a handler for events of type `TEvent`. When an event of this type is published, the handler will be invoked.
- **Type parameters:** `TEvent` – The type of event to subscribe to.
- **Parameters:** `handler` (delegate, e.g., `Action<TEvent>`) – The handler to invoke when the event is published.
- **Return value:** None.
- **Exceptions:**
  - `ArgumentNullException` – Thrown if `handler` is `null`.

### `void Unsubscribe<TEvent>`

- **Description:** Removes a previously registered handler for events of type `TEvent`. After unsubscribing, the handler will no longer be invoked when the event is published.
- **Type parameters:** `TEvent` – The type of event to unsubscribe from.
- **Parameters:** `handler` (delegate, e.g., `Action<TEvent>`) – The handler to remove.
- **Return value:** None.
- **Exceptions:**
  - `ArgumentNullException` – Thrown if `handler` is `null`.
  - `InvalidOperationException` – Thrown if the handler was not previously subscribed.

### `async Task PublishAsync<TEvent>`

- **Description:** Publishes an event of type `TEvent` to all currently subscribed handlers. The method returns a task that completes when all handlers have finished execution.
- **Type parameters:** `TEvent` – The type of event to publish.
- **Parameters:** `eventData` (of type `TEvent`) – The event data to pass to each handler.
- **Return value:** A `Task` representing the asynchronous publish operation.
- **Exceptions:**
  - `ArgumentNullException` – Thrown if `eventData` is `null`.
  - `AggregateException` – May contain exceptions thrown by individual handlers. The publish operation does not stop on the first failure; all handlers are invoked, and any exceptions are collected.

## Usage

### Example 1: Basic subscribe, publish, and unsubscribe

```csharp
public class VideoProcessedEvent
{
    public string VideoId { get; set; }
}

public class NotificationService
{
    private readonly IEventPublisher _publisher;

    public NotificationService(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public void RegisterHandlers()
    {
        _publisher.Subscribe<VideoProcessedEvent>(OnVideoProcessed);
    }

    public void UnregisterHandlers()
    {
        _publisher.Unsubscribe<VideoProcessedEvent>(OnVideoProcessed);
    }

    private void OnVideoProcessed(VideoProcessedEvent e)
    {
        Console.WriteLine($"Video {e.VideoId} processed.");
    }
}

// Usage
var publisher = new EventPublisher(); // concrete implementation
var service = new NotificationService(publisher);
service.RegisterHandlers();

await publisher.PublishAsync(new VideoProcessedEvent { VideoId = "abc123" });
// Output: Video abc123 processed.

service.UnregisterHandlers();
```

### Example 2: Publishing with multiple subscribers and error handling

```csharp
public class ShortsUploadedEvent
{
    public string ShortsId { get; set; }
}

public class AnalyticsTracker
{
    public void OnShortsUploaded(ShortsUploadedEvent e)
    {
        // Simulate a failure
        throw new InvalidOperationException("Analytics service unavailable.");
    }
}

public class LoggingService
{
    public void OnShortsUploaded(ShortsUploadedEvent e)
    {
        Console.WriteLine($"Shorts {e.ShortsId} uploaded at {DateTime.UtcNow}.");
    }
}

// Setup
var publisher = new EventPublisher();
var analytics = new AnalyticsTracker();
var logger = new LoggingService();

publisher.Subscribe<ShortsUploadedEvent>(analytics.OnShortsUploaded);
publisher.Subscribe<ShortsUploadedEvent>(logger.OnShortsUploaded);

try
{
    await publisher.PublishAsync(new ShortsUploadedEvent { ShortsId = "shorts456" });
}
catch (AggregateException ex)
{
    foreach (var inner in ex.InnerExceptions)
    {
        Console.WriteLine($"Handler error: {inner.Message}");
    }
}
// Output:
// Shorts shorts456 uploaded at ...
// Handler error: Analytics service unavailable.
```

## Notes

- **Thread safety:** Implementations of `IEventPublisher` are expected to be thread-safe. Subscribing, unsubscribing, and publishing can occur concurrently from multiple threads without corrupting internal state. However, handlers themselves are invoked on the thread that calls `PublishAsync` unless the implementation explicitly offloads execution.
- **Null handlers:** Passing `null` to `Subscribe` or `Unsubscribe` throws `ArgumentNullException`. Always validate handler references before registration.
- **Unsubscribing a non‑existent handler:** Attempting to unsubscribe a handler that was never registered (or already removed) throws `InvalidOperationException`. Use a flag or check subscription state if this is a concern.
- **Handler exceptions:** `PublishAsync` does not stop on the first handler failure. All subscribed handlers are invoked, and any exceptions are aggregated into an `AggregateException`. Handlers should be written to tolerate failures in other handlers.
- **Event type uniqueness:** The interface treats `TEvent` as the event identifier. Subscribing the same handler twice for the same event type results in duplicate invocations. Unsubscribing removes only one registration; if the handler was registered multiple times, it must be unsubscribed the same number of times.
- **`EventPublisher` property:** The exposed `EventPublisher` instance is the same object that implements the interface. Direct manipulation of the underlying publisher (e.g., calling methods not defined in `IEventPublisher`) may bypass the generic abstraction and should be used with caution.
