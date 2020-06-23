// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for serializing and deserializing <see cref="OptimizationSuggestion"/>
/// instances to and from JSON using System.Text.Json.
/// </summary>
public static class OptimizationSuggestionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the specified <see cref="OptimizationSuggestion"/> value to a JSON string.
    /// </summary>
    /// <param name="value">The optimization suggestion to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the optimization suggestion.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this OptimizationSuggestion value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="OptimizationSuggestion"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="OptimizationSuggestion"/> instance, or <c>null</c> if the JSON is empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static OptimizationSuggestion? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<OptimizationSuggestion>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="OptimizationSuggestion"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="OptimizationSuggestion"/> instance if successful, otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryFromJson(string json, out OptimizationSuggestion? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<OptimizationSuggestion>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}