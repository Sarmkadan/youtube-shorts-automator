# ApiResult

The `ApiResult` and `ApiResult<T>` types provide a standardized envelope for API responses within the `youtube-shorts-automator` project. They encapsulate operational outcomes, allowing service methods to return a uniform object containing success indicators, human-readable messages, error codes, and optional payload data or validation details, thereby promoting consistent error handling and response structures across the codebase.

## API

### ApiResult (Non-Generic)

*   **`bool IsSuccess`**: Indicates whether the operation was completed successfully.
*   **`string Message`**: A human-readable message describing the result of the operation.
*   **`string? ErrorCode`**: An optional machine-readable error code if the operation failed.
*   **`Dictionary<string, string>? Errors`**: An optional dictionary containing detailed validation or field-specific errors.
*   **`static ApiResult Success`**: A factory method to create an `ApiResult` indicating success.
*   **`static ApiResult Failure`**: A factory method to create an `ApiResult` indicating a general failure.
*   **`static ApiResult ValidationFailure`**: A factory method to create an `ApiResult` indicating a validation-related failure.

### ApiResult<T> (Generic)

*   **`bool IsSuccess`**: Indicates whether the operation was completed successfully.
*   **`T? Data`**: The payload data returned by the operation if successful; otherwise, `null`.
*   **`string Message`**: A human-readable message describing the result of the operation.
*   **`string? ErrorCode`**: An optional machine-readable error code if the operation failed.
*   **`Dictionary<string, string>? Errors`**: An optional dictionary containing detailed validation or field-specific errors.
*   **`List<T> Items`**: A list of items returned by the operation, useful for collection-based responses.
*   **`static ApiResult<T> Success`**: A factory method to create a successful `ApiResult<T>` instance.
*   **`static ApiResult<T> Failure`**: A factory method to create a failed `ApiResult<T>` instance.
*   **`static ApiResult<T> ValidationFailure`**: A factory method to create a `ApiResult<T>` instance indicating a validation failure.
*   **`static ApiResult<T> NotFound`**: A factory method to create an `ApiResult<T>` instance indicating that the requested resource was not found.
*   **`static ApiResult<T> Conflict`**: A factory method to create an `ApiResult<T>` instance indicating a resource conflict.
*   **`TOut Map<TOut>(...)`**: Maps the result data to a different type.
*   **`async Task<TOut> MapAsync<TOut>(...)`**: Asynchronously maps the result data to a different type.

## Usage

### Example 1: Returning a successful result with data
```csharp
public ApiResult<VideoMetadata> GetVideoMetadata(string videoId)
{
    var metadata = _repository.GetById(videoId);
    if (metadata == null)
    {
        return ApiResult<VideoMetadata>.NotFound;
    }

    return ApiResult<VideoMetadata>.Success; // Assumes internal assignment or constructor usage
    // Or populated explicitly:
    // return new ApiResult<VideoMetadata> { IsSuccess = true, Data = metadata };
}
```

### Example 2: Handling a validation failure
```csharp
public ApiResult ProcessJob(ProcessingJobRequest request)
{
    if (string.IsNullOrEmpty(request.JobId))
    {
        return ApiResult.ValidationFailure;
    }

    // Process request...
    return ApiResult.Success;
}
```

## Notes

*   **Nullability**: Ensure that the `Data` property of `ApiResult<T>` is checked for `null` if the operation was not successful (`IsSuccess` is false).
*   **Immutability**: These types are designed to be used as data transfer objects (DTOs) and should be treated as immutable once returned from a method.
*   **Thread Safety**: Since these types are generally used as immutable result containers, they are safe to be accessed across threads without additional synchronization, provided they are not modified after instantiation.
*   **Mapping**: The `Map` and `MapAsync` methods should be used to transform successful results into different representations, ensuring that the `IsSuccess` state and error information are appropriately handled or propagated.
