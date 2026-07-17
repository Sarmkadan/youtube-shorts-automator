// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="SystemController"/> response models.
/// Includes support for camelCase naming policy, enum conversion, and culture-invariant formatting.
/// </summary>
/// <remarks>
/// All serialization uses <see cref="CultureInfo.InvariantCulture"/> to ensure consistent behavior across different system locales.
/// The default JSON serializer options include:
/// <list type="bullet">
///   <item><description>CamelCase property naming policy</description></item>
///   <item><description>Null value suppression for output</description></item>
///   <item><description>CamelCase enum value conversion</description></item>
///   <item><description>Indented formatting disabled by default</description></item>
/// </list>
/// </remarks>
public static class SystemControllerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        // Use InvariantCulture for consistent serialization behavior across different system locales
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="SystemController"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="SystemController"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="SystemController"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SystemController value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SystemController"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SystemController"/> instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static SystemController? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<SystemController>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SystemController"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="SystemController"/> instance if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out SystemController? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<SystemController>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
