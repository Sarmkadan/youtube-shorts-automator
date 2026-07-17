// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for ScheduleRepository
/// </summary>
public static class ScheduleRepositoryJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes the ScheduleRepository instance to a JSON string
	/// </summary>
	/// <param name="value">The ScheduleRepository instance to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability</param>
	/// <returns>A JSON string representation of the ScheduleRepository</returns>
	/// <exception cref="ArgumentNullException">Thrown when value is null</exception>
	public static string ToJson(this ScheduleRepository value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions)
			{
				WriteIndented = true
			} : _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a ScheduleRepository instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <returns>The deserialized ScheduleRepository instance, or null if JSON is empty</returns>
	/// <exception cref="ArgumentNullException">Thrown when json is null</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid</exception>
	public static ScheduleRepository? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<ScheduleRepository>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a ScheduleRepository instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <param name="value">Receives the deserialized ScheduleRepository instance if successful; null if deserialization fails or JSON is empty</param>
	/// <returns>True if deserialization succeeded and produced a non-null result; false otherwise</returns>
	/// <exception cref="ArgumentNullException">Thrown when json is null</exception>
	public static bool TryFromJson(string json, out ScheduleRepository? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			value = default;
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<ScheduleRepository>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = default;
			return false;
		}
	}
}