# EncodingUtility

The `EncodingUtility` class is a static service provider that encapsulates commonly used methods for encoding, decoding, hashing, string randomization, and URL/Query string manipulation. It is designed to facilitate consistent and secure data processing across the `youtube-shorts-automator` application by centralizing boilerplate transformation logic.

## API

### Base64
*   **`EncodeBase64(string input)`**
    *   **Purpose:** Converts a UTF-8 string to a Base64 encoded string.
    *   **Returns:** The Base64 encoded representation of the input.
*   **`DecodeBase64(string base64EncodedData)`**
    *   **Purpose:** Decodes a Base64 string back to its original UTF-8 string format.
    *   **Returns:** The decoded string.
    *   **Throws:** `FormatException` if the input is not a valid Base64 string.

### Hashing
*   **`ComputeSha256Hash(string input)`**
    *   **Purpose:** Computes the SHA-256 hash of a string.
    *   **Returns:** A hex-encoded SHA-256 hash string.
*   **`ComputeSha256Hash(byte[] input)`**
    *   **Purpose:** Computes the SHA-256 hash of a byte array.
    *   **Returns:** A hex-encoded SHA-256 hash string.
*   **`ComputeMd5Hash(string input)`**
    *   **Purpose:** Computes the MD5 hash of a string.
    *   **Returns:** A hex-encoded MD5 hash string.
*   **`VerifyHash(string input, string hash)`**
    *   **Purpose:** Verifies if an input string matches a provided hash value.
    *   **Returns:** `true` if the input, when hashed, matches the provided hash; otherwise `false`.

### Web Encoding
*   **`UrlEncode(string input)`**
    *   **Purpose:** Encodes a string for safe use in a URL.
    *   **Returns:** A URL-encoded string.
*   **`UrlDecode(string input)`**
    *   **Purpose:** Decodes a URL-encoded string.
    *   **Returns:** A decoded string.
*   **`HtmlEncode(string input)`**
    *   **Purpose:** Encodes a string to be safely rendered as HTML.
    *   **Returns:** An HTML-encoded string.
*   **`HtmlDecode(string input)`**
    *   **Purpose:** Decodes an HTML-encoded string.
    *   **Returns:** A decoded string.

### Query Strings
*   **`ParseQueryString(string queryString)`**
    *   **Purpose:** Parses a URL query string into a dictionary of key-value pairs.
    *   **Returns:** A `Dictionary<string, string>` representing the query parameters.
*   **`BuildQueryString(Dictionary<string, string> parameters)`**
    *   **Purpose:** Constructs a URL query string from a dictionary of parameters.
    *   **Returns:** A formatted query string.

### Randomization
*   **`GenerateRandomString(int length)`**
    *   **Purpose:** Generates a cryptographically strong random alphanumeric string.
    *   **Parameters:** `length` - The desired length of the string.
    *   **Returns:** A random string.
*   **`GenerateRandomHexString(int length)`**
    *   **Purpose:** Generates a cryptographically strong random hexadecimal string.
    *   **Parameters:** `length` - The number of characters in the hex string.
    *   **Returns:** A random hexadecimal string.
*   **`GenerateSecureGuid()`**
    *   **Purpose:** Generates a cryptographically secure `Guid`.
    *   **Returns:** A new `Guid`.

## Usage

### Hashing and Base64 Encoding
```csharp
string originalData = "sensitive-content";
string hash = EncodingUtility.ComputeSha256Hash(originalData);

string encoded = EncodingUtility.EncodeBase64(originalData);
string decoded = EncodingUtility.DecodeBase64(encoded);

// Verify integrity
bool isValid = EncodingUtility.VerifyHash(originalData, hash);
```

### URL Query String Manipulation
```csharp
var parameters = new Dictionary<string, string>
{
    { "videoId", "abc12345" },
    { "format", "mp4" }
};

// Build: "videoId=abc12345&format=mp4"
string queryString = EncodingUtility.BuildQueryString(parameters);

// Parse back
var parsed = EncodingUtility.ParseQueryString(queryString);
```

## Notes

*   **Thread Safety:** The `EncodingUtility` class is entirely static and stateless. All methods are thread-safe and can be safely called concurrently from multiple threads.
*   **Input Validation:** While the methods handle basic null checks, consumers should ensure that input passed to decoding and parsing methods (such as `DecodeBase64` or `ParseQueryString`) adheres to the expected formats to avoid `ArgumentException` or `FormatException`.
*   **Performance:** Hashing methods, particularly SHA-256, are computationally intensive. They should be used judiciously in high-throughput or latency-sensitive paths.
*   **Encoding Standards:** Methods involving text transformation assume UTF-8 encoding unless otherwise specified by the underlying .NET implementation being utilized.
