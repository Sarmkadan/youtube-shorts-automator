# ChannelService

The `ChannelService` class manages the authentication state and status of a YouTube channel within the shorts automator workflow. It provides properties to inspect the current token validity and refresh requirement, methods to update the internal status, retrieve a human-readable summary, and validate stored credentials against the external service.

## API

### `public ChannelService()`

Initializes a new instance of the `ChannelService` class. No parameters are required. The constructor does not throw exceptions.

### `public bool IsChannelTokenValid { get; }`

Gets a value indicating whether the current channel token is considered valid. Returns `true` if the token is valid; otherwise `false`. This property does not throw exceptions.

### `public bool NeedsTokenRefresh { get; }`

Gets a value indicating whether the channel token requires a refresh. Returns `true` if a refresh is needed; otherwise `false`. This property does not throw exceptions.

### `public void UpdateChannelStatus()`

Updates the internal channel status by re-evaluating the token state and any cached data. This method does not throw exceptions.

### `public string GetChannelStatusSummary()`

Returns a human-readable summary of the current channel status. The returned string contains information such as token validity and refresh state. This method does not throw exceptions.

### `public bool ValidateChannelCredentials()`

Validates the stored channel credentials by performing a check against the external service. Returns `true` if the credentials are valid; otherwise `false`. This method does not throw exceptions.

## Usage

### Example 1: Basic status inspection

```csharp
var service = new ChannelService();

// Check initial token state
if (!service.IsChannelTokenValid)
{
    Console.WriteLine("Token is not valid. Updating status...");
    service.UpdateChannelStatus();
}

// Display a summary
string summary = service.GetChannelStatusSummary();
Console.WriteLine(summary);
```

### Example 2: Handling token refresh

```csharp
var service = new ChannelService();

if (service.NeedsTokenRefresh)
{
    Console.WriteLine("Token refresh required. Validating credentials...");
    bool valid = service.ValidateChannelCredentials();

    if (valid)
    {
        Console.WriteLine("Credentials are valid. Updating status...");
        service.UpdateChannelStatus();
    }
    else
    {
        Console.WriteLine("Credentials are invalid. Manual intervention needed.");
    }
}

string finalSummary = service.GetChannelStatusSummary();
Console.WriteLine(finalSummary);
```

## Notes

- **Edge cases**: If the service has not been configured with any credentials, `IsChannelTokenValid` and `NeedsTokenRefresh` may both return `false` until `ValidateChannelCredentials` or `UpdateChannelStatus` is called. The `GetChannelStatusSummary` method will still return a meaningful string even when no credentials are present.
- **Thread safety**: This class is not thread-safe. Properties and methods are intended to be called from a single thread. If concurrent access is required, external synchronization (e.g., a lock) must be used. The values of `IsChannelTokenValid` and `NeedsTokenRefresh` can change after calls to `UpdateChannelStatus` or `ValidateChannelCredentials`.
