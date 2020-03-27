# YouTubeChannel

The `YouTubeChannel` class serves as the primary domain entity representing a connected YouTube channel within the `youtube-shorts-automator` system. It encapsulates essential channel metadata, authentication credentials required for API interactions, statistical data, and navigation properties to associated short-form video content. This type acts as the central record for managing channel state, token lifecycle, and synchronization history.

## API

### Properties

#### `public int Id`
The primary key identifier for the channel record within the local database.
*   **Purpose**: Uniquely identifies the entity in the persistence layer.
*   **Return Value**: An integer value.
*   **Throws**: None.

#### `public string ChannelId`
The unique alphanumeric identifier assigned by YouTube to the channel (e.g., `UC...`).
*   **Purpose**: Used to correlate the local record with the remote YouTube API resource.
*   **Return Value**: A string containing the YouTube Channel ID.
*   **Throws**: None.

#### `public string ChannelName`
The display name of the YouTube channel.
*   **Purpose**: Provides a human-readable label for the channel.
*   **Return Value**: A string representing the channel title.
*   **Throws**: None.

#### `public string Description`
The textual description or "About" section of the channel.
*   **Purpose**: Stores metadata regarding the channel's content focus.
*   **Return Value**: A string containing the description text; may be empty.
*   **Throws**: None.

#### `public string AccessToken`
The current OAuth 2.0 access token used to authorize API requests on behalf of the channel.
*   **Purpose**: Facilitates authenticated communication with the YouTube Data API.
*   **Return Value**: A string containing the bearer token.
*   **Throws**: None.
*   **Note**: This value is sensitive and should be handled securely.

#### `public string RefreshToken`
The OAuth 2.0 refresh token used to obtain new access tokens when the current one expires.
*   **Purpose**: Enables long-term authentication without requiring user re-login.
*   **Return Value**: A string containing the refresh token.
*   **Throws**: None.
*   **Note**: This value is highly sensitive and must be stored securely.

#### `public DateTime TokenExpiresAt`
The precise date and time when the current `AccessToken` becomes invalid.
*   **Purpose**: Determines when a token refresh operation is required.
*   **Return Value**: A `DateTime` value (typically in UTC).
*   **Throws**: None.

#### `public long SubscriberCount`
The total number of subscribers to the channel.
*   **Purpose**: Tracks channel growth and metrics.
*   **Return Value**: A 64-bit integer.
*   **Throws**: None.

#### `public long ViewCount`
The cumulative number of views across all videos on the channel.
*   **Purpose**: Aggregates total viewership statistics.
*   **Return Value**: A 64-bit integer.
*   **Throws**: None.

#### `public long VideoCount`
The total number of videos uploaded to the channel.
*   **Purpose**: Tracks content volume.
*   **Return Value**: A 64-bit integer.
*   **Throws**: None.

#### `public string ProfileImageUrl`
The URL pointing to the channel's profile picture or avatar.
*   **Purpose**: Provides a visual representation of the channel.
*   **Return Value**: A string containing a valid HTTP/HTTPS URL.
*   **Throws**: None.

#### `public bool IsVerified`
Indicates whether the channel has an official verification badge from YouTube.
*   **Purpose**: Denotes the authenticity or official status of the channel.
*   **Return Value**: `true` if verified; otherwise, `false`.
*   **Throws**: None.

#### `public bool IsActive`
Indicates whether the automator should currently process tasks for this channel.
*   **Purpose**: Acts as a toggle to enable or disable automation logic for specific channels.
*   **Return Value**: `true` if active; otherwise, `false`.
*   **Throws**: None.

#### `public string DefaultLanguage`
The ISO language code representing the primary language of the channel's content.
*   **Purpose**: Assists in localization and content targeting logic.
*   **Return Value**: A string (e.g., "en", "es").
*   **Throws**: None.

#### `public DateTime CreatedAt`
The timestamp indicating when this record was initially created in the system.
*   **Purpose**: Audit trail for record creation.
*   **Return Value**: A `DateTime` value.
*   **Throws**: None.

#### `public DateTime UpdatedAt`
The timestamp indicating the last time any property on this record was modified.
*   **Purpose**: Audit trail for data modifications.
*   **Return Value**: A `DateTime` value.
*   **Throws**: None.

#### `public DateTime? LastSyncAt`
The timestamp of the most recent successful synchronization with the YouTube API.
*   **Purpose**: Tracks data freshness; `null` if no sync has occurred yet.
*   **Return Value**: A nullable `DateTime`.
*   **Throws**: None.

#### `public ICollection<VideoShort> VideoShorts`
A collection of `VideoShort` entities associated with this channel.
*   **Purpose**: Navigation property for accessing shorts uploaded to or managed for this channel.
*   **Return Value**: A collection of `VideoShort` objects.
*   **Throws**: None.
*   **Note**: Depending on the ORM configuration, accessing this may trigger lazy loading.

#### `public bool IsTokenExpired`
A computed property indicating the validity status of the current access token.
*   **Purpose**: Provides a quick boolean check to determine if `UpdateToken` needs to be invoked.
*   **Return Value**: `true` if the current time is past `TokenExpiresAt`; otherwise, `false`.
*   **Throws**: None.

### Methods

#### `public void UpdateToken`
Refreshes the OAuth credentials by exchanging the `RefreshToken` for a new `AccessToken` and updates the expiration time.
*   **Purpose**: Maintains valid authentication for API calls when the current token expires.
*   **Parameters**: None.
*   **Return Value**: `void`. Updates internal properties (`AccessToken`, `RefreshToken`, `TokenExpiresAt`, `UpdatedAt`).
*   **Throws**:
    *   `InvalidOperationException`: If the `RefreshToken` is missing or invalid.
    *   `HttpRequestException`: If the network request to the OAuth provider fails.
    *   `AuthenticationException`: If the credentials are rejected by the authorization server.

## Usage

### Example 1: Checking Token Validity and Refreshing
This example demonstrates how to safely ensure valid credentials before making an API call by checking the `IsTokenExpired` property and invoking `UpdateToken` if necessary.

```csharp
public async Task EnsureChannelAuthenticatedAsync(YouTubeChannel channel)
{
    if (channel == null) throw new ArgumentNullException(nameof(channel));

    // Check if the current token is expired
    if (channel.IsTokenExpired)
    {
        Console.WriteLine($"Token expired for {channel.ChannelName}. Refreshing...");
        
        // UpdateToken handles the OAuth exchange and updates the entity properties
        try 
        {
            channel.UpdateToken();
            // Persist the updated tokens to the database immediately
            await _repository.SaveAsync(channel);
        }
        catch (AuthenticationException ex)
        {
            Console.Error.WriteLine($"Failed to refresh token for {channel.ChannelId}: {ex.Message}");
            channel.IsActive = false; // Disable channel on auth failure
            await _repository.SaveAsync(channel);
            throw;
        }
    }
    
    // Proceed with API calls using channel.AccessToken
}
```

### Example 2: Aggregating Channel Statistics
This example illustrates accessing statistical properties and the navigation collection to calculate aggregate metrics for active channels.

```csharp
public void GenerateChannelReport(IEnumerable<YouTubeChannel> channels)
{
    var activeChannels = channels.Where(c => c.IsActive);

    foreach (var channel in activeChannels)
    {
        Console.WriteLine($"Channel: {channel.ChannelName} (Verified: {channel.IsVerified})");
        Console.WriteLine($"Subscribers: {channel.SubscriberCount:N0}");
        Console.WriteLine($"Total Views: {channel.ViewCount:N0}");
        Console.WriteLine($"Video Count: {channel.VideoCount}");
        
        // Accessing the navigation property
        var shortsCount = channel.VideoShorts?.Count ?? 0;
        Console.WriteLine($"Managed Shorts: {shortsCount}");
        
        if (channel.LastSyncAt.HasValue)
        {
            Console.WriteLine($"Last Synced: {channel.LastSyncAt.Value:yyyy-MM-dd HH:mm:ss}");
        }
        else
        {
            Console.WriteLine("Status: Never synced");
        }
        
        Console.WriteLine(new string('-', 40));
    }
}
```

## Notes

*   **Thread Safety**: The `YouTubeChannel` class is not thread-safe. Properties such as `AccessToken`, `RefreshToken`, and `TokenExpiresAt` are mutable. If an instance is shared across multiple threads, external synchronization (e.g., `lock` statements) is required when invoking `UpdateToken` or modifying properties to prevent race conditions during token refresh operations.
*   **Token Lifecycle**: The `IsTokenExpired` property relies on the system clock compared against `TokenExpiresAt`. Clock skew between the server and the OAuth provider may cause premature expiration checks or brief periods of invalid token usage. It is recommended to implement a small buffer (e.g., treating tokens as expired 5 minutes before `TokenExpiresAt`) in calling logic, though this class strictly reports the raw time comparison.
*   **Lazy Loading**: The `VideoShorts` collection is an `ICollection<T>`. If backed by an ORM like Entity Framework, accessing this property may trigger a database query if lazy loading is enabled. Ensure the data context is still active when accessing this property to avoid runtime disposal exceptions.
*   **Sensitive Data**: Instances of this class contain raw OAuth tokens (`AccessToken`, `RefreshToken`). Care must be taken to avoid logging these properties or serializing them to insecure storage mediums.
*   **Nullability**: While most string properties are expected to be populated, `Description`, `DefaultLanguage`, and `ProfileImageUrl` may potentially be empty strings or null depending on the source data from YouTube. `LastSyncAt` is explicitly nullable to represent channels that have been added but not yet synchronized.
