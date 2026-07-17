// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for converting objects to and from JSON strings.
/// This is a utility class for JSON serialization/deserialization operations.
/// </summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this object? value, bool indented = false) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(
                value,
                indented
                    ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                    : _jsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <returns>An object instance if deserialization succeeds; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
    /// <param name="value">Receives the deserialized object if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}