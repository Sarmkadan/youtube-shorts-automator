// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Domain.Models;

public static class ProcessingErrorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the ProcessingError instance to a JSON string
    /// </summary>
    /// <param name="value">The ProcessingError instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the ProcessingError</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this ProcessingError value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = _jsonOptions;
        if (indented)
        {
            options = new JsonSerializerOptions(_jsonOptions)
            {
                PropertyNamingPolicy = _jsonOptions.PropertyNamingPolicy,
                WriteIndented = true,
                DefaultIgnoreCondition = _jsonOptions.DefaultIgnoreCondition
            };
        }

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ProcessingError instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized ProcessingError instance, or null if JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static ProcessingError? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ProcessingError>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ProcessingError instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter containing the deserialized ProcessingError, or null on failure</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    public static bool TryFromJson(string json, out ProcessingError? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ProcessingError>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}