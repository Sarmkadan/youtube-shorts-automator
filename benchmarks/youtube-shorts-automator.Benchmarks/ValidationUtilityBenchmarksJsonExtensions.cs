// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ValidationUtilityBenchmarks"/>
/// to enable benchmark results persistence and exchange.
/// </summary>
public static class ValidationUtilityBenchmarksJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the <see cref="ValidationUtilityBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The benchmark instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the benchmark instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ValidationUtilityBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? _jsonOptionsIndented : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="ValidationUtilityBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized benchmark instance, or null if the JSON is empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ValidationUtilityBenchmarks? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ValidationUtilityBenchmarks>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="ValidationUtilityBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized benchmark instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ValidationUtilityBenchmarks? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ValidationUtilityBenchmarks>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets the JSON serializer options with indentation enabled for pretty-printing.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}