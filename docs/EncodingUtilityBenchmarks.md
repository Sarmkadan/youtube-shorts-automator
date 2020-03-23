# EncodingUtilityBenchmarks

A utility class providing high-performance encoding and hashing operations commonly used in data processing pipelines, such as generating checksums, encoding binary data for safe transmission, and producing random identifiers.

## API

### `Sha256Hash`
Computes the SHA-256 hash of the input string and returns the hexadecimal representation.

- **Parameters**: `string input` – The string to hash.
- **Return value**: A 64-character hexadecimal string representing the SHA-256 hash.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `Md5Hash`
Computes the MD5 hash of the input string and returns the hexadecimal representation.

- **Parameters**: `string input` – The string to hash.
- **Return value**: A 32-character hexadecimal string representing the MD5 hash.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `EncodeBase64`
Encodes a string into Base64, including padding characters if required.

- **Parameters**: `string input` – The string to encode.
- **Return value**: A Base64-encoded string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `DecodeBase64`
Decodes a Base64-encoded string back into its original form.

- **Parameters**: `string input` – The Base64-encoded string to decode.
- **Return value**: The decoded string.
- **Throws**:
  - `ArgumentNullException` if `input` is `null`.
  - `FormatException` if `input` is not a valid Base64 string.

### `GenerateRandomString`
Generates a cryptographically secure random alphanumeric string of the specified length.

- **Parameters**: `int length` – The desired length of the output string.
- **Return value**: A random alphanumeric string of length `length`.
- **Throws**: `ArgumentOutOfRangeException` if `length` is less than zero.

### `GenerateRandomHexString`
Generates a cryptographically secure random hexadecimal string of the specified length.

- **Parameters**: `int length` – The desired length of the output string. Must be even.
- **Return value**: A random hexadecimal string of length `length`.
- **Throws**:
  - `ArgumentOutOfRangeException` if `length` is negative or odd.

## Usage
