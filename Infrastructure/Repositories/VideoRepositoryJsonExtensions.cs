// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// System.Text.Json serialization extensions for VideoRepository
/// </summary>
public static class VideoRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the VideoRepository to JSON string
    /// </summary>
    /// <param name="value">The VideoRepository instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the VideoRepository</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this VideoRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a VideoRepository from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized VideoRepository instance, or null if JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static VideoRepository? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<VideoRepository>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a VideoRepository from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized VideoRepository</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out VideoRepository? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<VideoRepository>(json, _jsonOptions)!;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}