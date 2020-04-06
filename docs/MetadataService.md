# MetadataService

The `MetadataService` class provides validation and sanitization utilities for YouTube Shorts metadata, ensuring that titles, descriptions, and tags comply with platform constraints before upload. It acts as a preprocessing layer to prevent upload failures caused by invalid characters, excessive lengths, or malformed content, offering both individual field processors and a consolidated validation method that returns a structured report of the metadata state.

## API

### `public MetadataService`
Initializes a new instance of the `MetadataService` class. This constructor requires no parameters and sets up the internal configuration needed for validation rules and sanitization patterns.

### `public bool ValidateTitle(string title)`
Validates a video title against length limits and character restrictions.
- **Parameters**: `title` – The raw title string to validate.
- **Returns**: `true` if the title meets all criteria; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `title` is null.

### `public bool ValidateDescription(string description)`
Validates a video description for length constraints and prohibited content patterns.
- **Parameters**: `description` – The raw description string to validate.
- **Returns**: `true` if the description is valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `description` is null.

### `public bool ValidateTags(string[] tags)`
Validates an array of tags, ensuring each tag meets length requirements and contains only allowed characters.
- **Parameters**: `tags` – The array of tag strings to validate.
- **Returns**: `true` if all tags in the array are valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `tags` is null or if any element within the array is null.

### `public string SanitizeTitle(string title)`
Cleans a title by removing invalid characters and trimming excess whitespace while preserving core content.
- **Parameters**: `title` – The raw title string to sanitize.
- **Returns**: A sanitized string safe for use as a YouTube title.
- **Throws**: `ArgumentNullException` if `title` is null.

### `public string SanitizeDescription(string description)`
Cleans a description by stripping forbidden characters and normalizing line breaks.
- **Parameters**: `description` – The raw description string to sanitize.
- **Returns**: A sanitized string safe for use as a YouTube description.
- **Throws**: `ArgumentNullException` if `description` is null.

### `public string[] SanitizeTags(string[] tags)`
Processes an array of tags, removing invalid characters from each entry and filtering out empty results.
- **Parameters**: `tags` – The raw array of tags to sanitize.
- **Returns**: A new array containing only sanitized, non-empty tags.
- **Throws**: `ArgumentNullException` if `tags` is null.

### `public Dictionary<string, string> ValidateMetadata(string title, string description, string[] tags)`
Performs a comprehensive validation and sanitization pass on all metadata fields simultaneously.
- **Parameters**: 
  - `title` – The video title.
  - `description` – The video description.
  - `tags` – The array of tags.
- **Returns**: A dictionary where keys represent field names ("Title", "Description", "Tags") and values contain the sanitized content or error messages if validation fails.
- **Throws**: `ArgumentNullException` if any input parameter is null.

## Usage

### Example 1: Individual Field Validation and Sanitization
This example demonstrates validating and sanitizing fields independently before constructing a metadata object.

```csharp
var service = new MetadataService();
string rawTitle = "  Amazing Short! #viral  ";
string rawDescription = "Check this out!!! http://spam-link.com";
string[] rawTags = { "funny", "cat<>video", "" };

if (service.ValidateTitle(rawTitle) && service.ValidateDescription(rawDescription))
{
    string cleanTitle = service.SanitizeTitle(rawTitle);
    string cleanDescription = service.SanitizeDescription(rawDescription);
    string[] cleanTags = service.SanitizeTags(rawTags);

    // Proceed with upload logic using cleanTitle, cleanDescription, and cleanTags
}
```

### Example 2: Bulk Validation Report
This example uses the consolidated method to generate a report of all metadata fields, handling errors and sanitized outputs in a single step.

```csharp
var service = new MetadataService();
var report = service.ValidateMetadata(
    "Top 10 Moments", 
    "A compilation of the best scenes.", 
    new[] { "compilation", "top10" }
);

if (report.ContainsKey("Title") && !report["Title"].StartsWith("ERROR:"))
{
    string finalTitle = report["Title"];
    string finalDescription = report["Description"];
    // Parse final tags from report if necessary based on implementation details
}
```

## Notes

- **Null Handling**: All public methods explicitly throw `ArgumentNullException` when passed null inputs. Callers must ensure strings and arrays are instantiated before invocation.
- **Thread Safety**: The `MetadataService` class maintains no internal mutable state between method calls; instances are thread-safe and can be shared across concurrent threads without locking.
- **Empty Results**: The `SanitizeTags` method may return an array with fewer elements than the input if certain tags become empty after sanitization. It will not return null, but an empty array is possible if all input tags are invalid.
- **Validation vs. Sanitization**: A return value of `false` from validation methods indicates the input is fundamentally rejected (e.g., too long even after potential trimming), whereas sanitization methods attempt to coerce the input into a valid format. Relying solely on sanitization without prior validation may result in data loss.
