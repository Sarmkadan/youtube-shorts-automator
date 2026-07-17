using System;
using System.Text.Json;

namespace YouTubeShortsAutomator.Infrastructure.Repositories
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for converting objects to and from JSON strings.
    /// </summary>
    public static class ScheduleRepositoryExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
        public static string ToJson(this object? value, bool indented = false)
            => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true } : _jsonSerializerOptions);

        /// <summary>
        /// Deserializes a JSON string to an object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object instance, or <see langword="null"/> if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
        public static object? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object, or <see langword="null"/> if deserialization fails.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
        public static bool TryFromJson(string json, out object? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = default;
                return false;
            }
        }
    }
}
