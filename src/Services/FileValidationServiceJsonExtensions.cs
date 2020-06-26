using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides JSON serialization helpers for the <see cref="FileValidationService"/> class.
/// </summary>
public static class FileValidationServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Converts a <see cref="FileValidationService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="FileValidationService"/> instance to convert.</param>
    /// <param name="indented">Whether to format the JSON string with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="FileValidationService"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this FileValidationService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="FileValidationService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="FileValidationService"/> instance if deserialization is successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static FileValidationService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            return JsonSerializer.Deserialize<FileValidationService>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="FileValidationService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="FileValidationService"/> instance if successful.</param>
    /// <returns>True if deserialization is successful; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out FileValidationService? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<FileValidationService>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
