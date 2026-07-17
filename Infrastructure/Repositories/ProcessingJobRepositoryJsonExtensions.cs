// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeShortsAutomator.Infrastructure.Repositories;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for ProcessingJobRepository type.
/// </summary>
public static class ProcessingJobRepositoryJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true
	};

	/// <summary>
	/// Serializes the ProcessingJobRepository to a JSON string.
	/// </summary>
	/// <param name="value">The ProcessingJobRepository instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the ProcessingJobRepository.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	public static string ToJson(this ProcessingJobRepository value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a ProcessingJobRepository from JSON string.
	/// </summary>
	/// <param name="json">JSON string to deserialize.</param>
	/// <returns>Deserialized ProcessingJobRepository instance, or null if JSON is null or empty.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
	/// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized.</exception>
	public static ProcessingJobRepository? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json.Trim());

		return JsonSerializer.Deserialize<ProcessingJobRepository>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a ProcessingJobRepository from JSON string.
	/// </summary>
	/// <param name="json">JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized ProcessingJobRepository instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out ProcessingJobRepository? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = default;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<ProcessingJobRepository>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	/// <summary>
	/// Attempts to deserialize a ProcessingJobRepository from JSON string with error details.
	/// </summary>
	/// <param name="json">JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized ProcessingJobRepository instance if successful.</param>
	/// <param name="errorMessage">Receives the error message if deserialization fails.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out ProcessingJobRepository? value, out string? errorMessage)
	{
		value = null;
		errorMessage = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			errorMessage = "JSON string is null or whitespace.";
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<ProcessingJobRepository>(json, _jsonSerializerOptions);
			return value is not null;
		}
		catch (JsonException ex)
		{
			errorMessage = ex.Message;
			return false;
		}
	}
}