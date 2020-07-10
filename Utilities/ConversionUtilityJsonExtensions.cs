// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides System.Text.Json serialization helpers for conversion utility operations
/// Enables serialization of conversion operation results to JSON and deserialization back to objects
/// </summary>
public static class ConversionUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Represents a conversion operation result that can be serialized to JSON
    /// </summary>
    public sealed record ConversionResult(
        object? OriginalValue,
        int? IntValue,
        long? LongValue,
        decimal? DecimalValue,
        double? DoubleValue,
        float? FloatValue,
        bool? BooleanValue,
        DateTime? DateTimeValue,
        Guid? GuidValue,
        string? StringValue,
        string? ErrorMessage = null
    );

    /// <summary>
    /// Serializes a conversion result to a JSON string
    /// </summary>
    /// <param name="result">The conversion result to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the conversion result</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
    public static string ToJson(this ConversionResult result, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(result, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a conversion result
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized conversion result, or null if deserialization fails</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static ConversionResult? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<ConversionResult>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a conversion result
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized conversion result if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    public static bool TryFromJson(string json, out ConversionResult? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ConversionResult>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}