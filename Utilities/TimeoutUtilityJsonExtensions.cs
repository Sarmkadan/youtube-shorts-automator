// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for timeout-related data structures
/// </summary>
public static class TimeoutUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a timeout configuration to a JSON string
    /// </summary>
    /// <param name="timeoutSeconds">The timeout value in seconds</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the timeout configuration</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeoutSeconds is negative</exception>
    public static string ToJson(int timeoutSeconds, bool indented = false)
    {
        if (timeoutSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "Timeout value cannot be negative");
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { timeoutSeconds }, options);
    }

    /// <summary>
    /// Serializes a TimeSpan timeout to a JSON string
    /// </summary>
    /// <param name="timeout">The timeout value as TimeSpan</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the timeout configuration</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is negative</exception>
    public static string ToJson(TimeSpan timeout, bool indented = false)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout value cannot be negative");
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { timeoutSeconds = (int)timeout.TotalSeconds }, options);
    }

    /// <summary>
    /// Deserializes a timeout configuration from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An int representing timeout in seconds, or null if the JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static int? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var result = JsonSerializer.Deserialize<TimeoutConfiguration>(json, _jsonOptions);
        return result?.TimeoutSeconds;
    }

    /// <summary>
    /// Attempts to deserialize a timeout configuration from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="timeoutSeconds">Receives the timeout in seconds if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out int? timeoutSeconds)
    {
        timeoutSeconds = default;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            var result = JsonSerializer.Deserialize<TimeoutConfiguration>(json, _jsonOptions);
            timeoutSeconds = result?.TimeoutSeconds;
            return timeoutSeconds.HasValue;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Record representing timeout configuration for serialization
    /// </summary>
    private sealed record TimeoutConfiguration(int TimeoutSeconds);
}