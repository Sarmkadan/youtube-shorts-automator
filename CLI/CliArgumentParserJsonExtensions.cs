using System.Text.Json;

namespace YouTubeShortsAutomator.CLI;

public static class CliArgumentParserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Serializes the <see cref="CliArgumentParser"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The parser instance to serialize.</param>
    /// <param name="indented">Whether to indent the JSON output for readability.</param>
    /// <returns>A JSON representation of the parser.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CliArgumentParser value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? _jsonOptionsIndented : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="CliArgumentParser"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized parser instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CliArgumentParser? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CliArgumentParser>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="CliArgumentParser"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized parser instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out CliArgumentParser? value)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<CliArgumentParser>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}