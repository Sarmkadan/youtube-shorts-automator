# IWebhookPublisher

`IWebhookPublisher` defines a contract for publishing typed event payloads to external webhook endpoints. It abstracts the serialization, delivery, and optional batching of events, allowing consumers to fire-and-forget notifications without coupling to transport details. The interface models a single event as a uniquely identified envelope carrying a timestamp, an event type discriminator, and a generic data payload.

## API

### WebhookPublisher

A concrete implementation reference exposed on the interface. This property provides access to the underlying publisher instance, enabling advanced scenarios such as inspecting configuration, diagnostics, or manual lifecycle management that fall outside the generic publish methods.

- **Type:** `WebhookPublisher`
- **Access:** get

### PublishEventAsync\<T\>

Publishes a single typed event to the configured webhook endpoint.

- **Signature:** `async Task<bool> PublishEventAsync<T>()`
- **Type Parameter `T`:** The type of the event payload, constrained by the implementation.
- **Returns:** A `Task<bool>` that resolves to `true` if the event was successfully delivered and acknowledged by the receiver; `false` if delivery failed, was rejected, or timed out.
- **Exceptions:** May throw `InvalidOperationException` if the publisher has not been initialized or configured. Network-level exceptions (`HttpRequestException`, `TaskCanceledException`) are typically caught internally and reflected in the `false` return value rather than propagated.

### PublishBulkEventsAsync\<T\>

Publishes multiple events of the same type in a single batch operation.

- **Signature:** `async Task<bool> PublishBulkEventsAsync<T>()`
- **Type Parameter `T`:** The type of the event payloads in the batch.
- **Returns:** A `Task<bool>` that resolves to `true` if the entire batch was successfully delivered; `false` if any event in the batch failed or the batch request itself was unsuccessful.
- **Exceptions:** Same error-handling semantics as `PublishEventAsync<T>` — initialization errors may propagate; transient delivery failures are captured in the return value.

### EventType

The discriminator string that identifies the category or name of the event being published (e.g., `"video.processed"`, `"upload.completed"`). This value is serialized into the webhook payload so receivers can route or filter events.

- **Type:** `string`
- **Access:** get; set

### Data

The generic payload associated with the event. When `T` is supplied through the publish methods, this property carries the strongly-typed instance that will be serialized into the webhook body.

- **Type:** `T?` (nullable)
- **Access:** get; set

### Timestamp

The UTC moment when the event was created or when the publish action was initiated. Included in the webhook envelope for ordering and auditing purposes.

- **Type:** `DateTime`
- **Access:** get; set

### Id

A unique identifier for the event instance, typically a GUID or similar distinct string. This allows receivers to deduplicate events and correlate delivery attempts.

- **Type:** `string`
- **Access:** get; set

## Usage

### Example 1: Publishing a Single Event

```csharp
var publisher = serviceProvider.GetRequiredService<IWebhookPublisher>();

publisher.EventType = "short.generated";
publisher.Id = Guid.NewGuid().ToString();
publisher.Timestamp = DateTime.UtcNow;
publisher.Data = new ShortGeneratedPayload
{
    VideoId = "abc123",
    Title = "My Automated Short",
    DurationSeconds = 28
};

bool delivered = await publisher.PublishEventAsync<ShortGeneratedPayload>();
if (!delivered)
{
    logger.LogWarning("Webhook delivery failed for event {EventId}", publisher.Id);
}
```

### Example 2: Publishing a Batch of Events

```csharp
var publisher = serviceProvider.GetRequiredService<IWebhookPublisher>();

var batchPayloads = recentUploads.Select(upload => new UploadCompletedPayload
{
    UploadId = upload.Id,
    Status = upload.Status,
    ProcessedAt = upload.FinishedAt
}).ToList();

publisher.EventType = "upload.batch.completed";
publisher.Id = Guid.NewGuid().ToString();
publisher.Timestamp = DateTime.UtcNow;
publisher.Data = batchPayloads;

bool batchDelivered = await publisher.PublishBulkEventsAsync<List<UploadCompletedPayload>>();
if (!batchDelivered)
{
    // The entire batch failed; individual events were not delivered.
    logger.LogError("Bulk webhook publish failed for batch {BatchId}", publisher.Id);
}
```

## Notes

- **Null Data:** The `Data` property is typed as `T?`, meaning it can be `null`. Publishers must decide whether to serialize a null payload, omit the data field, or reject the publish attempt. Consumers should verify that `Data` is assigned before calling publish methods.
- **Idempotency:** The `Id` field enables receivers to deduplicate events, but the interface itself does not enforce uniqueness. Callers are responsible for generating distinct IDs per event or batch.
- **Batch Atomicity:** `PublishBulkEventsAsync<T>` returns a single boolean indicating success or failure of the entire batch. Partial delivery (some events succeeded, some failed) is treated as a failure. If partial success tracking is required, callers should fall back to individual `PublishEventAsync<T>` calls.
- **Thread Safety:** The interface exposes mutable properties (`EventType`, `Data`, `Timestamp`, `Id`) that are set before calling publish methods. This design is inherently not thread-safe for concurrent use on the same instance. Multiple threads setting properties and calling publish methods on a shared `IWebhookPublisher` instance will encounter race conditions. Each publish operation should use its own instance or external synchronization must be applied.
- **Initialization:** Implementations may require explicit initialization (e.g., setting a target URL, authentication token) before publish methods succeed. Calling `PublishEventAsync<T>` or `PublishBulkEventsAsync<T>` on an unconfigured instance typically results in an `InvalidOperationException` or a `false` return value depending on the implementation's error-handling strategy.
