using System.Text.Json;

namespace YouTubeShortsAutomator.API
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="ConfigurationController"/>
    /// </summary>
    public static class ConfigurationControllerJsonExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        /// <summary>
        /// Serializes the <see cref="ConfigurationController"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The controller instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the controller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ConfigurationController value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
                : JsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ConfigurationController"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="ConfigurationController"/> instance, or <see langword="null"/> if the JSON represents a null value.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to <see cref="ConfigurationController"/>.</exception>
        public static ConfigurationController? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            return JsonSerializer.Deserialize<ConfigurationController>(json, JsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="ConfigurationController"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        public static bool TryFromJson(string json, out ConfigurationController? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}