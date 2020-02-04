// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Safe type conversion utilities with fallback defaults
/// Handles nullable types and culture-aware conversions
/// </summary>
public static class ConversionUtility
{
    public static int ToInt(object? value, int defaultValue = 0)
    {
        if (value == null)
            return defaultValue;

        if (int.TryParse(value.ToString(), out var result))
            return result;

        return defaultValue;
    }

    public static long ToLong(object? value, long defaultValue = 0)
    {
        if (value == null)
            return defaultValue;

        if (long.TryParse(value.ToString(), out var result))
            return result;

        return defaultValue;
    }

    public static decimal ToDecimal(object? value, decimal defaultValue = 0)
    {
        if (value == null)
            return defaultValue;

        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return defaultValue;
    }

    public static double ToDouble(object? value, double defaultValue = 0)
    {
        if (value == null)
            return defaultValue;

        if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return defaultValue;
    }

    public static float ToFloat(object? value, float defaultValue = 0)
    {
        if (value == null)
            return defaultValue;

        if (float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return defaultValue;
    }

    public static bool ToBoolean(object? value, bool defaultValue = false)
    {
        if (value == null)
            return defaultValue;

        var str = value.ToString()?.ToLowerInvariant();
        return str == "true" || str == "1" || str == "yes" || str == "on";
    }

    public static DateTime ToDateTime(object? value, DateTime? defaultValue = null)
    {
        if (value == null)
            return defaultValue ?? DateTime.MinValue;

        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var result))
            return result;

        return defaultValue ?? DateTime.MinValue;
    }

    public static Guid ToGuid(object? value, Guid? defaultValue = null)
    {
        if (value == null)
            return defaultValue ?? Guid.Empty;

        if (Guid.TryParse(value.ToString(), out var result))
            return result;

        return defaultValue ?? Guid.Empty;
    }

    public static T? ToEnum<T>(object? value, T? defaultValue = null) where T : struct, Enum
    {
        if (value == null)
            return defaultValue;

        if (Enum.TryParse<T>(value.ToString(), true, out var result))
            return result;

        return defaultValue;
    }

    public static string ToString(object? value, string defaultValue = "")
    {
        return value?.ToString() ?? defaultValue;
    }

    public static byte[] ToByteArray(object? value)
    {
        if (value == null)
            return Array.Empty<byte>();

        if (value is byte[] bytes)
            return bytes;

        if (value is string str)
            return System.Text.Encoding.UTF8.GetBytes(str);

        return System.Text.Encoding.UTF8.GetBytes(value.ToString() ?? "");
    }

    public static T? JsonDeserialize<T>(string json) where T : class
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }

    public static string JsonSerialize<T>(T obj) where T : class
    {
        try
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static List<T> ToList<T>(object? value) where T : notnull
    {
        if (value == null)
            return new List<T>();

        if (value is IEnumerable<T> enumerable)
            return enumerable.ToList();

        if (value is T item)
            return new List<T> { item };

        return new List<T>();
    }

    public static Dictionary<string, object?> ObjectToDictionary(object? obj)
    {
        if (obj == null)
            return new Dictionary<string, object?>();

        var properties = obj.GetType().GetProperties();
        var result = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            result[prop.Name] = prop.GetValue(obj);
        }

        return result;
    }
}
