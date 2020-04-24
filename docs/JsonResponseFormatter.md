# JsonResponseFormatter

The `JsonResponseFormatter` class provides a centralized mechanism for constructing and formatting JSON responses in a consistent structure across the API. It exposes instance properties that represent the components of a response (success indicator, data, message, timestamp, error details, pagination metadata) and instance methods that serialize those properties into JSON strings. Additionally, it offers static-like utility methods for generic JSON serialization, deserialization, and indented formatting. This class is designed to be used as a builder: you set the relevant properties for the desired response type, then call the corresponding `Format*` method to produce the JSON string.

## API

### Constructor

`public JsonResponseFormatter()`

Initializes a new instance of the formatter with default property values. All properties are initially `null` or `default` (e.g., `Success` is `false`, `Timestamp` is `DateTime.MinValue`).

### Properties – Success Response Group

These properties are used by `FormatSuccessResponse<T>()`.

- `public bool Success`  
  Indicates whether the operation succeeded. Typically set to `true` for a success response.

- `public T? Data`  
  The payload of the response. Can be any type; `null` if no data is returned.

- `public string? Message`  
  An optional human-readable message describing the result.

- `public DateTime Timestamp`  
  The UTC timestamp when the response was generated.

### Properties – Error Response Group

These properties are used by `FormatErrorResponse()`.

- `public bool Success`  
  Always `false` for an error response.

- `public string ErrorCode`  
  A machine-readable error code (e.g., `"VALIDATION_ERROR"`).

- `public string Message`  
  A human-readable error description.

- `public object? Details`  
  Optional additional error details (e.g., validation errors, stack trace).

- `public DateTime Timestamp`  
  The UTC timestamp when the error occurred.

### Properties – Paginated Response Group

These properties are used by `FormatPaginatedResponse<T>()`.

- `public bool Success`  
  Indicates whether the paginated request succeeded.

- `public IEnumerable<T>? Data`  
  The collection of items for the current page.

- `public PaginationInfo? Pagination`  
  Metadata about the pagination (page number, page size, total count, etc.). The `PaginationInfo` type is defined elsewhere.

- `public DateTime Timestamp`  
  The UTC timestamp when the response was generated.

### Methods

#### `public string FormatSuccessResponse<T>()`

Serializes the current values of the success response properties (`Success`, `Data`, `Message`, `Timestamp`) into a compact JSON string.

- **Parameters:** None (uses instance properties).
- **Returns:** A JSON string representing the success response.
- **Throws:** `InvalidOperationException` if `Data` is `null` and the response is expected to contain data (the method does not enforce this; the caller is responsible for setting appropriate values).

#### `public string FormatErrorResponse()`

Serializes the current values of the error response properties (`Success`, `ErrorCode`, `Message`, `Details`, `Timestamp`) into a compact JSON string.

- **Parameters:** None.
- **Returns:** A JSON string representing the error response.
- **Throws:** `InvalidOperationException` if `ErrorCode` or `Message` is `null` or empty.

#### `public string FormatPaginatedResponse<T>()`

Serializes the current values of the paginated response properties (`Success`, `Data`, `Pagination`, `Timestamp`) into a compact JSON string.

- **Parameters:** None.
- **Returns:** A JSON string representing the paginated response.
- **Throws:** `InvalidOperationException` if `Pagination` is `null`.

#### `public T? DeserializeJson<T>(string json)`

Deserializes the given JSON string into an object of type `T`.

- **Parameters:** `json` – The JSON string to deserialize.
- **Returns:** The deserialized object, or `default(T)` if `json` is `null` or empty.
- **Throws:** `JsonException` if the JSON is malformed or cannot be mapped to `T`.

#### `public string SerializeToJson<T>(T obj)`

Serializes the given object into a compact JSON string.

- **Parameters:** `obj` – The object to serialize.
- **Returns:** A JSON string representation of `obj`.
- **Throws:** `ArgumentNullException` if `obj` is `null`.

#### `public string FormatIndentedJson<T>(T obj)`

Serializes the given object into an indented (pretty-printed) JSON string.

- **Parameters:** `obj` – The object to serialize.
- **Returns:** An indented JSON string.
- **Throws:** `ArgumentNullException` if `obj` is `null`.

## Usage

### Example 1: Building a success response

```csharp
var formatter = new JsonResponseFormatter();
formatter.Success = true;
formatter.Data = new { Id = 42, Name = "Sample" };
formatter.Message = "Resource created successfully.";
formatter.Timestamp = DateTime.UtcNow;

string json = formatter.FormatSuccessResponse<object>();
// Output: {"success":true,"data":{"id":42,"name":"Sample"},"message":"Resource created successfully.","timestamp":"2025-03-29T12:00:00Z"}
```

### Example 2: Building an error response and using utility methods

```csharp
var formatter = new JsonResponseFormatter();
formatter.Success = false;
formatter.ErrorCode = "NOT_FOUND";
formatter.Message = "The requested resource was not found.";
formatter.Details = new { ResourceId = 99 };
formatter.Timestamp = DateTime.UtcNow;

string errorJson = formatter.FormatErrorResponse();
// Output: {"success":false,"errorCode":"NOT_FOUND","message":"The requested resource was not found.","details":{"resourceId":99},"timestamp":"..."}

// Utility: deserialize a JSON string
var deserialized = formatter.DeserializeJson<Dictionary<string, object>>(errorJson);
```

## Notes

- **Property groups are mutually exclusive:** The formatter does not enforce that only one group of properties is set at a time. Calling a `Format*` method will use whichever properties are currently assigned, even if they belong to a different group. It is the caller’s responsibility to set only the properties relevant to the intended response type.
- **Null handling:** The `DeserializeJson<T>` method returns `default(T)` for `null` or empty input. The `SerializeToJson<T>` and `FormatIndentedJson<T>` methods throw `ArgumentNullException` when the input object is `null`.
- **Timestamp:** The `Timestamp` property is shared across all response groups. If not explicitly set, it remains `DateTime.MinValue`. It is recommended to always assign a UTC timestamp before calling a `Format*` method.
- **Thread safety:** This class is not thread-safe. Instance members should not be modified concurrently from multiple threads. If concurrent formatting is required, create separate instances or use synchronization.
- **Generic methods:** The generic type parameter `T` in `FormatSuccessResponse<T>` and `FormatPaginatedResponse<T>` is used only for type inference and serialization of the `Data` property. The method itself does not accept a separate argument; it reads the already-set `Data` property.
- **PaginationInfo type:** The `PaginationInfo` class is expected to contain properties such as `Page`, `PageSize`, `TotalCount`, and `TotalPages`. Its exact definition is outside the scope of this formatter.
