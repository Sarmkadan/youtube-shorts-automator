// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="FileValidationServiceTests"/>.
/// </summary>
public static class FileValidationServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the <see cref="FileValidationServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this FileValidationServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true } : _jsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="FileValidationServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static FileValidationServiceTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<FileValidationServiceTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="FileValidationServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful, otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out FileValidationServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<FileValidationServiceTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}