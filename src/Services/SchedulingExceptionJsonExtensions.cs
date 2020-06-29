// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="SchedulingException"/>.
/// </summary>
public static class SchedulingExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the specified <see cref="SchedulingException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The scheduling exception to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the scheduling exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SchedulingException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SchedulingException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized scheduling exception, or null if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static SchedulingException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<SchedulingException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SchedulingException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized scheduling exception, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out SchedulingException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<SchedulingException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}