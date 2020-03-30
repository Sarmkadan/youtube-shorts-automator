// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization helpers for UploadSchedule and related types
/// </summary>
public static class UploadScheduleJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Converts an UploadSchedule instance to a JSON string
    /// </summary>
    /// <param name="value">The UploadSchedule instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the UploadSchedule</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this UploadSchedule value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses an UploadSchedule instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>The deserialized UploadSchedule instance, or null if parsing fails</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static UploadSchedule? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<UploadSchedule>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse an UploadSchedule instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <param name="value">Receives the deserialized UploadSchedule if successful</param>
    /// <returns>True if parsing succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static bool TryFromJson(string json, out UploadSchedule? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<UploadSchedule>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}