// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for RepositoryExtensions type marker
/// </summary>
public static class RepositoryExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an object to a JSON string using camelCase property naming
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>JSON representation of the object</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>The deserialized object, or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        try
        {
            return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to deserialize JSON string", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an object
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter receiving the deserialized object</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}