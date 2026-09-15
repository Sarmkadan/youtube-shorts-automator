namespace YouTubeShortsAutomator.Constants;

/// <summary>
/// Provides side-effect-free formatting helpers for <see cref="TimeSpan"/> values.
/// </summary>
public static class TimeSpanFormattingExtensions
{
    /// <summary>
    /// Formats a duration as a human-readable list of its non-zero day, hour,
    /// minute, and second components.
    /// </summary>
    /// <param name="value">The duration to format.</param>
    /// <returns>
    /// A string such as <c>2 days, 3 hours, 4 minutes, 5 seconds</c>.
    /// Negative durations are prefixed with a minus sign.
    /// </returns>
    public static string ToHumanReadableString(this TimeSpan value)
    {
        var parts = new List<string>(4);

        AddPart(parts, Math.Abs(value.Days), "day");
        AddPart(parts, Math.Abs(value.Hours), "hour");
        AddPart(parts, Math.Abs(value.Minutes), "minute");
        AddPart(parts, Math.Abs(value.Seconds), "second");

        if (parts.Count == 0)
        {
            return "0 seconds";
        }

        var result = string.Join(", ", parts);
        return value < TimeSpan.Zero ? $"-{result}" : result;
    }

    /// <summary>
    /// Formats a duration using compact day, hour, minute, and second units.
    /// </summary>
    /// <param name="value">The duration to format.</param>
    /// <returns>
    /// A string such as <c>2d 3h 4m 5s</c>. Negative durations are prefixed
    /// with a minus sign.
    /// </returns>
    public static string ToCompactString(this TimeSpan value)
    {
        var parts = new List<string>(4);

        AddCompactPart(parts, Math.Abs(value.Days), "d");
        AddCompactPart(parts, Math.Abs(value.Hours), "h");
        AddCompactPart(parts, Math.Abs(value.Minutes), "m");
        AddCompactPart(parts, Math.Abs(value.Seconds), "s");

        if (parts.Count == 0)
        {
            return "0s";
        }

        var result = string.Join(" ", parts);
        return value < TimeSpan.Zero ? $"-{result}" : result;
    }

    private static void AddPart(List<string> parts, int value, string unit)
    {
        if (value > 0)
        {
            parts.Add($"{value} {unit}{(value == 1 ? string.Empty : "s")}");
        }
    }

    private static void AddCompactPart(List<string> parts, int value, string unit)
    {
        if (value > 0)
        {
            parts.Add($"{value}{unit}");
        }
    }
}
