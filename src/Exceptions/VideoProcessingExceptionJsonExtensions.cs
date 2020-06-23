using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Provides JSON serialization helpers for <see cref="VideoProcessingException"/>.
/// </summary>
public static class VideoProcessingExceptionJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the specified <see cref="VideoProcessingException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The exception instance to serialize.</param>
    /// <param name="indented">If set to <c>true</c>, the JSON output will be indented.</param>
    /// <returns>A JSON representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this VideoProcessingException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = _options.PropertyNamingPolicy,
            WriteIndented = indented
        };
        foreach (var converter in _options.Converters)
        {
            options.Converters.Add(converter);
        }
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into a <see cref="VideoProcessingException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="VideoProcessingException"/>, or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static VideoProcessingException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<VideoProcessingException>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize the specified JSON string into a <see cref="VideoProcessingException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized exception if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out VideoProcessingException? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<VideoProcessingException>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
