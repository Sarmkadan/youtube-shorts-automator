// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Provides JSON serialization extensions for <see cref="RateLimitingMiddleware"/> and related types
/// </summary>
public static class RateLimitingMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="RateLimitingMiddleware"/> instance to JSON string
    /// </summary>
    /// <param name="value">The middleware instance to serialize</param>
    /// <param name="indented">Whether to use indented formatting</param>
    /// <returns>JSON representation of the middleware</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this RateLimitingMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="RateLimitingMiddleware"/> from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>The deserialized middleware instance, or null if JSON is empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static RateLimitingMiddleware? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="RateLimitingMiddleware"/> from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output variable for the deserialized instance</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    public static bool TryFromJson(string json, out RateLimitingMiddleware? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes <see cref="RateLimitingOptions"/> to JSON string
    /// </summary>
    /// <param name="value">The options to serialize</param>
    /// <param name="indented">Whether to use indented formatting</param>
    /// <returns>JSON representation of the options</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this RateLimitingOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes <see cref="RateLimitingOptions"/> from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>The deserialized options instance, or null if JSON is empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static RateLimitingOptions? FromJsonToOptions(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<RateLimitingOptions>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize <see cref="RateLimitingOptions"/> from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output variable for the deserialized instance</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    public static bool TryFromJson(string json, out RateLimitingOptions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<RateLimitingOptions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}