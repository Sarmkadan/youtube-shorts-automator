// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace YouTubeShortsAutomator.Domain.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="DomainException"/> and its derived types.
/// </summary>
public static class DomainExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        // Use property filter to ignore null values when writing
    };

    static DomainExceptionJsonExtensions()
    {
        // Configure type discriminator for polymorphic serialization of DomainException hierarchy
        _jsonOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
    }

    /// <summary>
    /// Serializes a <see cref="DomainException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The domain exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DomainException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DomainException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="DomainException"/> or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DomainException? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var exception = JsonSerializer.Deserialize<DomainException>(json, _jsonOptions);
        return exception;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DomainException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized exception if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out DomainException? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DomainException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}