# CsvExportFormatter

The `CsvExportFormatter` class provides utility methods for converting collections of objects and dictionaries into Comma-Separated Values (CSV) format. It facilitates data export operations by managing field escaping, header construction, and data row generation, ensuring the output conforms to standard CSV formatting requirements suitable for file persistence or web responses.

## API

### CsvExportFormatter()
Initializes a new instance of the `CsvExportFormatter` class.

### byte[] ExportToCsv<T>(IEnumerable<T> data)
Converts a collection of objects of type `T` into a CSV-encoded byte array. This method uses reflection to automatically map public properties of `T` to CSV columns.
*   **Parameters:** `data` - The collection of objects to export.
*   **Returns:** A `byte[]` representing the CSV content.
*   **Throws:** `ArgumentNullException` if `data` is null.

### byte[] ExportToCsvWithAllProperties<T>(IEnumerable<T> data)
Converts a collection of objects of type `T` into a CSV-encoded byte array, including all available properties of the type, regardless of standard filtering logic.
*   **Parameters:** `data` - The collection of objects to export.
*   **Returns:** A `byte[]` representing the CSV content.
*   **Throws:** `ArgumentNullException` if `data` is null.

### byte[] ExportDictionariesToCsv(IEnumerable<Dictionary<string, string>> data)
Converts a collection of dictionaries, where each dictionary represents a row of data, into a CSV-encoded byte array. Keys are treated as header names and values as row data.
*   **Parameters:** `data` - The collection of dictionaries to export.
*   **Returns:** A `byte[]` representing the CSV content.
*   **Throws:** `ArgumentNullException` if `data` is null.

### string EscapeCsvField(string field)
Processes a string to ensure it is valid within a CSV field, wrapping it in quotes and escaping existing quotes if the field contains delimiters, newlines, or quotes.
*   **Parameters:** `field` - The raw string to escape.
*   **Returns:** The escaped CSV-compliant string.

### string BuildCsvHeaderRow(IEnumerable<string> columns)
Constructs a single CSV-formatted header row string from a provided collection of column names.
*   **Parameters:** `columns` - An enumerable of strings representing the column names.
*   **Returns:** A comma-separated string representing the CSV header row.

### string BuildCsvDataRow(IEnumerable<string> values)
Constructs a single CSV-formatted data row string from a provided collection of field values, ensuring each value is correctly escaped.
*   **Parameters:** `values` - An enumerable of strings representing the data values.
*   **Returns:** A comma-separated string representing the CSV data row.

## Usage

### Exporting an object collection
```csharp
var formatter = new CsvExportFormatter();
var videos = new List<Video> { new Video("Title1", 120), new Video("Title2", 300) };

byte[] csvData = formatter.ExportToCsv(videos);
// csvData can now be written to a file or returned via HTTP
```

### Manually building a CSV row
```csharp
var formatter = new CsvExportFormatter();
var headers = new List<string> { "Name", "Description" };
var data = new List<string> { "Item1", "Description with, comma" };

string csvRow = formatter.BuildCsvHeaderRow(headers) + "\n" + formatter.BuildCsvDataRow(data);
```

## Notes

*   **Thread-Safety:** The methods in this class are stateless and considered thread-safe. They can be safely utilized from multiple threads simultaneously.
*   **Character Encoding:** The `ExportTo` methods return a `byte[]`, typically encoded using UTF-8. Ensure that consumers of this output expect this encoding.
*   **Handling Nulls:** When exporting objects, null properties may be treated as empty strings in the generated CSV. Ensure your data model is sanitized if specific behavior for null values is required.
*   **Data Size:** For extremely large collections, exporting to a `byte[]` may cause high memory consumption. Consider streaming approaches if the expected data volume exceeds available heap space.
