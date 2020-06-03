// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for CsvExportFormatter providing additional CSV export functionality
//
// This file adds practical extension methods that build upon the existing formatter
// without duplicating its core functionality.
// =============================================================================

using System.Globalization;
using System.Reflection;
using System.Text;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Extension methods for <see cref="CsvExportFormatter"/> providing additional CSV export functionality
/// </summary>
public static class CsvExportFormatterExtensions
{
    /// <summary>
    /// Exports data to CSV with custom formatting options including header inclusion control
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="formatter">The formatter instance</param>
    /// <param name="data">Data to export</param>
    /// <param name="columns">Columns to include in the export</param>
    /// <param name="includeHeader">Whether to include header row in output</param>
    /// <returns>CSV data as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when data or columns is null</exception>
    public static byte[] ExportToCsv<T>(this CsvExportFormatter formatter, IEnumerable<T> data, string[] columns, bool includeHeader) where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(columns);

        if (columns.Length == 0)
            throw new ArgumentException("Columns array cannot be empty", nameof(columns));

        var sb = new StringBuilder();

        if (includeHeader)
        {
            sb.AppendLine(formatter.BuildCsvHeaderRow(columns));
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var item in data)
        {
            var values = columns.Select(col =>
            {
                var property = properties.FirstOrDefault(p => p.Name.Equals(col, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(item);
                return formatter.EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(formatter.GetDelimiter().ToString(), values));
        }

        return formatter.GetEncoding().GetBytes(sb.ToString());
    }

    /// <summary>
    /// Exports data to CSV with custom delimiter and encoding settings
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="formatter">The formatter instance</param>
    /// <param name="data">Data to export</param>
    /// <param name="columns">Columns to include in the export</param>
    /// <param name="delimiter">Custom delimiter character to use</param>
    /// <param name="encoding">Custom encoding to use</param>
    /// <returns>CSV data as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when data or columns is null</exception>
    public static byte[] ExportToCsv<T>(this CsvExportFormatter formatter, IEnumerable<T> data, string[] columns, char delimiter, Encoding? encoding = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(columns);

        if (columns.Length == 0)
            throw new ArgumentException("Columns array cannot be empty", nameof(columns));

        // Create a new formatter with custom settings for this export
        var customFormatter = new CsvExportFormatter(delimiter, encoding ?? formatter.GetEncoding());

        var sb = new StringBuilder();
        sb.AppendLine(customFormatter.BuildCsvHeaderRow(columns));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var item in data)
        {
            var values = columns.Select(col =>
            {
                var property = properties.FirstOrDefault(p => p.Name.Equals(col, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(item);
                return customFormatter.EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(customFormatter.GetDelimiter().ToString(), values));
        }

        return customFormatter.GetEncoding().GetBytes(sb.ToString());
    }

    /// <summary>
    /// Exports data to CSV with culture-specific number formatting
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="formatter">The formatter instance</param>
    /// <param name="data">Data to export</param>
    /// <param name="columns">Columns to include in the export</param>
    /// <param name="cultureInfo">Culture to use for number formatting</param>
    /// <returns>CSV data as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when data, columns, or cultureInfo is null</exception>
    public static byte[] ExportToCsv<T>(this CsvExportFormatter formatter, IEnumerable<T> data, string[] columns, CultureInfo cultureInfo) where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        if (columns.Length == 0)
            throw new ArgumentException("Columns array cannot be empty", nameof(columns));

        var sb = new StringBuilder();
        sb.AppendLine(formatter.BuildCsvHeaderRow(columns));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var item in data)
        {
            var values = columns.Select(col =>
            {
                var property = properties.FirstOrDefault(p => p.Name.Equals(col, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                    return string.Empty;

                var value = property.GetValue(item);
                if (value != null && double.TryParse(value.ToString(), out var number))
                {
                    return formatter.EscapeCsvField(number.ToString(cultureInfo));
                }

                return formatter.EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(formatter.GetDelimiter().ToString(), values));
        }

        return formatter.GetEncoding().GetBytes(sb.ToString());
    }

    /// <summary>
    /// Builds a CSV row from a dictionary of values with automatic escaping
    /// </summary>
    /// <param name="formatter">The formatter instance</param>
    /// <param name="values">Dictionary of column names to values</param>
    /// <returns>Properly escaped and formatted CSV row</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    public static string BuildCsvDataRow(this CsvExportFormatter formatter, Dictionary<string, object> values)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(values);

        var orderedValues = values.OrderBy(kvp => kvp.Key)
            .Select(kvp => formatter.EscapeCsvField(kvp.Value?.ToString() ?? string.Empty))
            .ToArray();

        return string.Join(formatter.GetDelimiter().ToString(), orderedValues);
    }

    /// <summary>
    /// Gets the delimiter character used by this formatter instance
    /// </summary>
    /// <param name="formatter">The formatter instance</param>
    /// <returns>The delimiter character</returns>
    /// <exception cref="ArgumentNullException">Thrown when formatter is null</exception>
    public static char GetDelimiter(this CsvExportFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var field = typeof(CsvExportFormatter).GetField("_delimiter", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            throw new InvalidOperationException("Delimiter field not found in CsvExportFormatter");

        return (char)field.GetValue(formatter)!;
    }

    /// <summary>
    /// Gets the encoding used by this formatter instance
    /// </summary>
    /// <param name="formatter">The formatter instance</param>
    /// <returns>The encoding instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when formatter is null</exception>
    public static Encoding GetEncoding(this CsvExportFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var field = typeof(CsvExportFormatter).GetField("_encoding", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            throw new InvalidOperationException("Encoding field not found in CsvExportFormatter");

        return (Encoding)field.GetValue(formatter)!;
    }

    /// <summary>
    /// Exports a collection of objects to CSV with automatic column detection and culture-aware formatting
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="formatter">The formatter instance</param>
    /// <param name="data">Data to export</param>
    /// <param name="cultureInfo">Culture to use for number formatting</param>
    /// <returns>CSV data as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when data or cultureInfo is null</exception>
    public static byte[] ExportToCsvWithCulture<T>(this CsvExportFormatter formatter, IEnumerable<T> data, CultureInfo cultureInfo) where T : class
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        var columnNames = properties.Select(p => p.Name).ToArray();

        return formatter.ExportToCsv(data, columnNames, cultureInfo);
    }
}