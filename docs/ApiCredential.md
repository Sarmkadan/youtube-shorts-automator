# ApiCredential

The `ApiCredential` class is an entity designed to manage authentication credentials for external services integrated with the application, such as YouTube. It maintains token lifecycle data, including access and refresh tokens, associated expiration timestamps, and the current status of the credential, ensuring secure and tracked interactions with third-party APIs on behalf of a specific user.

## API

### Properties

*   **`Id`** (`Guid`): The unique identifier for this credential record.
*   **`UserId`** (`Guid`): The unique identifier of the user to whom this credential belongs.
*   **`User`** (`User?`): The navigation property for the associated user account.
*   **`CredentialType`** (`ApiCredentialType`): The type or provider of the API credential (e.g., YouTube, Google).
*   **`ClientId`** (`string`): The client identifier provided by the API service.
*   **`ClientSecret`** (`string`): The client secret associated with the client identifier.
*   **`AccessToken`** (`string`): The current OAuth access token used for API requests.
*   **`RefreshToken`** (`string`): The token used to obtain a new access token when the current one expires.
*   **`AccessTokenExpiresAt`** (`DateTime`): The timestamp when the `AccessToken` becomes invalid.
*   **`RefreshTokenExpiresAt`** (`DateTime?`): The optional timestamp when the `RefreshToken` becomes invalid.
*   **`CreatedAt`** (`DateTime`): The timestamp when the record was created.
*   **`UpdatedAt`** (`DateTime?`): The timestamp when the record was last modified.
*   **`IsValid`** (`bool`): Indicates whether the credential is considered valid and active.
*   **`Scope`** (`string?`): The permission scopes granted by the credential.
*   **`Status`** (`CredentialStatus`): The current lifecycle status of the credential.
*   **`RefreshAttempts`** (`int`): The counter for attempts made to refresh the token.
*   **`IsAccessTokenExpired`** (`bool`): Returns `true` if `AccessTokenExpiresAt` is in the past.
*   **`IsRefreshTokenExpired`** (`bool`): Returns `true` if `RefreshTokenExpiresAt` is in the past.
*   **`NeedsRefresh`** (`bool`): Returns `true` if the access token is expired or approaching expiration and requires a refresh.

### Methods

*   **`UpdateAccessToken(string newAccessToken, DateTime newExpiresAt)`**: Updates the access token and its expiration timestamp, and sets the `UpdatedAt` property to the current time.

## Usage

### Checking Credential Status Before Request
```csharp
if (credential.NeedsRefresh)
{
    // Logic to perform token refresh flow
    var refreshedData = await _oauthService.RefreshTokenAsync(credential.RefreshToken);
    credential.UpdateAccessToken(refreshedData.AccessToken, refreshedData.ExpiresAt);
    await _dbContext.SaveChangesAsync();
}

// Proceed with API call using credential.AccessToken
```

### Initializing a Credential Record
```csharp
var newCredential = new ApiCredential
{
    Id = Guid.NewGuid(),
    UserId = currentUser.Id,
    CredentialType = ApiCredentialType.YouTube,
    ClientId = config.ClientId,
    ClientSecret = config.ClientSecret,
    AccessToken = tokenResponse.AccessToken,
    RefreshToken = tokenResponse.RefreshToken,
    AccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
    CreatedAt = DateTime.UtcNow,
    Status = CredentialStatus.Active
};
```

## Notes

*   **Thread Safety**: This class is a standard data entity and is not inherently thread-safe. Concurrent access or modifications to an instance must be synchronized by the calling code, particularly when updating token information.
*   **Persistence**: `UpdateAccessToken` modifies the state of the object in memory. Persistence to the underlying database must be explicitly handled by calling the appropriate `DbContext` methods (e.g., `SaveChangesAsync`) following any method invocations that alter the object state.
*   **Nullability**: Ensure `RefreshToken` and `RefreshTokenExpiresAt` are handled according to the requirements of the specific OAuth provider, as some providers may not issue new refresh tokens or may issue tokens with indefinite lifespans.
