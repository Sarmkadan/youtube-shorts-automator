# ProcessingError

The `ProcessingError` class represents a failure or exceptional event encountered during the execution of a processing job within the `youtube-shorts-automator` system. It provides comprehensive metadata regarding the nature of the error, its severity, and its current resolution status, enabling robust error tracking, reporting, and automated retry management.

## API

*   **`Guid Id`** (Property)
    Unique identifier for the error record.
*   **`Guid JobId`** (Property)
    The unique identifier of the `ProcessingJob` that triggered this error.
*   **`ProcessingJob? Job`** (Property)
    The associated `ProcessingJob` instance. May be null if the job reference is not loaded or has been removed.
*   **`ProcessingErrorType ErrorType`** (Property)
    Categorization of the error (e.g., transient network failure, authentication issue, validation error).
*   **`string ErrorMessage`** (Property)
    A detailed description of the error encountered.
*   **`string? StackTrace`** (Property)
    The stack trace associated with the error, if applicable.
*   **`string? ErrorCode`** (Property)
    An optional machine-readable error code provided by external services or internal components.
*   **`DateTime OccurredAt`** (Property)
    The timestamp indicating when the error occurred.
*   **`ErrorSeverity Severity`** (Property)
    The severity level of the error, influencing system behavior and alerting.
*   **`bool IsResolved`** (Property)
    Indicates whether the error has been addressed.
*   **`DateTime? ResolvedAt`** (Property)
    The timestamp indicating when the error was marked as resolved, if applicable.
*   **`string? ResolutionNotes`** (Property)
    Optional notes documenting how the error was resolved.
*   **`int RetryAttemptCount`** (Property)
    The number of times the system has attempted to retry the failed operation.
*   **`void MarkAsResolved()`** (Method)
    Marks the error instance as resolved, setting `IsResolved` to `true` and `ResolvedAt` to the current time.
*   **`void RecordRetryAttempt()`** (Method)
    Increments the `RetryAttemptCount` for this error.
*   **`bool IsCritical`** (Property)
    Determines if the error is classified as critical, requiring immediate attention.
*   **`bool IsRetryable`** (Property)
    Determines if the error is of a type that allows for automated retries.
*   **`string GetUserMessage()`** (Method)
    Generates a user-friendly, non-technical explanation of the error suitable for display in the UI or notifications.

## Usage

### Recording a retry attempt
When a transient error occurs during a job, record the attempt to help track failure patterns.

```csharp
if (processingError.IsRetryable)
{
    processingError.RecordRetryAttempt();
    // Proceed with logic to reschedule the job...
}
```

### Resolving an error
Once a root cause has been addressed or the error is deemed no longer relevant, mark it as resolved.

```csharp
if (!processingError.IsResolved)
{
    processingError.MarkAsResolved();
    processingError.ResolutionNotes = "Manually cleared after verifying upstream service availability.";
    
    // Save the changes to the persistence layer...
}
```

## Notes

*   **Thread-Safety**: The `ProcessingError` class is not thread-safe. Methods like `MarkAsResolved` and `RecordRetryAttempt` should not be invoked concurrently on the same instance without external synchronization.
*   **Nullability**: Members marked with `?` (e.g., `Job`, `StackTrace`, `ResolvedAt`) can hold null values. Always perform null checks before accessing their properties to avoid `NullReferenceException`.
*   **State Consistency**: The `ResolvedAt` timestamp is automatically set when `MarkAsResolved` is called. Modifying `IsResolved` directly without using the method may result in an inconsistent state where the resolution timestamp is missing.
*   **Derived Properties**: The `IsCritical` and `IsRetryable` properties are derived from the `ErrorSeverity` and `ErrorType` values. They reflect the current state and are not intended for manual setter assignment.
