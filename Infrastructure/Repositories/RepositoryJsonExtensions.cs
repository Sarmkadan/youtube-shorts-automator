// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for Repository<TEntity>
/// </summary>
public static class RepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the Repository instance to a JSON string
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="value">The Repository instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the Repository</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson<TEntity>(this Repository<TEntity> value, bool indented = false) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a Repository instance
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized Repository instance, or null if JSON is null or empty</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static Repository<TEntity>? FromJson<TEntity>(string json) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Repository<TEntity>>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a Repository instance
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized Repository instance if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static bool TryFromJson<TEntity>(string json, out Repository<TEntity>? value) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(json);

        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<Repository<TEntity>>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
