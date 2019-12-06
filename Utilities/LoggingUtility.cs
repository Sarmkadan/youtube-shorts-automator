// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Structured logging utilities for consistent logging patterns
/// Provides helpers for operation tracking and performance monitoring
/// </summary>
public static class LoggingUtility
{
    public static void LogOperationStart(ILogger logger, string operationName, object? parameters = null)
    {
        if (parameters != null)
        {
            logger.LogInformation("Starting operation: {OperationName} with parameters: {@Parameters}",
                operationName, parameters);
        }
        else
        {
            logger.LogInformation("Starting operation: {OperationName}", operationName);
        }
    }

    public static void LogOperationSuccess(
        ILogger logger,
        string operationName,
        long elapsedMilliseconds,
        object? result = null)
    {
        if (result != null)
        {
            logger.LogInformation("Operation completed successfully: {OperationName} ({ElapsedMs}ms). Result: {@Result}",
                operationName, elapsedMilliseconds, result);
        }
        else
        {
            logger.LogInformation("Operation completed successfully: {OperationName} ({ElapsedMs}ms)",
                operationName, elapsedMilliseconds);
        }
    }

    public static void LogOperationFailure(
        ILogger logger,
        string operationName,
        Exception exception,
        long elapsedMilliseconds)
    {
        logger.LogError(exception, "Operation failed: {OperationName} ({ElapsedMs}ms)",
            operationName, elapsedMilliseconds);
    }

    public static OperationLogger CreateOperationLogger(ILogger logger, string operationName, object? parameters = null)
    {
        return new OperationLogger(logger, operationName, parameters);
    }

    public static void LogValidationError(ILogger logger, string fieldName, string errorMessage)
    {
        logger.LogWarning("Validation error - Field: {FieldName}, Error: {ErrorMessage}",
            fieldName, errorMessage);
    }

    public static void LogApiCall(
        ILogger logger,
        string method,
        string endpoint,
        int statusCode,
        long durationMs)
    {
        var level = statusCode >= 500 ? LogLevel.Error : statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        logger.Log(level, "API Call: {Method} {Endpoint} -> {StatusCode} ({DurationMs}ms)",
            method, endpoint, statusCode, durationMs);
    }

    public static void LogDatabaseOperation(
        ILogger logger,
        string operation,
        string tableName,
        int affectedRows,
        long durationMs)
    {
        logger.LogInformation("Database operation: {Operation} on {TableName} ({AffectedRows} rows, {DurationMs}ms)",
            operation, tableName, affectedRows, durationMs);
    }
}

/// <summary>
/// Tracks operation execution time and logs results automatically
/// </summary>
public class OperationLogger : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly DateTime _startTime;
    private bool _disposed = false;

    public OperationLogger(ILogger logger, string operationName, object? parameters = null)
    {
        _logger = logger;
        _operationName = operationName;
        _startTime = DateTime.UtcNow;

        LoggingUtility.LogOperationStart(logger, operationName, parameters);
    }

    public void LogSuccess(object? result = null)
    {
        if (_disposed)
            return;

        var elapsed = (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        LoggingUtility.LogOperationSuccess(_logger, _operationName, elapsed, result);
        _disposed = true;
    }

    public void LogFailure(Exception exception)
    {
        if (_disposed)
            return;

        var elapsed = (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        LoggingUtility.LogOperationFailure(_logger, _operationName, exception, elapsed);
        _disposed = true;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            var elapsed = (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
            _logger.LogWarning("Operation did not complete explicitly: {OperationName} ({ElapsedMs}ms)",
                _operationName, elapsed);
            _disposed = true;
        }
    }
}
