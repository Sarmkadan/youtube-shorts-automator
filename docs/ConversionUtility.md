# ConversionUtility

The `ConversionUtility` class provides a set of static helper methods for robust data type conversion, casting, and JSON serialization within the `youtube-shorts-automator` project. It facilitates standardized data transformation by wrapping common parsing logic and simplifying complex operations such as enum mapping, object-to-dictionary conversion, and JSON handling.

## API

*   **`ToInt(object? input)`**: Converts the provided input to a 32-bit signed integer. Throws `FormatException` or `InvalidCastException` if the conversion fails.
*   **`ToLong(object? input)`**: Converts the provided input to a 64-bit signed integer. Throws `FormatException` or `InvalidCastException` if the conversion fails.
*   **`ToDecimal(object? input)`**: Converts the provided input to a `decimal` value. Throws `FormatException` or `InvalidCastException` if the conversion fails.
*   **`ToDouble(object? input)`**: Converts the provided input to a double-precision floating-point number. Throws `FormatException` or `InvalidCastException` if the conversion fails.
*   **`ToFloat(object? input)`**: Converts the provided input to a single-precision floating-point number. Throws `FormatException` or `InvalidCastException` if the conversion fails.
*   **`ToBoolean(object? input)`**: Converts the provided input to a `bool`. Recognizes standard string representations like "true"/"false" or numeric representations. Throws `FormatException` if unable to parse.
*   **`ToDateTime(object? input)`**: Attempts to parse the provided input into a `DateTime` object. Throws `FormatException` if the input string format is invalid.
*   **`ToGuid(object? input)`**: Converts the provided input to a `Guid` structure. Throws `FormatException` if the input is not a valid Guid string.
*   **`ToEnum<T>(string value)`**: Converts a string to an enum of type `T`. Returns `null` if the string does not match any defined enum member.
*   **`ToString(object? input)`**: Converts the input to its string representation, handling `null` inputs gracefully (typically returning `string.Empty` or `null`).
*   **`ToByteArray(object? input)`**: Converts input, typically a Base64 string or compatible format, into a `byte[]`.
*   **`JsonDeserialize<T>(string json)`**: Deserializes a JSON string into an object of type `T`. Throws `JsonException` if the JSON is malformed or incompatible with `T`.
*   **`JsonSerialize<T>(T obj)`**: Serializes the provided object into a JSON formatted string.
*   **`ToList<T>(object? input)`**: Attempts to convert an enumerable or collection into a `List<T>`. Throws `InvalidCastException` if conversion is not possible.
*   **`ObjectToDictionary(object? obj)`**: Converts an object's public properties into a `Dictionary<string, object?>` where the key is the property name and the value is the property value.

## Usage

```csharp
// Example 1: JSON serialization and enum conversion
var settings = new AppSettings { Theme = "Dark", Version = 1 };
string json = ConversionUtility.JsonSerialize(settings);

string statusInput = "Active";
var status = ConversionUtility.ToEnum<JobStatus>(statusInput);

// Example 2: Parsing configurations and object transformation
string rawValue = "123.45";
double threshold = ConversionUtility.ToDouble(rawValue);

var job = new ProcessingJob { Id = Guid.NewGuid(), Name = "VideoUpload" };
var jobData = ConversionUtility.ObjectToDictionary(job);
```

## Notes

*   **Thread Safety**: All methods in `ConversionUtility` are thread-safe, as they are static, stateless, and rely on underlying framework libraries (such as `System.Text.Json` or `System.Convert`) which are designed for concurrent use.
*   **Null Handling**: Input parameters of type `object?` are generally checked for `null`. Depending on the specific method, passing `null` may result in a default value, an empty string, or an `ArgumentNullException`.
*   **Performance**: While convenient, frequent use of `ObjectToDictionary` or JSON methods on large object graphs may incur performance overhead due to reflection and serialization costs. Use sparingly in performance-critical hot paths.
