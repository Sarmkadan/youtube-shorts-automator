// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation helpers for LoggingUtility operations
// Provides validation for logger instances, operation names, and logging parameters
// =====================================================================

using Microsoft.Extensions.Logging;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for LoggingUtility operations
/// Validates logger instances, operation names, and logging parameters
/// </summary>
public static class LoggingUtilityValidation
{
    /// <summary>
    /// Validates ILogger instance for use with LoggingUtility methods
    /// </summary>
    /// <param name="logger">Logger instance to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if logger is null</exception>
    public static IReadOnlyList<string> ValidateLogger(ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates operation name for LoggingUtility methods
    /// </summary>
    /// <param name="operationName">Operation name to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace</exception>
    public static IReadOnlyList<string> ValidateOperationName(string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates operation name and optional parameters for LoggingUtility methods
    /// </summary>
    /// <param name="operationName">Operation name to validate</param>
    /// <param name="parameters">Optional parameters object</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace</exception>
    public static IReadOnlyList<string> ValidateOperationParameters(string operationName, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates operation name, elapsed time, and optional result for LogOperationSuccess
    /// </summary>
    /// <param name="operationName">Operation name to validate</param>
    /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
    /// <param name="result">Optional result object</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace or elapsedMilliseconds is negative</exception>
    public static IReadOnlyList<string> ValidateOperationSuccess(
        string operationName,
        long elapsedMilliseconds,
        object? result = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));

        var problems = new List<string>();

        if (elapsedMilliseconds < 0)
        {
            problems.Add("Elapsed milliseconds cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates operation name, exception, and elapsed time for LogOperationFailure
    /// </summary>
    /// <param name="operationName">Operation name to validate</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if exception is null</exception>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace or elapsedMilliseconds is negative</exception>
    public static IReadOnlyList<string> ValidateOperationFailure(
        string operationName,
        Exception exception,
        long elapsedMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));

        var problems = new List<string>();

        if (elapsedMilliseconds < 0)
        {
            problems.Add("Elapsed milliseconds cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates HTTP method, endpoint, status code, and duration for LogApiCall
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentException">Thrown if method or endpoint is null or whitespace, or statusCode/durationMs is invalid</exception>
    public static IReadOnlyList<string> ValidateApiCall(
        string method,
        string endpoint,
        int statusCode,
        long durationMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method, nameof(method));
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

        var problems = new List<string>();

        if (durationMs < 0)
        {
            problems.Add("Duration cannot be negative");
        }

        if (statusCode < 0)
        {
            problems.Add("Status code cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates database operation parameters for LogDatabaseOperation
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="tableName">Table name</param>
    /// <param name="affectedRows">Number of affected rows</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentException">Thrown if operation or tableName is null or whitespace, or affectedRows/durationMs is invalid</exception>
    public static IReadOnlyList<string> ValidateDatabaseOperation(
        string operation,
        string tableName,
        int affectedRows,
        long durationMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation, nameof(operation));
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));

        var problems = new List<string>();

        if (durationMs < 0)
        {
            problems.Add("Duration cannot be negative");
        }

        if (affectedRows < 0)
        {
            problems.Add("Affected rows cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates field name and error message for LogValidationError
    /// </summary>
    /// <param name="fieldName">Field name</param>
    /// <param name="errorMessage">Error message</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentException">Thrown if fieldName or errorMessage is null or whitespace</exception>
    public static IReadOnlyList<string> ValidateFieldError(string fieldName, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates OperationLogger instance
    /// </summary>
    /// <param name="logger">OperationLogger instance to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if logger is null</exception>
    public static IReadOnlyList<string> ValidateOperationLogger(OperationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if ILogger instance is valid for LoggingUtility operations
    /// </summary>
    /// <param name="logger">Logger instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidLogger(ILogger? logger)
    {
        try
        {
            return ValidateLogger(logger).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if operation name is valid
    /// </summary>
    /// <param name="operationName">Operation name to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidOperationName(string operationName)
    {
        try
        {
            return ValidateOperationName(operationName).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if operation name and parameters are valid
    /// </summary>
    /// <param name="operationName">Operation name to check</param>
    /// <param name="parameters">Optional parameters object</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidOperationParameters(string operationName, object? parameters = null)
    {
        try
        {
            return ValidateOperationParameters(operationName, parameters).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if operation success parameters are valid
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
    /// <param name="result">Optional result object</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidOperationSuccess(
        string operationName,
        long elapsedMilliseconds,
        object? result = null)
    {
        try
        {
            return ValidateOperationSuccess(operationName, elapsedMilliseconds, result).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if operation failure parameters are valid
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidOperationFailure(
        string operationName,
        Exception exception,
        long elapsedMilliseconds)
    {
        try
        {
            return ValidateOperationFailure(operationName, exception, elapsedMilliseconds).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if API call parameters are valid
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidApiCall(
        string method,
        string endpoint,
        int statusCode,
        long durationMs)
    {
        try
        {
            return ValidateApiCall(method, endpoint, statusCode, durationMs).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if database operation parameters are valid
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="tableName">Table name</param>
    /// <param name="affectedRows">Number of affected rows</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidDatabaseOperation(
        string operation,
        string tableName,
        int affectedRows,
        long durationMs)
    {
        try
        {
            return ValidateDatabaseOperation(operation, tableName, affectedRows, durationMs).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if field name and error message are valid
    /// </summary>
    /// <param name="fieldName">Field name</param>
    /// <param name="errorMessage">Error message</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidFieldError(string fieldName, string errorMessage)
    {
        try
        {
            return ValidateFieldError(fieldName, errorMessage).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if OperationLogger instance is valid
    /// </summary>
    /// <param name="logger">OperationLogger instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidOperationLogger(OperationLogger logger)
    {
        try
        {
            return ValidateOperationLogger(logger).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures ILogger instance is valid for LoggingUtility operations, throwing if not
    /// </summary>
    /// <param name="logger">Logger instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    /// <exception cref="ArgumentNullException">Thrown if logger is null</exception>
    public static void EnsureValidLogger(ILogger? logger)
    {
        var problems = ValidateLogger(logger);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Logger is invalid for LoggingUtility operations: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures operation name is valid, throwing if not
    /// </summary>
    /// <param name="operationName">Operation name to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidOperationName(string operationName)
    {
        var problems = ValidateOperationName(operationName);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Operation name is invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures operation name and parameters are valid, throwing if not
    /// </summary>
    /// <param name="operationName">Operation name to validate</param>
    /// <param name="parameters">Optional parameters object</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidOperationParameters(string operationName, object? parameters = null)
    {
        var problems = ValidateOperationParameters(operationName, parameters);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Operation name and parameters are invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures operation success parameters are valid, throwing if not
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
    /// <param name="result">Optional result object</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValidOperationSuccess(
        string operationName,
        long elapsedMilliseconds,
        object? result = null)
    {
        var problems = ValidateOperationSuccess(operationName, elapsedMilliseconds, result);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Operation success parameters are invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures operation failure parameters are valid, throwing if not
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="elapsedMilliseconds">Elapsed time in milliseconds</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValidOperationFailure(
        string operationName,
        Exception exception,
        long elapsedMilliseconds)
    {
        var problems = ValidateOperationFailure(operationName, exception, elapsedMilliseconds);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Operation failure parameters are invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures API call parameters are valid, throwing if not
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValidApiCall(
        string method,
        string endpoint,
        int statusCode,
        long durationMs)
    {
        var problems = ValidateApiCall(method, endpoint, statusCode, durationMs);

        if (problems.Count > 0)
        {
            throw new ArgumentException("API call parameters are invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures database operation parameters are valid, throwing if not
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="tableName">Table name</param>
    /// <param name="affectedRows">Number of affected rows</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValidDatabaseOperation(
        string operation,
        string tableName,
        int affectedRows,
        long durationMs)
    {
        var problems = ValidateDatabaseOperation(operation, tableName, affectedRows, durationMs);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Database operation parameters are invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures field name and error message are valid, throwing if not
    /// </summary>
    /// <param name="fieldName">Field name</param>
    /// <param name="errorMessage">Error message</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidFieldError(string fieldName, string errorMessage)
    {
        var problems = ValidateFieldError(fieldName, errorMessage);

        if (problems.Count > 0)
        {
            throw new ArgumentException("Field name and error message are invalid: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures OperationLogger instance is valid, throwing if not
    /// </summary>
    /// <param name="logger">OperationLogger instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    /// <exception cref="ArgumentNullException">Thrown if logger is null</exception>
    public static void EnsureValidOperationLogger(OperationLogger logger)
    {
        var problems = ValidateOperationLogger(logger);

        if (problems.Count > 0)
        {
            throw new ArgumentException("OperationLogger is invalid: " + string.Join("; ", problems));
        }
    }
}
