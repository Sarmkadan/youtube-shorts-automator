// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for JsonResponseFormatter providing additional utility methods
// for common JSON formatting scenarios
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Extension methods for JsonResponseFormatter providing additional utility methods
/// for common JSON formatting scenarios including validation, parsing, and
/// specialized response formatting
/// </summary>
public static class JsonResponseFormatterExtensions
{
    /// <summary>
    /// Safely attempts to parse a JSON string into a strongly-typed object with
    /// detailed error information in the response
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <param name="formatter">The JsonResponseFormatter instance</param>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>A response containing either the parsed data or detailed error information</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static string FormatParseResponse<T>(this JsonResponseFormatter formatter, string json)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            var result = formatter.DeserializeJson<T>(json);

            if (result is not null)
            {
                return formatter.FormatSuccessResponse(result, "Successfully parsed JSON");
            }

            var errorDetails = new
            {
                JsonLength = json.Length,
                First20Chars = json.Length > 20 ? json[..20] : json,
                IsValidJson = IsValidJson(json)
            };

            return formatter.FormatErrorResponse(
                "JSON_PARSE_FAILED",
                "Failed to parse JSON into the specified type",
                errorDetails
            );
        }
        catch (Exception ex)
        {
            return formatter.FormatErrorResponse(
                "JSON_PARSE_EXCEPTION",
                "Exception occurred during JSON parsing",
                new
                {
                    ExceptionType = ex.GetType().Name,
                    ExceptionMessage = ex.Message,
                    JsonLength = json.Length,
                    Timestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
                }
            );
        }
    }

    /// <summary>
    /// Formats a validation response with detailed validation errors
    /// </summary>
    /// <typeparam name="T">The type of data being validated</typeparam>
    /// <param name="formatter">The JsonResponseFormatter instance</param>
    /// <param name="isValid">Whether validation passed</param>
    /// <param name="data">The validated data</param>
    /// <param name="validationErrors">Collection of validation error messages</param>
    /// <returns>Formatted JSON response with validation results</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null and isValid is true</exception>
    public static string FormatValidationResponse<T>(
        this JsonResponseFormatter formatter,
        bool isValid,
        T? data,
        IReadOnlyList<string> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(validationErrors);

        if (isValid)
        {
            ArgumentNullException.ThrowIfNull(data);
            return formatter.FormatSuccessResponse(data, "Validation passed");
        }

        var errorDetails = new
        {
            Errors = validationErrors,
            ErrorCount = validationErrors.Count,
            Timestamp = DateTime.UtcNow
        };

        return formatter.FormatErrorResponse(
            "VALIDATION_FAILED",
            "Validation failed with errors",
            errorDetails
        );
    }

    /// <summary>
    /// Formats a response with additional metadata beyond standard success/error responses
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    /// <param name="formatter">The JsonResponseFormatter instance</param>
    /// <param name="data">The data to include in response</param>
    /// <param name="metadata">Additional metadata to include</param>
    /// <param name="message">Optional success message</param>
    /// <returns>Formatted JSON response with metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when data or metadata is null</exception>
    public static string FormatMetadataResponse<T>(
        this JsonResponseFormatter formatter,
        T data,
        IReadOnlyDictionary<string, object> metadata,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(metadata);

        var response = new
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully",
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };

        return formatter.SerializeToJson(response);
    }

    /// <summary>
    /// Safely formats a response that may contain circular references or complex objects
    /// by attempting serialization and providing detailed error information
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="formatter">The JsonResponseFormatter instance</param>
    /// <param name="obj">The object to serialize</param>
    /// <param name="fallbackMessage">Message to use if serialization fails</param>
    /// <returns>Formatted JSON response with either serialized data or error details</returns>
    /// <exception cref="ArgumentNullException">Thrown when obj is null</exception>
    public static string FormatSafeSerialization<T>(
        this JsonResponseFormatter formatter,
        T obj,
        string? fallbackMessage = null)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            var json = formatter.SerializeToJson(obj);
            return formatter.FormatSuccessResponse(json, "Object serialized successfully");
        }
        catch (Exception ex)
        {
            return formatter.FormatErrorResponse(
                "SERIALIZATION_FAILED",
                fallbackMessage ?? "Failed to serialize object to JSON",
                new
                {
                    ExceptionType = ex.GetType().Name,
                    ExceptionMessage = ex.Message,
                    ObjectType = typeof(T).FullName,
                    Timestamp = DateTime.UtcNow
                }
            );
        }
    }

    /// <summary>
    /// Determines whether the given string is valid JSON
    /// </summary>
    /// <param name="json">The JSON string to validate</param>
    /// <returns>True if the string is valid JSON; otherwise, false</returns>
    private static bool IsValidJson(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Undefined;
        }
        catch
        {
            return false;
        }
    }
}