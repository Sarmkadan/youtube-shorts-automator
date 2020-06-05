using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeShortsAutomator.Infrastructure.Repositories;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

public static class ProcessingJobRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Converts a ProcessingJobRepository to a JSON string.
    /// </summary>
    /// <param name="value">The ProcessingJobRepository to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the ProcessingJobRepository.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this ProcessingJobRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ProcessingJobRepository.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A ProcessingJobRepository deserialized from the JSON string, or null if the JSON is invalid.</returns>
    public static ProcessingJobRepository? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ProcessingJobRepository>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ProcessingJobRepository.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized ProcessingJobRepository, or null if the JSON is invalid.</param>
    /// <returns>True if the JSON was successfully deserialized, false otherwise.</returns>
    public static bool TryFromJson(string json, out ProcessingJobRepository? value)
    {
        value = FromJson(json);
        return value != null;
    }
}
