# StringUtilityBenchmarks

`StringUtilityBenchmarks` is a performance benchmarking suite for common string transformation and sanitization utilities used throughout the `youtube-shorts-automator` project. It measures the execution time and memory allocation characteristics of operations such as truncation, casing conversions, slug generation, splitting, and whitespace normalization under realistic workloads.

## API

### Truncate

```csharp
public string Truncate(string value, int maxLength)
```

Truncates a string to the specified maximum length. If the input string is shorter than or equal to `maxLength`, it is returned unchanged. Otherwise, the string is cut at `maxLength` characters.

**Parameters:**
- `value` — The input string to truncate.
- `maxLength` — The maximum number of characters to retain.

**Return value:** A new string that is at most `maxLength` characters long.

**Throws:** `ArgumentOutOfRangeException` when `maxLength` is less than zero.

---

### ToCamelCase

```csharp
public string ToCamelCase(string value)
```

Converts a string to camelCase format. The first character is lowercased, and each subsequent word boundary (typically indicated by spaces, underscores, or hyphens) begins with an uppercase letter while all other characters are lowercased.

**Parameters:**
- `value` — The input string to convert.

**Return value:** The camelCase representation of the input string.

**Throws:** `ArgumentNullException` when `value` is `null`.

---

### ToPascalCase

```csharp
public string ToPascalCase(string value)
```

Converts a string to PascalCase format. Every word boundary begins with an uppercase letter, and all other characters are lowercased. Word boundaries are recognized at spaces, underscores, and hyphens.

**Parameters:**
- `value` — The input string to convert.

**Return value:** The PascalCase representation of the input string.

**Throws:** `ArgumentNullException` when `value` is `null`.

---

### ToSlug

```csharp
public string ToSlug(string value)
```

Generates a URL-friendly slug from the input string. Non-alphanumeric characters are replaced with hyphens, consecutive hyphens are collapsed into a single hyphen, and leading/trailing hyphens are removed. The result is lowercased.

**Parameters:**
- `value` — The input string to sluggify.

**Return value:** A lowercase, hyphen-separated slug string suitable for use in URLs.

**Throws:** `ArgumentNullException` when `value` is `null`.

---

### SplitByLength

```csharp
public string[] SplitByLength(string value, int chunkSize)
```

Splits a string into an array of substrings, each having a length of at most `chunkSize`. The last element may be shorter if the input length is not evenly divisible by `chunkSize`.

**Parameters:**
- `value` — The input string to split.
- `chunkSize` — The maximum length of each resulting substring.

**Return value:** An array of strings representing the chunks.

**Throws:**
- `ArgumentNullException` when `value` is `null`.
- `ArgumentOutOfRangeException` when `chunkSize` is less than or equal to zero.

---

### RemoveWhitespace

```csharp
public string RemoveWhitespace(string value)
```

Removes all whitespace characters from the input string, including spaces, tabs, newlines, and other Unicode whitespace characters. The remaining characters are concatenated in their original order.

**Parameters:**
- `value` — The input string to strip of whitespace.

**Return value:** A new string with all whitespace characters removed.

**Throws:** `ArgumentNullException` when `value` is `null`.

---

### NormalizeWhitespace

```csharp
public string NormalizeWhitespace(string value)
```

Collapses all sequences of whitespace characters into a single space and trims leading and trailing whitespace. Tabs, newlines, and multiple consecutive spaces are all reduced to one space character.

**Parameters:**
- `value` — The input string to normalize.

**Return value:** A string with whitespace normalized to single spaces and no leading or trailing whitespace.

**Throws:** `ArgumentNullException` when `value` is `null`.

## Usage

### Example 1: Preparing a Video Title for a Filename Slug

```csharp
var benchmarks = new StringUtilityBenchmarks();

string rawTitle = "  My Amazing   YouTube Short -- 2025 Edition!!!  ";
string normalized = benchmarks.NormalizeWhitespace(rawTitle);
// normalized: "My Amazing YouTube Short -- 2025 Edition!!!"

string slug = benchmarks.ToSlug(normalized);
// slug: "my-amazing-youtube-short-2025-edition"

string truncatedSlug = benchmarks.Truncate(slug, 50);
// truncatedSlug: "my-amazing-youtube-short-2025-edition" (unchanged, within limit)
```

### Example 2: Chunking a Description for Batch Processing

```csharp
var benchmarks = new StringUtilityBenchmarks();

string description = "This tutorial covers advanced C# string manipulation techniques for content automation pipelines.";
string pascal = benchmarks.ToPascalCase(description);
// pascal: "ThisTutorialCoversAdvancedCSharpStringManipulationTechniquesForContentAutomationPipelines"

string[] chunks = benchmarks.SplitByLength(pascal, 20);
// chunks[0]: "ThisTutorialCoversAd"
// chunks[1]: "vancedCSharpStringMa"
// chunks[2]: "nipulationTechniques"
// chunks[3]: "ForContentAutomation"
// chunks[4]: "Pipelines"

string noSpaces = benchmarks.RemoveWhitespace("  batch   process  ");
// noSpaces: "batchprocess"
```

## Notes

- All methods that accept a `string` parameter throw `ArgumentNullException` when passed `null`. Callers should guard against null inputs or handle the exception explicitly.
- `Truncate` does not append an ellipsis or any indicator that truncation occurred; it performs a simple length cutoff. Callers requiring visual truncation markers must add them separately.
- `ToCamelCase` and `ToPascalCase` treat any non-letter, non-digit character as a word boundary. Strings consisting entirely of symbols or numbers may produce unexpected results (e.g., an input of `"123_456"` yields `"123456"` for camelCase and `"123456"` for PascalCase).
- `ToSlug` strips diacritics and non-ASCII characters depending on the implementation; strings with heavy Unicode content may be reduced more aggressively than expected.
- `SplitByLength` returns an empty array when the input string is empty. It does not pad the final chunk.
- `RemoveWhitespace` removes all Unicode whitespace categories, including zero-width spaces and other invisible separators, not just ASCII space and tab characters.
- `NormalizeWhitespace` treats any Unicode whitespace as equivalent; a tab followed by a newline collapses to a single space, identical to how multiple spaces are handled.
- All methods are stateless and thread-safe. They operate purely on their input parameters without shared mutable state, making them safe for concurrent invocation from multiple threads without synchronization.
