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
