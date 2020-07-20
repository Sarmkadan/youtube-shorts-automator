// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Provides System.Text.Json serialization extension methods for JsonResponseFormatter
/// </summary>
public static class JsonResponseFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Default)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a JsonResponseFormatter instance to a JSON string
    /// </summary>
    /// <param name="value">The formatter instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the formatter</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this JsonResponseFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (indented)
        {
            var indentedOptions = new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(value, indentedOptions);
        }

        return JsonSerializer.Serialize(value, _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a JsonResponseFormatter instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A JsonResponseFormatter instance, or null if deserialization fails</returns>
    public static JsonResponseFormatter? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<JsonResponseFormatter>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a JsonResponseFormatter instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized instance if successful</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    public static bool TryFromJson(string json, out JsonResponseFormatter? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<JsonResponseFormatter>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}