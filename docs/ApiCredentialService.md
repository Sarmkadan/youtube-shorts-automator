# ApiCredentialService

The `ApiCredentialService` provides functionality for managing API credentials used to authenticate requests against external services, such as YouTube. It handles the lifecycle of these credentials, including creation, retrieval, updates, and token management operations such as validation, refreshing access tokens, and revocation.

## API

### GetActiveCredentialAsync
Retrieves the currently active credential for the authenticated user.
*   **Returns:** A `Task<ApiCredential?>` containing the active credential if found, otherwise `null`.

### CreateCredentialAsync
Persists a new credential record to the underlying storage.
*   **Parameters:** `ApiCredential credential` - The credential data to create.
*   **Returns:** A `Task<ApiCredential>` containing the created credential record.

### UpdateCredentialAsync
Updates an existing credential record.
*   **Parameters:** `ApiCredential credential` - The updated credential data.
*   **Returns:** A `Task<ApiCredential>` containing the updated credential record.

### RefreshAccessTokenAsync
Attempts to perform an OAuth2 refresh flow to obtain a new access token for the credential.
*   **Returns:** A `Task<bool>` that returns `true` if the token was refreshed successfully, otherwise `false`.

### ValidateCredentialAsync
Checks if the credential is currently valid, often by verifying token expiration or performing a lightweight call to the provider.
*   **Returns:** A `Task<bool>` that returns `true` if the credential is valid, otherwise `false`.

### RevokeCredentialAsync
Invalidates the access token with the provider and removes the credential record from storage.
*   **Returns:** A `Task<bool>` that returns `true` if revocation was successful, otherwise `false`.

### GetByIdAsync
Retrieves a specific credential record by its unique identifier.
*   **Parameters:** `string id` - The unique identifier of the credential.
*   **Returns:** A `Task<ApiCredential?>` containing the credential if found, otherwise `null`.

### GetUserCredentialsAsync
Retrieves a list of all credentials associated with the current user.
*   **Returns:** A `Task<List<ApiCredential>>` containing the collection of credentials.

## Usage

**Example: Validating and using an active credential**

```csharp
var credentialService = serviceProvider.GetRequiredService<ApiCredentialService>();

var activeCredential = await credentialService.GetActiveCredentialAsync();

if (activeCredential != null)
{
    bool isValid = await credentialService.ValidateCredentialAsync();
    if (!isValid)
    {
        await credentialService.RefreshAccessTokenAsync();
    }
    
    // Proceed with authorized API calls
}
```

**Example: Creating a new credential**

```csharp
var newCredential = new ApiCredential { ... };
var credentialService = serviceProvider.GetRequiredService<ApiCredentialService>();

try
{
    var createdCredential = await credentialService.CreateCredentialAsync(newCredential);
    // Handle successful creation
}
catch (Exception ex)
{
    // Handle persistence or validation errors
}
```

## Notes

*   **Async Operations:** All methods are asynchronous and are designed to perform I/O operations against a database or external API. Ensure proper `await` usage to avoid blocking the calling thread.
*   **Thread Safety:** The service implementation is intended to be thread-safe for concurrent operations, provided that the underlying database context or HTTP client supports concurrent access.
*   **Error Handling:** Methods involving network requests (e.g., `RefreshAccessTokenAsync`, `RevokeCredentialAsync`) may throw exceptions if the remote service is unavailable or returns an error response. Implement appropriate try-catch blocks to handle these transient failures.
*   **Nullability:** `GetActiveCredentialAsync` and `GetByIdAsync` return nullable types (`ApiCredential?`). Consumers must perform null checks before attempting to access properties of the returned object to prevent `NullReferenceException`.
