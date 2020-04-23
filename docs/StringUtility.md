# StringUtility

The `StringUtility` class provides a centralized collection of static methods for common string manipulation, formatting, and validation tasks. This utility is designed to ensure consistency across the `youtube-shorts-automator` project by providing standardized implementations for string processing requirements.

## API

*   **`string Truncate(string input, int maxLength)`**
    Truncates the provided string to the specified maximum length. If the input string is shorter than or equal to `maxLength`, it is returned unchanged.
*   **`string TruncateWords(string input, int wordCount)`**
    Truncates the input string to the specified number of words, preserving word integrity.
*   **`string ToSlug(string input)`**
    Converts the input string into a URL-friendly slug, typically by converting to lowercase and replacing non-alphanumeric characters with hyphens.
*   **`string ToCamelCase(string input)`**
    Converts the input string to camelCase format.
*   **`string ToPascalCase(string input)`**
    Converts the input string to PascalCase format.
*   **`string ToSnakeCase(string input)`**
    Converts the input string to snake_case format.
*   **`bool IsNumeric(string input)`**
    Returns `true` if the input string consists entirely of numeric characters; otherwise, `false`.
*   **`bool IsAlphabetic(string input)`**
    Returns `true` if the input string consists entirely of alphabetic characters; otherwise, `false`.
*   **`bool IsAlphanumeric(string input)`**
    Returns `true` if the input string consists entirely of alphanumeric characters; otherwise, `false`.
*   **`string RemoveWhitespace(string input)`**
    Removes all whitespace characters (spaces, tabs, newlines) from the input string.
*   **`string NormalizeWhitespace(string input)`**
    Collapses multiple consecutive whitespace characters into a single space and trims whitespace from the start and end of the string.
*   **`string RemoveSpecialCharacters(string input)`**
    Removes all characters that are not alphanumeric from the string.
*   **`int CountOccurrences(string input, string substring)`**
    Returns the count of how many times the specified `substring` appears within the `input` string.
*   **`string[] SplitByLength(string input, int length)`**
    Splits the input string into an array of substrings, each with the specified `length`.
*   **`string ReverseString(string input)`**
    Returns a new string with the characters of the input string in reverse order.
*   **`bool ContainsAny(string input, IEnumerable<string> values)`**
    Returns `true` if the input string contains at least one of the substrings present in the `values` collection.
*   **`bool ContainsAll(string input, IEnumerable<string> values)`**
    Returns `true` if the input string contains all of the substrings present in the `values` collection.

## Usage

```csharp
// Example 1: Sanitizing a title for use in a file system path
string rawTitle = "My Awesome Video! (Part 1)";
string sanitizedTitle = StringUtility.ToSlug(rawTitle);
// Result: "my-awesome-video-part-1"

// Example 2: Normalizing a description for consistent formatting
string rawDescription = "This   video   has   inconsistent    spacing.";
string cleanDescription = StringUtility.NormalizeWhitespace(rawDescription);
// Result: "This video has inconsistent spacing."
```

## Notes

*   **Null Handling**: Unless otherwise specified for a specific method, passing `null` as the `input` parameter may result in an `ArgumentNullException` or return `null`/empty depending on the internal implementation. It is recommended to validate input before calling these methods.
*   **Thread Safety**: All methods within `StringUtility` are `static` and do not maintain internal state. They are safe to be called concurrently from multiple threads.
*   **Performance**: These methods are optimized for general use cases. For high-throughput scenarios involving extremely large strings or frequent, large-volume string manipulations, consider using `StringBuilder` or span-based operations directly if the overhead of creating new strings becomes a bottleneck.
