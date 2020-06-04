// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Infrastructure.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization for <see cref="ServiceCollectionExtensions"/>.
/// Note: ServiceCollectionExtensions is a static class with no serializable state.
/// These methods throw NotSupportedException as serialization of static classes is not supported.
/// </summary>
public static class ServiceCollectionExtensionsJsonExtensions
{
    /// <summary>
    /// JSON serialization options with camelCase naming policy
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the <see cref="ServiceCollectionExtensions"/> instance to a JSON string
    /// </summary>
    /// <param name="value">The <see cref="ServiceCollectionExtensions"/> instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the <see cref="ServiceCollectionExtensions"/> instance</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="NotSupportedException">Serialization of static classes is not supported</exception>
    public static string ToJson(object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        throw new NotSupportedException("Serialization of static classes is not supported. ServiceCollectionExtensions contains only extension methods and has no serializable state.");
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ServiceCollectionExtensions"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A <see cref="ServiceCollectionExtensions"/> instance if successful; otherwise, <see langword="null"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="NotSupportedException">Deserialization to static classes is not supported</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        throw new NotSupportedException("Deserialization to static classes is not supported. ServiceCollectionExtensions is a static class and cannot be instantiated.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ServiceCollectionExtensions"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized <see cref="ServiceCollectionExtensions"/> instance if successful</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/></returns>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        value = null;
        return false;
    }
}
