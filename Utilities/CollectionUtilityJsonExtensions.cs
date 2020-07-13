// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// JSON serialization extensions for CollectionUtility configuration
// Provides System.Text.Json serialization helpers for CollectionUtility configuration
// =====================================================================

using System.Text.Json;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides JSON serialization extension methods for CollectionUtility configuration
/// </summary>
public static class CollectionUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes CollectionUtility configuration to a JSON string
    /// </summary>
    /// <param name="chunkSize">The chunk size configuration value</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the CollectionUtility configuration</returns>
    public static string ToJson(int chunkSize = 100, bool indented = false)
    {
        var config = new CollectionUtilityConfiguration { ChunkSize = chunkSize };
        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a CollectionUtility configuration
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized CollectionUtility configuration, or null if JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static CollectionUtilityConfiguration? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CollectionUtilityConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CollectionUtility configuration
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized CollectionUtility configuration if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out CollectionUtilityConfiguration? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<CollectionUtilityConfiguration>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Configuration data for CollectionUtility operations
    /// </summary>
    public sealed class CollectionUtilityConfiguration
    {
        /// <summary>
        /// Gets or sets the chunk size for batch operations
        /// </summary>
        public int ChunkSize { get; set; } = 100;
    }
}