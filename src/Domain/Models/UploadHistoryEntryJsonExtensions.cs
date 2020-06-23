using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides JSON serialization helpers for UploadHistoryEntry.
/// </summary>
public static class UploadHistoryEntryJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Serializes the specified UploadHistoryEntry to a JSON string.
    /// </summary>
    /// <param name="value">The UploadHistoryEntry to serialize.</param>
    /// <param name="indented">If true, the output will be formatted with indentation.</param>
    /// <returns>A JSON representation of the UploadHistoryEntry.</returns>
    public static string ToJson(this UploadHistoryEntry value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        _options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, _options);
    }

    /// <summary>
    /// Deserializes the specified JSON string into an UploadHistoryEntry instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized UploadHistoryEntry, or null if deserialization fails.</returns>
    public static UploadHistoryEntry? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<UploadHistoryEntry>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize the specified JSON string into an UploadHistoryEntry instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized UploadHistoryEntry if successful; otherwise, null.</param>
    /// <returns>true if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out UploadHistoryEntry? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<UploadHistoryEntry>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
