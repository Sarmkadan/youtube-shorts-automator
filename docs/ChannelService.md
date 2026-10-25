# ChannelService

`ChannelService` checks and updates the authentication state and activity status of a `YouTubeChannel`, and produces a human-readable status summary. It is a sealed class in the console pipeline (`src/Services/ChannelService.cs`, namespace `YouTubeShortAutomator.Services`) and has no interface. It does not call the YouTube API: every method works on the `YouTubeChannel` instance it is given, so the token and credential checks reflect the locally stored state.

## API

### public ChannelService(ILogger\<ChannelService\> logger)
Initializes a new instance of the service. The logger is used to write informational and warning messages from `UpdateChannelStatus` and `ValidateChannelCredentials`. Throws `ArgumentNullException` if `logger` is `null`.

### public bool IsChannelTokenValid(YouTubeChannel channel)
Returns `true` when the channel's access token has not expired (`YouTubeChannel.IsTokenExpired()` is `false`) and the channel is active (`IsActive` is `true`); otherwise `false`. Returns `false` when `channel` is `null` instead of throwing. This method only reads state and does not log.

### public bool NeedsTokenRefresh(YouTubeChannel channel)
Returns the result of `YouTubeChannel.NeedsTokenRefresh()`, which indicates the token should be refreshed proactively. The service does not perform the refresh itself. Throws `ArgumentNullException` if `channel` is `null`.

### public void UpdateChannelStatus(YouTubeChannel channel, bool isActive)
Activates the channel when `isActive` is `true` (`Activate()`), or deactivates it otherwise (`Deactivate()`). Logs an information message with the channel name and the new status. Throws `ArgumentNullException` if `channel` is `null`. The change is applied to the in-memory instance only; persisting it is the caller's responsibility.

### public string GetChannelStatusSummary(YouTubeChannel channel)
Returns a multi-line, human-readable summary with one `Label: value` line per field, each terminated by a newline:

| Line | Source |
|------|--------|
| `Channel` | `ChannelName` |
| `Status` | `Active` or `Inactive`, from `IsActive` |
| `Verified` | `Yes` or `No`, from `IsVerified` |
| `Subscribers` | `SubscriberCount`, formatted with `N0` (thousands separators) |
| `Views` | `ViewCount`, formatted with `N0` |
| `Videos` | `VideoCount` |
| `Token Valid` | `Expired` or `Valid`, from `IsTokenExpired()` |
| `Last Synced` | `LastSyncAt`, formatted with the `F` (full date/time) pattern |

Throws `ArgumentNullException` if `channel` is `null`.

### public bool ValidateChannelCredentials(YouTubeChannel channel)
Checks the locally stored credentials in this order and returns `false` at the first failure:

1. `channel.IsValid()`, the validation method defined on `YouTubeChannel`. Logs a warning with the channel ID.
2. `AccessToken` is not empty or whitespace. Logs a warning.
3. `RefreshToken` is not empty or whitespace. Logs a warning.

Returns `true` when all checks pass. The method does not contact Google or verify that the tokens are accepted by the API. Throws `ArgumentNullException` if `channel` is `null`.

## Usage

```csharp
using Microsoft.Extensions.Logging;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

ILogger<ChannelService> logger = loggerFactory.CreateLogger<ChannelService>();
var channelService = new ChannelService(logger);

YouTubeChannel channel = await LoadChannelAsync();

if (!channelService.ValidateChannelCredentials(channel))
{
    channelService.UpdateChannelStatus(channel, isActive: false);
    Console.WriteLine("Credentials are incomplete. Channel deactivated.");
    return;
}

if (channelService.NeedsTokenRefresh(channel))
{
    Console.WriteLine("Access token should be refreshed before the next upload.");
}

Console.WriteLine(channelService.GetChannelStatusSummary(channel));
```

```csharp
// Check whether the channel can be used for uploads right now
if (!channelService.IsChannelTokenValid(channel))
{
    Console.WriteLine("Channel is not usable: token expired or channel inactive.");
}
```

## Notes

- **Null handling:** `IsChannelTokenValid` returns `false` for a `null` channel. All other public methods throw `ArgumentNullException` with the message `YouTube channel cannot be null.`
- **Local checks only:** `ValidateChannelCredentials` confirms that the credentials are present and pass entity validation. It does not detect revoked or rejected tokens, because it never contacts the YouTube API.
- **Activity and token state are separate:** `IsChannelTokenValid` requires both an unexpired token and an active channel. `GetChannelStatusSummary` reports `Token Valid` from the token expiry alone, so a summary can show `Token Valid: Valid` for an inactive channel.
- **Last synced:** `LastSyncAt` is nullable. When the channel has never been synced, the `Last Synced` line is empty rather than showing a date.
- **Persistence:** `UpdateChannelStatus` changes the instance only. Save the channel through the repository layer if the new status must survive a restart.
- **Dependency injection:** `ChannelService` is not registered in the console app's `ServiceCollection` in `src/Program.cs`. Create it directly with a logger, as shown in the usage examples.
- **Thread safety:** The class holds no mutable state of its own, but it mutates the `YouTubeChannel` passed to `UpdateChannelStatus`. Callers must synchronize access to the same channel instance.
