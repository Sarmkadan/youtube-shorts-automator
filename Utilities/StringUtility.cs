// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// String manipulation and formatting utilities
/// Provides methods for truncation, slugification, and text transformation
/// </summary>
public static class StringUtility
{
    public static string Truncate(string text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string TruncateWords(string text, int wordCount, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= wordCount)
            return text;

        return string.Join(" ", words.Take(wordCount)) + suffix;
    }

    public static string ToSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Remove accents
        var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(slug);
        slug = Encoding.ASCII.GetString(bytes);

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, "-+", "-");

        // Trim hyphens from start and end
        return slug.Trim('-');
    }

    public static string ToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var words = text.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return text;

        var result = words[0].ToLowerInvariant();
        for (int i = 1; i < words.Length; i++)
        {
            result += char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
        }

        return result;
    }

    public static string ToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var camelCase = ToCamelCase(text);
        return char.ToUpperInvariant(camelCase[0]) + camelCase.Substring(1);
    }

    public static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var snake = Regex.Replace(text, "([a-z0-9])([A-Z])", "$1_$2");
        return snake.ToLowerInvariant();
    }

    public static bool IsNumeric(string text)
    {
        return !string.IsNullOrEmpty(text) && text.All(char.IsDigit);
    }

    public static bool IsAlphabetic(string text)
    {
        return !string.IsNullOrEmpty(text) && text.All(char.IsLetter);
    }

    public static bool IsAlphanumeric(string text)
    {
        return !string.IsNullOrEmpty(text) && text.All(char.IsLetterOrDigit);
    }

    public static string RemoveWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return Regex.Replace(text, @"\s+", "");
    }

    public static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return Regex.Replace(text.Trim(), @"\s+", " ");
    }

    public static string RemoveSpecialCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return Regex.Replace(text, @"[^a-zA-Z0-9\s-]", "");
    }

    public static int CountOccurrences(string text, string searchText)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchText))
            return 0;

        return (text.Length - text.Replace(searchText, "").Length) / searchText.Length;
    }

    public static string[] SplitByLength(string text, int length)
    {
        if (string.IsNullOrEmpty(text) || length <= 0)
            return Array.Empty<string>();

        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += length)
        {
            chunks.Add(text.Substring(i, Math.Min(length, text.Length - i)));
        }

        return chunks.ToArray();
    }

    public static string ReverseString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var chars = text.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public static bool ContainsAny(string text, params string[] values)
    {
        return !string.IsNullOrEmpty(text) && values.Any(v => text.Contains(v, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ContainsAll(string text, params string[] values)
    {
        return !string.IsNullOrEmpty(text) && values.All(v => text.Contains(v, StringComparison.OrdinalIgnoreCase));
    }
}
