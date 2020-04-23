# LoggingUtility

The `LoggingUtility` is a centralized utility class in the `youtube-shorts-automator` project designed to provide standardized logging across the application, ensuring consistency in structured logging for operations, API calls, and database transactions.

## API

### Static Methods

*   **`LogOperationStart(ILogger logger, string operationName, object? metadata = null)`**
    *   **Purpose:** Logs the initiation of an operation.
    *   **Parameters:** `logger` (the `ILogger` instance), `operationName` (name of the operation), `metadata` (optional contextual data).
    *   **Returns:** `void`.

*   **`LogOperationSuccess(ILogger logger, string operationName, long durationMs, object? metadata = null)`**
    *   **Purpose:** Logs the successful completion of an operation along with its duration.
    *   **Parameters:** `logger`, `operationName`, `durationMs` (execution time in milliseconds), `metadata` (optional).
    *   **Returns:** `void`.

*   **`LogOperationFailure(ILogger logger, string operationName, Exception ex, long durationMs)`**
    *   **Purpose:** Logs an operation failure, including the exception details and duration.
    *   **Parameters:** `logger`, `operationName`, `ex` (the exception encountered), `durationMs` (execution time).
    *   **Returns:** `void`.

*   **`CreateOperationLogger(ILogger logger, string operationName, object? metadata = null)`**
    *   **Purpose:** Factory method to create an `OperationLogger` instance for scoped logging.
    *   **Parameters:** `logger`, `operationName`, `metadata` (optional).
    *   **Returns:** `OperationLogger`.

*   **`LogValidationError(ILogger logger, string operationName, string errorMessage)`**
    *   **Purpose:** Logs validation errors associated with a specific operation.
    *   **Parameters:** `logger`, `operationName`, `errorMessage` (description of the validation error).
    *   **Returns:** `void`.

*   **`LogApiCall(ILogger logger, string apiName, string endpoint, int statusCode, long durationMs)`**
    *   **Purpose:** Logs details of an external API call.
    *   **Parameters:** `logger`, `apiName`, `endpoint` (the URL or resource), `statusCode` (HTTP status code), `durationMs`.
    *   **Returns:** `void`.

*   **`LogDatabaseOperation(ILogger logger, string operationName, string queryType, int recordsAffected, long durationMs)`**
    *   **Purpose:** Logs details of a database operation.
    *   **Parameters:** `logger`, `operationName`, `queryType` (e.g., SELECT, INSERT), `recordsAffected`, `durationMs`.
    *   **Returns:** `void`.

### OperationLogger Class

*   **`OperationLogger(ILogger logger, string operationName, object? metadata = null)`**
    *   **Purpose:** Constructor for scoped operation logging.

*   **`LogSuccess(object? result = null)`**
    *   **Purpose:** Marks the operation as successful.
    *   **Parameters:** `result` (optional result data).
    *   **Returns:** `void`.

*   **`LogFailure(Exception ex)`**
    *   **Purpose:** Marks the operation as failed.
    *   **Parameters:** `ex` (the exception).
    *   **Returns:** `void`.

*   **`Dispose()`**
    *   **Purpose:** Cleans up the scoped logger.
    *   **Returns:** `void`.

## Usage

**1. Using Static Methods**
```csharp
public void ProcessData(ILogger logger)
{
    var sw = Stopwatch.StartNew();
    LoggingUtility.LogOperationStart(logger, "DataProcessing");
    try
    {
        // Perform processing...
        sw.Stop();
        LoggingUtility.LogOperationSuccess(logger, "DataProcessing", sw.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        sw.Stop();
        LoggingUtility.LogOperationFailure(logger, "DataProcessing", ex, sw.ElapsedMilliseconds);
    }
}
```

**2. Using Scoped OperationLogger**
```csharp
public void PerformTask(ILogger logger)
{
    using var op = LoggingUtility.CreateOperationLogger(logger, "ComplexTask");
    try
    {
        // Perform complex logic...
        op.LogSuccess();
    }
    catch (Exception ex)
    {
        op.LogFailure(ex);
    }
}
```

## Notes

*   **Thread Safety:** The `LoggingUtility` static methods are thread-safe, assuming the underlying `ILogger` implementation is thread-safe (standard Microsoft `ILogger` implementations are).
*   **Metadata:** Use caution with large or complex objects passed as `metadata`, as they may be serialized for structured logging and could impact performance or log volume.
*   **Dispose:** The `OperationLogger.Dispose()` method should always be called (preferably via the `using` statement) to ensure proper closure of the log scope.
