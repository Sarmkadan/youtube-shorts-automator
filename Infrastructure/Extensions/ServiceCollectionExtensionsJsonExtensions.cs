// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Infrastructure.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
/// <remarks>
/// <see cref="ServiceCollectionExtensions"/> is a static class containing only extension methods with no serializable state.
/// The serialization methods in this class throw <see cref="NotSupportedException"/> because static classes
/// cannot be meaningfully serialized or deserialized.
/// </remarks>
public static class ServiceCollectionExtensionsJsonExtensions
{
    /// <summary>
    /// JSON serialization options with camelCase naming policy and optimized settings for API responses.
    /// </summary>
    /// <remarks>
    /// Configured with:
    /// - CamelCase property naming for API compatibility
    /// - No indentation for compact JSON output
    /// - Ignores null values to reduce payload size
    /// - Ignores cycles to prevent reference loops in object graphs
    /// </remarks>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the specified <see cref="ServiceCollectionExtensions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ServiceCollectionExtensions"/> instance to serialize.
    /// This parameter is required for API consistency but is not used in the implementation.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.
    /// This parameter is accepted for API consistency but has no effect on the output.</param>
    /// <returns>A JSON string representation of the <see cref="ServiceCollectionExtensions"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Always thrown because <see cref="ServiceCollectionExtensions"/> is a static class with no serializable state.</exception>
    public static string ToJson(object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        throw new NotSupportedException(
            "Serialization of static classes is not supported. ServiceCollectionExtensions contains only extension methods and has no serializable state.");
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ServiceCollectionExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.
    /// This parameter is required for API consistency but is not used in the implementation.</param>
    /// <returns>Always <see langword="null"/> because <see cref="ServiceCollectionExtensions"/> cannot be instantiated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Always thrown because <see cref="ServiceCollectionExtensions"/> is a static class and cannot be instantiated.</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        throw new NotSupportedException(
            "Deserialization to static classes is not supported. ServiceCollectionExtensions is a static class and cannot be instantiated.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ServiceCollectionExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.
    /// This parameter is validated but not used in the implementation.</param>
    /// <param name="value">Receives <see langword="null"/> because deserialization is not supported.
    /// This parameter is required for API consistency.</param>
    /// <returns>Always <see langword="false"/> because deserialization of static classes is not supported.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        value = null;
        return false;
    }
}
