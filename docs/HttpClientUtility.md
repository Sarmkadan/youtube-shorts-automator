# HttpClientUtility

`HttpClientUtility` provides static helper methods to streamline the creation, configuration, and execution of HTTP requests within the `youtube-shorts-automator` project. It reduces boilerplate code for common operations such as initializing pre-configured HTTP clients, applying authentication headers, performing JSON-based request-response interactions, and evaluating HTTP response status codes.

## API

*   **`CreateConfiguredClient()`**
    *   **Purpose**: Creates and returns a new `HttpClient` instance pre-configured with project-specific default settings, such as timeouts and default headers.
    *   **Parameters**: None.
    *   **Return Value**: `HttpClient` - The configured HTTP client.
    *   **Throws**: N/A.

*   **`AddAuthorizationHeader(HttpRequestMessage request, string scheme, string parameter)`**
    *   **Purpose**: Configures the specified `HttpRequestMessage` with a custom authorization header.
    *   **Parameters**: `request` (the request to modify), `scheme` (the auth scheme, e.g., "Basic"), `parameter` (the auth parameter).
    *   **Return Value**: `void`.
    *   **Throws**: `ArgumentNullException` if `request` is null.

*   **`AddApiKeyHeader(HttpRequestMessage request, string apiKey)`**
    *   **Purpose**: Configures the specified `HttpRequestMessage` with an API key header.
    *   **Parameters**: `request` (the request to modify), `apiKey` (the API key value).
    *   **Return Value**: `void`.
    *   **Throws**: `ArgumentNullException` if `request` is null.

*   **`AddBearerToken(HttpRequestMessage request, string token)`**
    *   **Purpose**: Configures the specified `HttpRequestMessage` with a Bearer authentication token.
    *   **Parameters**: `request` (the request to modify), `token` (the JWT or token string).
    *   **Return Value**: `void`.
    *   **Throws**: `ArgumentNullException` if `request` is null.

*   **`GetAsJsonAsync<T>(HttpClient client, string url)`**
    *   **Purpose**: Sends an HTTP GET request to the specified URL and deserializes the JSON response into type `T`.
    *   **Parameters**: `client` (the `HttpClient` to use), `url` (the target endpoint).
    *   **Return Value**: `Task<T?>` - The deserialized object or null.
    *   **Throws**: `HttpRequestException` on network or HTTP errors, `JsonException` on deserialization failures.

*   **`PostAsJsonAsync<T>(HttpClient client, string url, T data)`**
    *   **Purpose**: Serializes the provided `data` to JSON and sends it via an HTTP POST request.
    *   **Parameters**: `client`, `url`, `data` (the payload).
    *   **Return Value**: `Task<HttpResponseMessage>` - The response message.
    *   **Throws**: `HttpRequestException` on network or HTTP errors.

*   **`GetAsStringAsync(HttpClient client, string url)`**
    *   **Purpose**: Sends an HTTP GET request and returns the response content as a string.
    *   **Parameters**: `client`, `url`.
    *   **Return Value**: `Task<string>` - The response content.
    *   **Throws**: `HttpRequestException` on network or HTTP errors.

*   **`PutAsJsonAsync<T>(HttpClient client, string url, T data)`**
    *   **Purpose**: Serializes the provided `data` to JSON and sends it via an HTTP PUT request.
    *   **Parameters**: `client`, `url`, `data`.
    *   **Return Value**: `Task<HttpResponseMessage>` - The response message.
    *   **Throws**: `HttpRequestException` on network or HTTP errors.

*   **`DeleteAsync(HttpClient client, string url)`**
    *   **Purpose**: Sends an HTTP DELETE request to the specified URL.
    *   **Parameters**: `client`, `url`.
    *   **Return Value**: `Task<HttpResponseMessage>` - The response message.
    *   **Throws**: `HttpRequestException` on network or HTTP errors.

*   **`IsSuccessStatusCode(HttpResponseMessage response)`**
    *   **Purpose**: Evaluates whether the response indicates success (status code in the 200-299 range).
    *   **Parameters**: `response`.
    *   **Return Value**: `bool`.
    *   **Throws**: N/A.

*   **`IsClientError(HttpResponseMessage response)`**
    *   **Purpose**: Evaluates whether the response indicates a client-side error (status code in the 400-499 range).
    *   **Parameters**: `response`.
    *   **Return Value**: `bool`.
    *   **Throws**: N/A.

*   **`IsServerError(HttpResponseMessage response)`**
    *   **Purpose**: Evaluates whether the response indicates a server-side error (status code in the 500-599 range).
    *   **Parameters**: `response`.
    *   **Return Value**: `bool`.
    *   **Throws**: N/A.

*   **`GetStatusCodeDescription(HttpResponseMessage response)`**
    *   **Purpose**: Returns a descriptive string representation of the HTTP status code.
    *   **Parameters**: `response`.
    *   **Return Value**: `string`.
    *   **Throws**: N/A.

## Usage

```csharp
// Example 1: Fetching and deserializing JSON
using var client = HttpClientUtility.CreateConfiguredClient();
var data = await HttpClientUtility.GetAsJsonAsync<MyModel>(client, "https://api.example.com/data");

if (data != null)
{
    Console.WriteLine($"Received: {data.Id}");
}
```

```csharp
// Example 2: Sending a POST request with authentication
using var client = HttpClientUtility.CreateConfiguredClient();
using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/submit");
HttpClientUtility.AddBearerToken(request, "my_secure_token");

var response = await HttpClientUtility.PostAsJsonAsync(client, "https://api.example.com/submit", payload);

if (HttpClientUtility.IsSuccessStatusCode(response))
{
    Console.WriteLine("Request successful");
}
```

## Notes

*   **HttpClient Lifecycle**: While `CreateConfiguredClient` facilitates creating clients, it is recommended to reuse `HttpClient` instances where appropriate to prevent socket exhaustion, rather than creating a new client for every individual request.
*   **Thread Safety**: The `HttpClient` instance itself is thread-safe; however, configuring headers on an `HttpRequestMessage` prior to sending it is not thread-safe if the same request instance is accessed concurrently.
*   **Exception Handling**: The `Async` methods wrapping HTTP calls may throw `HttpRequestException` if the request fails due to network issues or invalid responses. Callers should wrap these operations in appropriate try-catch blocks to handle connectivity or service-level errors.
