using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides JSON serialization helpers for <see cref="TextOverlayOptions"/>.
/// </summary>
public static class TextOverlayOptionsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serialises the specified <see cref="TextOverlayOptions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="TextOverlayOptions"/> instance to serialise.</param>
    /// <param name="indented">If <c>true</c>, the output will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this TextOverlayOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        _options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, _options);
    }

    /// <summary>
    /// Deserialises the specified JSON string into a <see cref="TextOverlayOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialise.</param>
    /// <returns>A <see cref="TextOverlayOptions"/> instance represented by <paramref name="json"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialised.</exception>
    public static TextOverlayOptions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<TextOverlayOptions>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialise the specified JSON string into a <see cref="TextOverlayOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialise.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialised <see cref="TextOverlayOptions"/> instance
    /// if the operation succeeded, or <c>null</c> if it failed.
    /// </param>
    /// <returns><c>true</c> if the JSON was successfully deserialised; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out TextOverlayOptions? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<TextOverlayOptions>(json, _options);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
