using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides JSON serialization extension methods for HttpClientUtility configuration.
/// </summary>
public static class HttpClientUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes HttpClientUtility configuration to a JSON string.
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="userAgent">User agent string.</param>
    /// <param name="indented">Whether to format with indentation.</param>
    /// <returns>JSON string representation.</returns>
    public static string ToJson(int timeoutSeconds = 30, string? userAgent = null, bool indented = false)
    {
        var config = new { timeoutSeconds, userAgent };
        var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Deserializes JSON to HttpClientUtility configuration.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized configuration, or null if JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static HttpClientUtilityConfiguration? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<HttpClientUtilityConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize JSON to HttpClientUtility configuration.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Deserialized configuration if successful.</param>
    /// <returns>True if deserialization succeeds.</returns>
    public static bool TryFromJson(string json, out HttpClientUtilityConfiguration? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<HttpClientUtilityConfiguration>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Configuration data for HttpClientUtility.
    /// </summary>
    public sealed class HttpClientUtilityConfiguration
    {
        /// <summary>Timeout in seconds.</summary>
        public int TimeoutSeconds { get; set; } = 30;
        /// <summary>User agent string.</summary>
        public string? UserAgent { get; set; }
    }
}