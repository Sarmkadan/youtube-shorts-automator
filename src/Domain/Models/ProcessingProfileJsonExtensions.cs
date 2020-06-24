// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ProcessingProfile"/>.
/// </summary>
public static class ProcessingProfileJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the specified <see cref="ProcessingProfile"/> to a JSON string.
    /// </summary>
    /// <param name="value">The processing profile to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output will be formatted with indentation.</param>
    /// <returns>A JSON string representation of the processing profile.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this ProcessingProfile value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        _options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, _options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ProcessingProfile"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized processing profile, or <c>null</c> if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static ProcessingProfile? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<ProcessingProfile>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ProcessingProfile"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="ProcessingProfile"/> instance
    /// if the operation succeeded, or <c>null</c> if it failed.
    /// </param>
    /// <returns><c>true</c> if the JSON was successfully deserialized; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out ProcessingProfile? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ProcessingProfile>(json, _options);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}