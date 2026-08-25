## FileSystemUtility

`FileSystemUtility` provides static methods for common file system operations, such as ensuring directories exist, validating video files, reading and writing files, and managing directories.

### Usage Example

```csharp
// Ensure the output directory exists
await FileSystemUtility.EnsureDirectoryExistsAsync("./output");

// Check if a file is a valid video file
bool isValid = FileSystemUtility.IsValidVideoFile("./video.mp4");

// Read a text file
string content = await FileSystemUtility.ReadFileAsStringAsync("./notes.txt");

// Write the content to a new file in the output directory
await FileSystemUtility.WriteFileAsync("./output/notes.txt", content);

// Get all text files in the output directory
string[] textFiles = FileSystemUtility.GetFilesWithExtension("./output", "txt");

// Get the total size of the output directory
long directorySize = FileSystemUtility.GetDirectorySizeBytes("./output");
```

## TimeoutUtility

`TimeoutUtility` provides static helper methods for managing timeouts, deadlines, and backoff strategies in asynchronous and synchronous operations. It simplifies the creation of cancellation tokens, checking expiration status, calculating remaining time, and implementing retry logic with exponential backoff and jitter.

### Usage Example

```csharp
// Define a deadline 30 seconds from now
DateTime deadline = TimeoutUtility.GetDeadline(TimeSpan.FromSeconds(30));

// Check if the deadline has passed
if (!TimeoutUtility.IsExpired(deadline))
{
    // Create a cancellation token tied to the deadline
    using var cts = TimeoutUtility.CreateCancellationToken(deadline);

    // Execute an async operation with a timeout
    var result = await TimeoutUtility.ExecuteWithTimeoutAsync(
        async ct => await Task.Delay(1000, ct),
        deadline,
        cts.Token);

    // Calculate remaining time
    TimeSpan remaining = TimeoutUtility.GetTimeRemaining(deadline);
}

// Implement retry logic with exponential backoff and jitter
TimeSpan delay = TimeoutUtility.CalculateBackoffDelayWithJitter(attempt: 3, baseDelay: TimeSpan.FromSeconds(1));
```

## EncodingUtility

`EncodingUtility` provides static helpers for encoding, decoding, and hashing data, supporting Base64, URL, and HTML transformations alongside SHA-256 and MD5 hashes with optional verification. It also generates cryptographically secure random strings, hex strings, and GUIDs, and includes helpers for parsing and building URL query strings.

### Usage Example

```csharp
// Encode and decode Base64 strings
string encoded = EncodingUtility.EncodeBase64("Hello, world!");
string decoded = EncodingUtility.DecodeBase64(encoded);

// Compute hashes
string sha256 = EncodingUtility.ComputeSha256Hash("sensitive data");
string md5 = EncodingUtility.ComputeMd5Hash("checksum payload");

// Verify input against a previously computed SHA-256 hash
bool isValid = EncodingUtility.VerifyHash("sensitive data", sha256);

// URL encode and decode
string urlEncoded = EncodingUtility.UrlEncode("search term & more");
string urlDecoded = EncodingUtility.UrlDecode(urlEncoded);

// Build and parse query strings
var queryParams = new Dictionary<string, string>
{
    ["page"] = "2",
    ["sort"] = "desc"
};
string query = EncodingUtility.BuildQueryString(queryParams);
Dictionary<string, string> parsed = EncodingUtility.ParseQueryString(query);

// Securely generate random values
string token = EncodingUtility.GenerateRandomString(32);
string hexToken = EncodingUtility.GenerateRandomHexString(16);
Guid secureId = EncodingUtility.GenerateSecureGuid();

// HTML encode and decode
string safeHtml = EncodingUtility.HtmlEncode("<script>alert('x')</script>");
string rawHtml = EncodingUtility.HtmlDecode(safeHtml);
```
