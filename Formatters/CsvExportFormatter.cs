// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Formats data as CSV for export operations
/// Handles escaping, encoding, and dynamic property extraction
/// </summary>
public class CsvExportFormatter
{
    private readonly char _delimiter;
    private readonly Encoding _encoding;

    public CsvExportFormatter(char delimiter = ',', Encoding? encoding = null)
    {
        _delimiter = delimiter;
        _encoding = encoding ?? Encoding.UTF8;
    }

    public byte[] ExportToCsv<T>(IEnumerable<T> data, string[] columns) where T : class
    {
        var sb = new StringBuilder();

        // Write header
        sb.AppendLine(string.Join(_delimiter.ToString(), columns.Select(EscapeCsvField)));

        // Write data rows
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var item in data)
        {
            var values = columns.Select(col =>
            {
                var property = properties.FirstOrDefault(p => p.Name.Equals(col, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(item);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(_delimiter.ToString(), values));
        }

        return _encoding.GetBytes(sb.ToString());
    }

    public byte[] ExportToCsvWithAllProperties<T>(IEnumerable<T> data) where T : class
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        var columnNames = properties.Select(p => p.Name).ToArray();
        return ExportToCsv(data, columnNames);
    }

    public byte[] ExportDictionariesToCsv(IEnumerable<Dictionary<string, object>> data, string[] columns)
    {
        var sb = new StringBuilder();

        // Write header
        sb.AppendLine(string.Join(_delimiter.ToString(), columns.Select(EscapeCsvField)));

        // Write data rows
        foreach (var row in data)
        {
            var values = columns.Select(col =>
            {
                var hasValue = row.TryGetValue(col, out var value);
                return EscapeCsvField(hasValue ? value?.ToString() ?? string.Empty : string.Empty);
            });

            sb.AppendLine(string.Join(_delimiter.ToString(), values));
        }

        return _encoding.GetBytes(sb.ToString());
    }

    public string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains delimiter, quotes, or newlines, wrap in quotes and escape quotes
        if (field.Contains(_delimiter) || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    public string BuildCsvHeaderRow(string[] columns)
    {
        return string.Join(_delimiter.ToString(), columns.Select(EscapeCsvField));
    }

    public string BuildCsvDataRow(object[] values)
    {
        var fields = values.Select(v => EscapeCsvField(v?.ToString() ?? string.Empty));
        return string.Join(_delimiter.ToString(), fields);
    }
}
