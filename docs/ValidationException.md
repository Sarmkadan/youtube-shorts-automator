# ValidationException

`ValidationException` is a custom exception type used in the `youtube-shorts-automator` project to signal validation failures during data processing. It aggregates multiple validation errors, including the name and value of the invalid field, and provides a structured way to report issues encountered during input validation.

## API

### `public string? FieldName`
- **Purpose**: Identifies the name of the field that failed validation. May be `null` if the validation error is not tied to a specific field.
- **Returns**: The field name as a string, or `null` if not applicable.

### `public string? FieldValue`
- **Purpose**: Contains the value of the field that failed validation. May be `null` if the field itself is `null` or if the error is not value-specific.
- **Returns**: The field value as a string, or `null` if not applicable.

### `public Dictionary<string, string> Errors`
- **Purpose**: Stores a collection of validation errors, where the key represents the error code or identifier and the value provides a human-readable description of the error.
- **Returns**: A non-null `Dictionary<string, string>` containing zero or more error entries.

### `public ValidationException(string message) : base(message)`
- **Purpose**: Initializes a new instance of `ValidationException` with a specified error message.
- **Parameters**:
  - `message` (string): A description of the validation error.
- **Throws**: `ArgumentNullException` if `message` is `null`.

### `public ValidationException()`
- **Purpose**: Initializes a new instance of `ValidationException` with default values. The `Errors` dictionary is initialized as an empty collection.

### `public ValidationException`
- **Purpose**: Default constructor. Equivalent to `ValidationException()`.
- **Remarks**: Exists for compatibility with serialization or reflection-based instantiation.

## Usage

### Example 1: Single Field Validation Error
