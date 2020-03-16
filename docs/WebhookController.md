# WebhookController

`WebhookController` manages the lifecycle of webhook registrations within the YouTube Shorts Automator system. It exposes endpoints to register, retrieve, list, update, and delete webhooks, enabling external services to subscribe to events such as video processing completion or publishing status changes. The controller handles both the incoming HTTP requests and the persistence of webhook configurations, ensuring that each registered endpoint receives signed payloads for the specified event types.

## API

### public WebhookController

Constructor. Initializes a new instance of the controller with the required dependencies for webhook storage and event dispatching. No parameters are exposed publicly; dependencies are injected by the framework.

### public async Task\<IActionResult\> RegisterWebhookAsync

Registers a new webhook subscription. Accepts a payload containing the target `Url`, the `Events` array specifying which event types to subscribe to, and an optional `Secret` for payload signing. Returns `201 Created` with the assigned `WebhookId` and full registration details on success. Throws an `ArgumentException` if the URL is malformed or the events array is empty. Throws a `DuplicateWebhookException` when a webhook with the same URL and overlapping events already exists.

### public async Task\<IActionResult\> GetWebhookAsync

Retrieves a single webhook by its unique identifier. Expects a `Guid WebhookId` route parameter. Returns `200 OK` with the webhook details, including `Url`, `Events`, `Status`, `IsActive`, `Secret` (masked), and `CreatedAtUtc`. Returns `404 Not Found` if no webhook matches the provided identifier.

### public async Task\<IActionResult\> ListWebhooksAsync

Lists all registered webhooks, optionally filtered by `IsActive` status. When the `IsActive` query parameter is supplied, only webhooks matching that boolean state are returned. Returns `200 OK` with an array of webhook summaries, each containing `WebhookId`, `Url`, `Events`, and `Status`. Returns an empty array if no webhooks match the filter.

### public async Task\<IActionResult\> UpdateWebhookAsync

Updates an existing webhook registration. Accepts a `Guid WebhookId` route parameter and a request body containing the fields to modify: `Url`, `Events`, `IsActive`, and `Secret`. All fields are optional; only the provided fields are changed. Returns `200 OK` with the updated webhook object. Returns `404 Not Found` if the webhook does not exist. Throws an `ArgumentException` if the new URL is invalid or the events array, when provided, is empty.

### public async Task\<IActionResult\> DeleteWebhookAsync

Permanently removes a webhook registration. Expects a `Guid WebhookId` route parameter. Returns `204 No Content` on successful deletion. Returns `404 Not Found` if the webhook does not exist. Idempotent—calling delete on an already-deleted webhook returns `404` rather than throwing.

### public string Url

The destination URL to which webhook payloads are delivered. Must be an absolute HTTPS URL. Read-only after creation unless modified through `UpdateWebhookAsync`.

### public string[] Events

The set of event type strings this webhook subscribes to. Typical values include `"video.processed"`, `"video.published"`, and `"video.failed"`. At least one event must be specified during registration.

### public string? Url

Nullable URL property used in update requests. When non-null in an update payload, it replaces the existing webhook URL. When null, the existing URL is preserved.

### public string[]? Events

Nullable events array used in update requests. When non-null, it replaces the existing event subscriptions entirely. When null, the existing events are preserved.

### public bool? IsActive

Nullable boolean used in update and list-filtering contexts. In an update request, a non-null value enables or disables the webhook without deleting it. In list queries, filters the result set to only active or inactive webhooks.

### public Guid WebhookId

Unique identifier assigned to each webhook upon registration. Used as the primary key for retrieval, update, and deletion operations. Immutable after creation.

### public string Status

Current operational status of the webhook. Derived from the `IsActive` flag and recent delivery success rates. Possible values include `"active"`, `"inactive"`, and `"failing"`. Read-only; modified indirectly by toggling `IsActive` or by the system based on delivery outcomes.

### public string Secret

A shared secret used to sign outgoing webhook payloads with HMAC-SHA256. Provided during registration and optionally updated later. Never returned in plaintext from `GetWebhookAsync` or `ListWebhooksAsync`; displayed masked unless the caller holds administrative privileges.

### public bool IsActive

Indicates whether the webhook is currently enabled for delivery. When `false`, no payloads are dispatched for this subscription. Defaults to `true` on registration.

### public DateTime CreatedAtUtc

UTC timestamp marking when the webhook was registered. Immutable; set once at creation time.

## Usage

### Example 1: Registering a Webhook for Video Events

```csharp
// Register a webhook to receive notifications when videos are processed or published
var registrationPayload = new
{
    Url = "https://example.com/webhook-receiver",
    Events = new[] { "video.processed", "video.published" },
    Secret = "whsec_8f7a3b2c1d4e5f6a7b8c9d0e1f2a3b4c"
};

var result = await controller.RegisterWebhookAsync(registrationPayload);
// Returns 201 Created with WebhookId, Url, Events, IsActive = true, CreatedAtUtc
```

### Example 2: Disabling a Failing Webhook

```csharp
// Retrieve the webhook, then disable it due to repeated delivery failures
var getResult = await controller.GetWebhookAsync(webhookId);
if (getResult is OkObjectResult okResult)
{
    dynamic webhook = okResult.Value;
    if (webhook.Status == "failing")
    {
        var updatePayload = new
        {
            IsActive = (bool?)false
        };
        await controller.UpdateWebhookAsync(webhookId, updatePayload);
        // Webhook is now inactive; no further delivery attempts will be made
    }
}
```

## Notes

- **Idempotency of Delete**: `DeleteWebhookAsync` returns `404` for non-existent webhooks rather than throwing, making it safe to call repeatedly without guarding against race conditions where another process may have already removed the registration.
- **Partial Updates**: `UpdateWebhookAsync` applies only the fields present in the request body. Omitting a field leaves the existing value intact. Passing an explicit `null` for `Url` or `Events` preserves the current value; passing an empty array for `Events` is treated as a validation error.
- **Event Overlap Detection**: `RegisterWebhookAsync` checks for duplicate subscriptions by comparing both URL and event types. Two webhooks pointing to the same URL may coexist if their event sets are disjoint. Overlapping events at the same URL trigger a `DuplicateWebhookException`.
- **Secret Masking**: The `Secret` field is never exposed in plaintext through `GetWebhookAsync` or `ListWebhooksAsync` unless the authenticated caller possesses an administrative scope. Update operations accept a new secret in plaintext, which is then hashed before storage.
- **Thread Safety**: The controller itself is stateless and relies on an injected backing store. Thread safety depends on the underlying storage implementation. Concurrent updates to the same webhook are subject to the last-write-wins behavior of the persistence layer unless an optimistic concurrency mechanism is employed externally.
- **URL Validation**: Only absolute HTTPS URLs are accepted. URLs with HTTP schemes, relative paths, or malformed structures cause `RegisterWebhookAsync` and `UpdateWebhookAsync` to throw `ArgumentException` before any persistence occurs.
