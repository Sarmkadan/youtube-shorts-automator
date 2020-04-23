# TimeoutUtility

The `TimeoutUtility` class provides a set of static helper methods for managing timeouts, deadlines, and task execution constraints within the `youtube-shorts-automator` application. It abstracts common time-based operations to facilitate robust handling of asynchronous workflows, cancellation token generation, and backoff calculations.

## API

*   **`IsExpired` (bool)**: Determines if a given deadline has passed.
*   **`GetTimeRemaining` (TimeSpan)**: Calculates the remaining duration until a specified deadline.
*   **`GetDeadline` (DateTime)**: Calculates a future deadline based on a start time and a duration. Provides multiple overloads to handle varying time sources.
*   **`CreateCancellationToken` (CancellationToken)**: Generates a `CancellationToken` that automatically triggers after the specified timeout duration.
*   **`ExecuteWithTimeoutAsync<T>` (Task<T?>)**: Executes an asynchronous operation wrapped in a timeout constraint. Returns the result if completed within the time limit.
*   **`ExecuteWithTimeoutAsync` (Task)**: Executes a task-based operation asynchronously with a specified timeout constraint.
*   **`ExecuteWithTimeout<T>` (T?)**: Executes a synchronous operation with a blocking timeout constraint.
*   **`IsWithinTimeWindow` (bool)**: Validates if a specific point in time falls within a defined duration window.
*   **`CalculateBackoffDelay` (TimeSpan)**: Computes a standard backoff delay, typically used for retry policies.
*   **`CalculateBackoffDelayWithJitter` (TimeSpan)**: Computes a backoff delay, incorporating randomness (jitter) to distribute load during retry attempts.

## Usage

### Asynchronous Execution with Timeout

```csharp
public async Task<string> ProcessVideoAsync(string videoId, CancellationToken ct)
{
    // Execute a task with a 30-second timeout
    var result = await TimeoutUtility.ExecuteWithTimeoutAsync(
        () => _videoService.DownloadAsync(videoId, ct),
        TimeSpan.FromSeconds(30)
    );

    return result ?? "Download timed out";
}
```

### Calculating Backoff with Jitter for Retries

```csharp
public TimeSpan GetRetryDelay(int attempt)
{
    // Base exponential backoff, then apply jitter to prevent thundering herd
    TimeSpan baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
    return TimeoutUtility.CalculateBackoffDelayWithJitter(baseDelay);
}
```

## Notes

*   **Thread Safety**: All members of `TimeoutUtility` are thread-safe, as they are stateless utility methods. They do not maintain internal state between calls.
*   **Time Sources**: When calculating deadlines or checking expiration, ensure consistent time sources (e.g., `DateTime.UtcNow`) are used across the application to avoid discrepancies.
*   **Exceptions**: `ExecuteWithTimeoutAsync` and `ExecuteWithTimeout` methods may throw a `TimeoutException` or propagate exceptions from the inner operation if the time limit is exceeded. Always wrap calls in appropriate `try-catch` blocks to handle these scenarios gracefully.
*   **Resource Management**: If `CreateCancellationToken` instantiates a `CancellationTokenSource`, ensure it is properly disposed of if not managed automatically to prevent resource leaks.
