using System.Text.Json;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for objects.
/// </summary>
public static class TextOverlayOptionsExtensionsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON representation of the <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="NotSupportedException">The type of <paramref name="value"/> is not supported for serialization.</exception>
    /// <exception cref="ArgumentException">The serializer options are invalid.</exception>
    /// <exception cref="InvalidOperationException">There is a circular reference in the object graph.</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="NotSupportedException">The type of the deserialized object is not supported.</exception>
    /// <exception cref="ArgumentException">The JSON is invalid for the target type or the options are invalid.</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<object>(json, _options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an object.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">When the method returns true, contains the deserialized object; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="NotSupportedException">The type of the deserialized object is not supported.</exception>
    /// <exception cref="ArgumentException">The JSON is invalid for the target type or the options are invalid.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}