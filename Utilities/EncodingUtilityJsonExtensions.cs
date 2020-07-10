using System;
using System.Text.Json;

namespace YoutubeShortsAutomator.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for encoding operations.
    /// Enables serialization of encoding configuration and results to JSON.
    /// </summary>
    public static class EncodingUtilityJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Represents encoding configuration that can be serialized to JSON.
        /// </summary>
        public sealed record EncodingConfiguration(
            string? Base64Text = null,
            string? Sha256Hash = null,
            string? Md5Hash = null,
            string? UrlEncodedText = null,
            string? HtmlEncodedText = null,
            int? RandomStringLength = null,
            bool UseLowercaseHex = true
        );

        /// <summary>
        /// Serializes an encoding configuration to a JSON string.
        /// </summary>
        /// <param name="configuration">The encoding configuration to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the encoding configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public static string ToJson(this EncodingConfiguration configuration, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(configuration, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an encoding configuration.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized encoding configuration, or null if the JSON is null or empty.</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty.</exception>
        public static EncodingConfiguration? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<EncodingConfiguration>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an encoding configuration.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized encoding configuration if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty.</exception>
        public static bool TryFromJson(string json, out EncodingConfiguration? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<EncodingConfiguration>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}