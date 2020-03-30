// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// System.Text.Json serialization helpers for FileSystemUtility operations
// Provides JSON serialization/deserialization capabilities compatible with FileSystemUtility
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides System.Text.Json serialization helpers for file system utility operations
/// Enables serialization of file system operation results to JSON and deserialization back to objects
/// </summary>
public static class FileSystemUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Represents a file system operation result that can be serialized to JSON
    /// </summary>
    public sealed record FileSystemOperationResult(
        bool Success,
        string? Message = null,
        long? SizeBytes = null,
        DateTime? Timestamp = null
    );


    /// <summary>
    /// Serializes a file system operation result to a JSON string
    /// </summary>
    /// <param name="result">The file system operation result to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the file system operation result</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
    public static string ToJson(this FileSystemOperationResult result, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(result, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a file system operation result
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized file system operation result, or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null or empty</exception>
    public static FileSystemOperationResult? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<FileSystemOperationResult>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a file system operation result
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized file system operation result if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null or empty</exception>
    public static bool TryFromJson(string json, out FileSystemOperationResult? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<FileSystemOperationResult>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}