// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for VideoRepository type.
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
    /// Serializes the VideoRepository to JSON string.
    /// </summary>
    /// <param name="value">The VideoRepository instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string representation of the VideoRepository.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this VideoRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a VideoRepository from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized VideoRepository instance, or null if JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized.</exception>
    public static VideoRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        return JsonSerializer.Deserialize<VideoRepository>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a VideoRepository from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter for the deserialized VideoRepository.</param>
    /// <returns>True if deserialization succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out VideoRepository? value)
        => TryFromJson(json, out value, out _);

    /// <summary>
    /// Attempts to deserialize a VideoRepository from JSON string with error details.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter for the deserialized VideoRepository.</param>
    /// <param name="errorMessage">Output parameter receiving the error message if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out VideoRepository? value, out string? errorMessage)
    {
        value = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage = "JSON string is null or whitespace.";
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<VideoRepository>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}