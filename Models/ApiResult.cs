// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Models;

/// <summary>
/// Generic result wrapper for API operations
/// Encapsulates success/failure state with optional data and error details
/// </summary>

public class ApiResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string>? Errors { get; set; }

    public static ApiResult Success(string message = "Operation successful")
    {
        return new ApiResult
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static ApiResult Failure(string message, string? errorCode = null)
    {
        return new ApiResult
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode
        };
    }

    public static ApiResult ValidationFailure(Dictionary<string, string> errors)
    {
        return new ApiResult
        {
            IsSuccess = false,
            Message = "Validation failed",
            ErrorCode = "VALIDATION_ERROR",
            Errors = errors
        };
    }
}

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string>? Errors { get; set; }

    public static ApiResult<T> Success(T? data, string message = "Operation successful")
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResult<T> Failure(string message, string? errorCode = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode
        };
    }

    public static ApiResult<T> ValidationFailure(Dictionary<string, string> errors)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Message = "Validation failed",
            ErrorCode = "VALIDATION_ERROR",
            Errors = errors
        };
    }

    public static ApiResult<T> NotFound(string message = "Resource not found")
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = "NOT_FOUND"
        };
    }

    public static ApiResult<T> Conflict(string message = "Resource conflict")
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = "CONFLICT"
        };
    }

    public TOut Map<TOut>(Func<T?, TOut> mapper)
    {
        return mapper(Data);
    }

    public async Task<TOut> MapAsync<TOut>(Func<T?, Task<TOut>> mapper)
    {
        return await mapper(Data);
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

public class BulkOperationResult
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public List<BulkItemResult> ItemResults { get; set; } = new();

    public bool IsSuccessful => FailedItems == 0;
    public double SuccessRate => TotalItems > 0 ? ((double)SuccessfulItems / TotalItems) * 100 : 0;
}

public class BulkItemResult
{
    public string ItemIdentifier { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}
