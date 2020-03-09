# ApiCredentialRepository

The `ApiCredentialRepository` serves as the data access layer for managing API credentials within the `youtube-shorts-automator` project. It provides asynchronous operations to retrieve, filter, and query `ApiCredential` entities based on user ownership, activation status, expiration state, credential type, and general status flags. This repository abstracts the underlying storage mechanism, ensuring that credential retrieval logic remains consistent and decoupled from business logic components.

## API

### `public ApiCredentialRepository`
Initializes a new instance of the `ApiCredentialRepository` class. This constructor typically injects necessary dependencies such as database contexts or configuration settings required to perform data operations.

### `public async Task<List<ApiCredential>> GetByUserIdAsync`
Retrieves a list of all API credentials associated with a specific user identifier.
*   **Parameters**: Accepts a user identifier (typically `Guid` or `string`, depending on the domain model definition) to filter results.
*   **Return Value**: Returns a `Task` resulting in a `List<ApiCredential>` containing all matching records. An empty list is returned if no credentials are found.
*   **Exceptions**: Throws a database-related exception if the underlying data store is unreachable or if the query fails due to connectivity issues.

### `public async Task<ApiCredential?> GetActiveCredentialAsync`
Fetches a single currently active API credential. This method is typically used to locate a valid credential ready for immediate use in external API calls.
*   **Parameters**: Takes no explicit parameters; filtering is based on internal status flags (e.g., `IsActive == true` and non-expired).
*   **Return Value**: Returns a `Task` resulting in an `ApiCredential` object if an active record exists, or `null` if no active credential is found.
*   **Exceptions**: May throw if multiple active credentials exist and the implementation does not support ambiguity, or if a data access error occurs.

### `public async Task<List<ApiCredential>> GetByStatusAsync`
Retrieves a list of API credentials filtered by a specific status enumeration or flag.
*   **Parameters**: Accepts a status value (e.g., `CredentialStatus.Revoked`, `CredentialStatus.Pending`) to filter the dataset.
*   **Return Value**: Returns a `Task` resulting in a `List<ApiCredential>` containing all credentials matching the specified status.
*   **Exceptions**: Throws if the provided status value is invalid or if a database error occurs during execution.

### `public async Task<List<ApiCredential>> GetExpiredCredentialsAsync`
Identifies and returns all API credentials that have passed their validity period. This is primarily used for maintenance tasks, such as triggering renewal workflows or notifying users.
*   **Parameters**: Takes no parameters; the query filters based on the `ExpirationDate` relative to the current system time.
*   **Return Value**: Returns a `Task` resulting in a `List<ApiCredential>` containing all expired entries.
*   **Exceptions**: Throws if the system clock is inaccessible or if a data retrieval error occurs.

### `public async Task<List<ApiCredential>> GetByTypeAsync`
Retrieves a list of API credentials filtered by their specific type (e.g., OAuth2, API Key, Service Account).
*   **Parameters**: Accepts a credential type identifier or enumeration value.
*   **Return Value**: Returns a `Task` resulting in a `List<ApiCredential>` containing all credentials of the specified type.
*   **Exceptions**: Throws if the type identifier is unrecognized or if a database error occurs.

## Usage

### Example 1: Retrieving Active Credentials for Automation
The following example demonstrates how to fetch the currently active credential to initialize an API client for uploading shorts.

```csharp
public class ShortsUploadService
{
    private readonly ApiCredentialRepository _credentialRepository;

    public ShortsUploadService(ApiCredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    public async Task InitializeClientAsync()
    {
        // Retrieve the single active credential
        var credential = await _credentialRepository.GetActiveCredentialAsync();

        if (credential == null)
        {
            throw new InvalidOperationException("No active API credential available for automation.");
        }

        // Use the credential to configure the external client
        var client = new YouTubeClient(credential.Token);
        await client.ConnectAsync();
    }
}
```

### Example 2: Auditing Expired Credentials by User
This example illustrates how to audit a specific user's account to identify expired credentials that require rotation or removal.

```csharp
public class CredentialAuditService
{
    private readonly ApiCredentialRepository _credentialRepository;

    public CredentialAuditService(ApiCredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    public async Task<List<ApiCredential>> GetUserExpiredCredentialsAsync(Guid userId)
    {
        // Fetch all credentials for the user
        var userCredentials = await _credentialRepository.GetByUserIdAsync(userId);
        
        // Filter locally or use specific repository methods if available
        // Here we assume a need to cross-reference with the global expired list
        var allExpired = await _credentialRepository.GetExpiredCredentialsAsync();

        return userCredentials
            .Join(allExpired, 
                user => user.Id, 
                expired => expired.Id, 
                (user, expired) => user)
            .ToList();
    }
}
```

## Notes

*   **Null Handling**: Methods returning a single entity (`GetActiveCredentialAsync`) explicitly return `null` when no match is found, rather than throwing an exception. Callers must perform null checks before accessing properties of the returned object.
*   **Empty Collections**: List-returning methods (`GetByUserIdAsync`, `GetByStatusAsync`, etc.) return an empty `List<ApiCredential>` rather than `null` when no records match the criteria. This prevents `NullReferenceException` during enumeration.
*   **Thread Safety**: As an asynchronous repository interacting with a database context, this class should be treated as stateless regarding request data. However, the underlying database context injected into the constructor may not be thread-safe. It is recommended to scope the `ApiCredentialRepository` instance per request or per unit of work to avoid concurrency conflicts within the data context.
*   **Date Sensitivity**: The `GetExpiredCredentialsAsync` method relies on the server's system clock at the time of query execution. Discrepancies between server time and database time zones may affect the accuracy of the "expired" determination.
