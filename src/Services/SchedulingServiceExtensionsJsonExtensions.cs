// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SchedulingServiceExtensions"/> type marker.
/// </summary>
public static class SchedulingServiceExtensionsJsonExtensions
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
    /// Serializes a <see cref="SchedulingServiceExtensions"/> type marker to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="SchedulingServiceExtensions"/> type marker.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON representation of the <see cref="SchedulingServiceExtensions"/> type marker.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SchedulingServiceExtensions"/> type marker.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns><see cref="SchedulingServiceExtensions"/> type marker instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
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
    /// Attempts to deserialize a JSON string to a <see cref="SchedulingServiceExtensions"/> type marker.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter receiving the deserialized type marker.</param>
    /// <returns>True if deserialization succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
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
