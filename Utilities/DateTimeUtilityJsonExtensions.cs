using System.Text.Json;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization helpers for DateTimeUtility operations.
/// </summary>
public static class DateTimeUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes DateTimeUtility configuration to a JSON string.
    /// </summary>
    /// <param name="formatUtcDateTime">Whether to format as UTC datetime.</param>
    /// <param name="formatIsoDateTime">Whether to format as ISO datetime.</param>
    /// <param name="formatRelativeTime">Whether to format as relative time.</param>
    /// <param name="formatDuration">Whether to format as duration.</param>
    /// <param name="isWithinBusinessHours">Whether to check business hours.</param>
    /// <param name="startHour">Business hours start hour.</param>
    /// <param name="endHour">Business hours end hour.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the configuration.</returns>
    public static string ToJson(
        bool formatUtcDateTime = false,
        bool formatIsoDateTime = false,
        bool formatRelativeTime = false,
        bool formatDuration = false,
        bool isWithinBusinessHours = false,
        int startHour = 8,
        int endHour = 18,
        bool indented = false)
    {
        var config = new DateTimeUtilityConfiguration
        {
            FormatUtcDateTime = formatUtcDateTime,
            FormatIsoDateTime = formatIsoDateTime,
            FormatRelativeTime = formatRelativeTime,
            FormatDuration = formatDuration,
            IsWithinBusinessHours = isWithinBusinessHours,
            StartHour = startHour,
            EndHour = endHour
        };

        JsonOptions.WriteIndented = indented;
        return JsonSerializer.Serialize(config, JsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTimeUtility configuration.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized DateTimeUtility configuration, or null if JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static DateTimeUtilityConfiguration? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<DateTimeUtilityConfiguration>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a DateTimeUtility configuration.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized DateTimeUtility configuration if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out DateTimeUtilityConfiguration? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

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

    /// <summary>
    /// Configuration data for DateTimeUtility operations.
    /// </summary>
    public sealed class DateTimeUtilityConfiguration
    {
        /// <summary>
        /// Gets or sets whether to format as UTC datetime.
        /// </summary>
        public bool FormatUtcDateTime { get; set; }

        /// <summary>
        /// Gets or sets whether to format as ISO datetime.
        /// </summary>
        public bool FormatIsoDateTime { get; set; }

        /// <summary>
        /// Gets or sets whether to format as relative time.
        /// </summary>
        public bool FormatRelativeTime { get; set; }

        /// <summary>
        /// Gets or sets whether to format as duration.
        /// </summary>
        public bool FormatDuration { get; set; }

        /// <summary>
        /// Gets or sets whether to check business hours.
        /// </summary>
        public bool IsWithinBusinessHours { get; set; }

        /// <summary>
        /// Gets or sets the business hours start hour.
        /// </summary>
        public int StartHour { get; set; }

        /// <summary>
        /// Gets or sets the business hours end hour.
        /// </summary>
        public int EndHour { get; set; }
    }
}