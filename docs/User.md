# User

The `User` type represents a registered account within the YouTube Shorts Automator system, serving as the primary aggregate root for managing user-specific data, including storage quotas, subscription status, and associated YouTube video content.

## API

### Properties
* `Id` (`Guid`): The unique identifier for the user.
* `Email` (`string`): The registered email address of the user.
* `DisplayName` (`string`): The display name associated with the user's profile.
* `ChannelId` (`string`): The identifier for the user's YouTube channel.
* `EmailConfirmed` (`bool`): Indicates whether the user has verified their email address.
* `StorageQuotaBytes` (`long`): The maximum amount of storage allocated to the user in bytes.
* `StorageUsedBytes` (`long`): The amount of storage currently consumed by the user in bytes.
* `SubscriptionTier` (`UserSubscriptionTier`): The current subscription level of the user.
* `SubscriptionExpiresAt` (`DateTime`): The date and time when the user's current subscription expires.
* `CreatedAt` (`DateTime`): The date and time when the user account was created.
* `LastActivityAt` (`DateTime?`): The date and time of the user's most recent activity, if available.
* `IsActive` (`bool`): Indicates whether the user account is currently active.
* `Videos` (`List<Video>`): A list of videos associated with the user account.
* `ApiCredential?` (`ApiCredential?`): The API credentials associated with the user, if configured.
* `UploadSchedule?` (`UploadSchedule?`): The upload schedule configured for the user, if applicable.

### Methods
* `Validate()`: Returns a tuple containing a `bool` indicating if the user state is valid and a `List<string>` containing validation error messages.
* `IsStorageFull()`: Returns `true` if `StorageUsedBytes` has reached or exceeded `StorageQuotaBytes`; otherwise `false`.
* `AddStorageUsage(long bytes)`: Updates `StorageUsedBytes` by adding the specified number of bytes.
* `RemoveStorageUsage(long bytes)`: Updates `StorageUsedBytes` by subtracting the specified number of bytes.
* `UpdateLastActivity()`: Updates `LastActivityAt` to the current date and time.

## Usage

### Validating a User and Checking Storage
```csharp
var user = userRepository.GetById(userId);
var (isValid, errors) = user.Validate();

if (!isValid)
{
    Console.WriteLine($"User validation failed: {string.Join(", ", errors)}");
}
else if (user.IsStorageFull())
{
    Console.WriteLine("User storage is full. Please upgrade subscription.");
}
```

### Updating User Activity and Storage
```csharp
// Update last seen activity
user.UpdateLastActivity();

// Log storage usage for a new video upload
long fileSize = 1024 * 1024 * 50; // 50MB
if (!user.IsStorageFull())
{
    user.AddStorageUsage(fileSize);
    userRepository.Update(user);
}
```

## Notes

* **Data Consistency:** The `StorageUsedBytes` property should always be modified using `AddStorageUsage` or `RemoveStorageUsage` to ensure internal state remains consistent. Direct modification is discouraged.
* **Thread Safety:** The `User` class is not inherently thread-safe. Concurrent modifications to `Videos`, `StorageUsedBytes`, or other mutable properties should be synchronized externally.
* **Nullability:** `ApiCredential`, `UploadSchedule`, and `LastActivityAt` are nullable. Always check for `null` before accessing members of these properties to avoid `NullReferenceException`.
* **Validation:** The `Validate` method should be called before persisting any changes to the `User` object to ensure the data adheres to business rules.
