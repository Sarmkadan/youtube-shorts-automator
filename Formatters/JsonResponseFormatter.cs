// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Standardizes JSON response formatting across the API
/// Provides consistent response structure with metadata and error handling
/// </summary>
public class JsonResponseFormatter
{
    private readonly JsonSerializerOptions _options;

    public JsonResponseFormatter()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public string FormatSuccessResponse<T>(T data, string? message = null)
    {
        var response = new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, _options);
    }

    public string FormatErrorResponse(string errorCode, string message, object? details = null)
    {
        var response = new ApiErrorResponse
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, _options);
    }

    public string FormatPaginatedResponse<T>(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount)
    {
        var response = new ApiPaginatedResponse<T>
        {
            Success = true,
            Data = data,
            Pagination = new PaginationInfo
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            },
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, _options);
    }

    public T? DeserializeJson<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public string SerializeToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public string FormatIndentedJson<T>(T obj)
    {
        var indentedOptions = new JsonSerializerOptions(_options)
        {
            WriteIndented = true
        };
        return JsonSerializer.Serialize(obj, indentedOptions);
    }
}

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class ApiErrorResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public object? Details { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class ApiPaginatedResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public IEnumerable<T>? Data { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class PaginationInfo
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
