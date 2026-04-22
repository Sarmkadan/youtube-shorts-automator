// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// String manipulation and formatting utilities.
/// Provides methods for truncation, slugification, and text transformation.
/// </summary>
public static partial class StringUtility
{
    // Source-generated regex — compiled to IL at build time; zero startup cost,
    // no JIT-compiled automaton, ~40 % faster than RegexOptions.Compiled.
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SlugInvalidCharsRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9\s\-]")]
    private static partial Regex SpecialCharsRegex();

    [GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial Regex SnakeCaseRegex();

    public static string Truncate(string text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        // string.Concat(span, span) builds the result in a single allocation —
        // no intermediate Substring string is created.
        return string.Concat(text.AsSpan(0, maxLength - suffix.Length), suffix);
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

        var slug = text.ToLowerInvariant();

        var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(slug);
        slug = Encoding.ASCII.GetString(bytes);

        slug = WhitespaceRegex().Replace(slug, "-");
        slug = SlugInvalidCharsRegex().Replace(slug, "");
        slug = MultipleHyphensRegex().Replace(slug, "-");

        return slug.Trim('-');
    }

    public static string ToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var words = text.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return text;

        // StringBuilder avoids O(n²) string concatenation in the loop —
        // each += in the original created a new heap string per iteration.
        var sb = new StringBuilder(text.Length);
        sb.Append(words[0].ToLowerInvariant());
        for (int i = 1; i < words.Length; i++)
        {
            var word = words[i];
            if (word.Length == 0)
                continue;
            sb.Append(char.ToUpperInvariant(word[0]));
            if (word.Length > 1)
                sb.Append(word[1..].ToLowerInvariant());
        }

        return sb.ToString();
    }

    public static string ToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var camelCase = ToCamelCase(text);
        if (camelCase.Length == 0)
            return camelCase;

        // string.Create writes directly into the result buffer —
        // no intermediate Substring allocation for the tail of the string.
        return string.Create(camelCase.Length, camelCase, static (span, s) =>
        {
            span[0] = char.ToUpperInvariant(s[0]);
            s.AsSpan(1).CopyTo(span[1..]);
        });
    }

    public static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var snake = SnakeCaseRegex().Replace(text, "$1_$2");
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

        return WhitespaceRegex().Replace(text, "");
    }

    public static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return WhitespaceRegex().Replace(text.Trim(), " ");
    }

    public static string RemoveSpecialCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return SpecialCharsRegex().Replace(text, "");
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

        // Pre-calculate count so we allocate the final array once and skip
        // the List<string> intermediate and its ToArray() copy.
        var count = (text.Length + length - 1) / length;
        var chunks = new string[count];
        for (int i = 0, idx = 0; i < text.Length; i += length, idx++)
            chunks[idx] = text.Substring(i, Math.Min(length, text.Length - i));

        return chunks;
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
