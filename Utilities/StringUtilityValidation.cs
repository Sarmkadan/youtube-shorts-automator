// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation helpers for StringUtility operations
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for StringUtility operations.
/// Validates StringUtility behavior and output constraints.
/// </summary>
public static class StringUtilityValidation
{
    /// <summary>
    /// Validates StringUtility behavior and returns a list of human-readable problems.
    /// Checks for potential issues in StringUtility methods and their outputs.
    /// </summary>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate Truncate behavior
        // Truncate should handle null/empty input gracefully
        var truncateNull = StringUtility.Truncate(null, 10);
        if (truncateNull != null)
        {
            problems.Add("StringUtility.Truncate(null, maxLength) should return null, but returned non-null value.");
        }

        var truncateEmpty = StringUtility.Truncate(string.Empty, 10);
        if (truncateEmpty != string.Empty)
        {
            problems.Add("StringUtility.Truncate(string.Empty, maxLength) should return string.Empty, but returned non-empty value.");
        }

        // Validate TruncateWords behavior
        var truncateWordsNull = StringUtility.TruncateWords(null, 5);
        if (truncateWordsNull != null)
        {
            problems.Add("StringUtility.TruncateWords(null, wordCount) should return null, but returned non-null value.");
        }

        var truncateWordsEmpty = StringUtility.TruncateWords(string.Empty, 5);
        if (truncateWordsEmpty != string.Empty)
        {
            problems.Add("StringUtility.TruncateWords(string.Empty, wordCount) should return string.Empty, but returned non-empty value.");
        }

        // Validate ToSlug behavior - should handle null/empty/whitespace
        var toSlugNull = StringUtility.ToSlug(null);
        if (toSlugNull != string.Empty)
        {
            problems.Add("StringUtility.ToSlug(null) should return string.Empty, but returned non-empty value.");
        }

        var toSlugEmpty = StringUtility.ToSlug(string.Empty);
        if (toSlugEmpty != string.Empty)
        {
            problems.Add("StringUtility.ToSlug(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        var toSlugWhitespace = StringUtility.ToSlug("   ");
        if (toSlugWhitespace != string.Empty)
        {
            problems.Add("StringUtility.ToSlug(whitespace) should return string.Empty, but returned non-empty value.");
        }

        // Validate ToCamelCase behavior
        var toCamelCaseNull = StringUtility.ToCamelCase(null);
        if (toCamelCaseNull != null)
        {
            problems.Add("StringUtility.ToCamelCase(null) should return null, but returned non-null value.");
        }

        var toCamelCaseEmpty = StringUtility.ToCamelCase(string.Empty);
        if (toCamelCaseEmpty != string.Empty)
        {
            problems.Add("StringUtility.ToCamelCase(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate ToPascalCase behavior
        var toPascalCaseNull = StringUtility.ToPascalCase(null);
        if (toPascalCaseNull != null)
        {
            problems.Add("StringUtility.ToPascalCase(null) should return null, but returned non-null value.");
        }

        var toPascalCaseEmpty = StringUtility.ToPascalCase(string.Empty);
        if (toPascalCaseEmpty != string.Empty)
        {
            problems.Add("StringUtility.ToPascalCase(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate ToSnakeCase behavior
        var toSnakeCaseNull = StringUtility.ToSnakeCase(null);
        if (toSnakeCaseNull != null)
        {
            problems.Add("StringUtility.ToSnakeCase(null) should return null, but returned non-null value.");
        }

        var toSnakeCaseEmpty = StringUtility.ToSnakeCase(string.Empty);
        if (toSnakeCaseEmpty != string.Empty)
        {
            problems.Add("StringUtility.ToSnakeCase(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate IsNumeric behavior
        var isNumericNull = StringUtility.IsNumeric(null);
        if (isNumericNull)
        {
            problems.Add("StringUtility.IsNumeric(null) should return false, but returned true.");
        }

        var isNumericEmpty = StringUtility.IsNumeric(string.Empty);
        if (isNumericEmpty)
        {
            problems.Add("StringUtility.IsNumeric(string.Empty) should return false, but returned true.");
        }

        // Validate IsAlphabetic behavior
        var isAlphabeticNull = StringUtility.IsAlphabetic(null);
        if (isAlphabeticNull)
        {
            problems.Add("StringUtility.IsAlphabetic(null) should return false, but returned true.");
        }

        var isAlphabeticEmpty = StringUtility.IsAlphabetic(string.Empty);
        if (isAlphabeticEmpty)
        {
            problems.Add("StringUtility.IsAlphabetic(string.Empty) should return false, but returned true.");
        }

        // Validate IsAlphanumeric behavior
        var isAlphanumericNull = StringUtility.IsAlphanumeric(null);
        if (isAlphanumericNull)
        {
            problems.Add("StringUtility.IsAlphanumeric(null) should return false, but returned true.");
        }

        var isAlphanumericEmpty = StringUtility.IsAlphanumeric(string.Empty);
        if (isAlphanumericEmpty)
        {
            problems.Add("StringUtility.IsAlphanumeric(string.Empty) should return false, but returned true.");
        }

        // Validate RemoveWhitespace behavior
        var removeWhitespaceNull = StringUtility.RemoveWhitespace(null);
        if (removeWhitespaceNull != null)
        {
            problems.Add("StringUtility.RemoveWhitespace(null) should return null, but returned non-null value.");
        }

        var removeWhitespaceEmpty = StringUtility.RemoveWhitespace(string.Empty);
        if (removeWhitespaceEmpty != string.Empty)
        {
            problems.Add("StringUtility.RemoveWhitespace(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate NormalizeWhitespace behavior
        var normalizeWhitespaceNull = StringUtility.NormalizeWhitespace(null);
        if (normalizeWhitespaceNull != null)
        {
            problems.Add("StringUtility.NormalizeWhitespace(null) should return null, but returned non-null value.");
        }

        var normalizeWhitespaceEmpty = StringUtility.NormalizeWhitespace(string.Empty);
        if (normalizeWhitespaceEmpty != string.Empty)
        {
            problems.Add("StringUtility.NormalizeWhitespace(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate RemoveSpecialCharacters behavior
        var removeSpecialCharsNull = StringUtility.RemoveSpecialCharacters(null);
        if (removeSpecialCharsNull != null)
        {
            problems.Add("StringUtility.RemoveSpecialCharacters(null) should return null, but returned non-null value.");
        }

        var removeSpecialCharsEmpty = StringUtility.RemoveSpecialCharacters(string.Empty);
        if (removeSpecialCharsEmpty != string.Empty)
        {
            problems.Add("StringUtility.RemoveSpecialCharacters(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate CountOccurrences behavior
        var countOccurrencesNullText = StringUtility.CountOccurrences(null, "test");
        if (countOccurrencesNullText != 0)
        {
            problems.Add("StringUtility.CountOccurrences(null, searchText) should return 0, but returned non-zero value.");
        }

        var countOccurrencesNullSearch = StringUtility.CountOccurrences("test text", null);
        if (countOccurrencesNullSearch != 0)
        {
            problems.Add("StringUtility.CountOccurrences(text, null) should return 0, but returned non-zero value.");
        }

        var countOccurrencesEmptyText = StringUtility.CountOccurrences(string.Empty, "test");
        if (countOccurrencesEmptyText != 0)
        {
            problems.Add("StringUtility.CountOccurrences(string.Empty, searchText) should return 0, but returned non-zero value.");
        }

        var countOccurrencesEmptySearch = StringUtility.CountOccurrences("test text", string.Empty);
        if (countOccurrencesEmptySearch != 0)
        {
            problems.Add("StringUtility.CountOccurrences(text, string.Empty) should return 0, but returned non-zero value.");
        }

        // Validate SplitByLength behavior
        var splitByLengthNull = StringUtility.SplitByLength(null, 5);
        if (splitByLengthNull == null || splitByLengthNull.Length != 0)
        {
            problems.Add("StringUtility.SplitByLength(null, length) should return empty array, but returned non-empty array.");
        }

        var splitByLengthEmpty = StringUtility.SplitByLength(string.Empty, 5);
        if (splitByLengthEmpty == null || splitByLengthEmpty.Length != 0)
        {
            problems.Add("StringUtility.SplitByLength(string.Empty, length) should return empty array, but returned non-empty array.");
        }

        var splitByLengthInvalidLength = StringUtility.SplitByLength("test", 0);
        if (splitByLengthInvalidLength == null || splitByLengthInvalidLength.Length != 0)
        {
            problems.Add("StringUtility.SplitByLength(text, 0) should return empty array, but returned non-empty array.");
        }

        var splitByLengthNegativeLength = StringUtility.SplitByLength("test", -1);
        if (splitByLengthNegativeLength == null || splitByLengthNegativeLength.Length != 0)
        {
            problems.Add("StringUtility.SplitByLength(text, negativeLength) should return empty array, but returned non-empty array.");
        }

        // Validate ReverseString behavior
        var reverseStringNull = StringUtility.ReverseString(null);
        if (reverseStringNull != null)
        {
            problems.Add("StringUtility.ReverseString(null) should return null, but returned non-null value.");
        }

        var reverseStringEmpty = StringUtility.ReverseString(string.Empty);
        if (reverseStringEmpty != string.Empty)
        {
            problems.Add("StringUtility.ReverseString(string.Empty) should return string.Empty, but returned non-empty value.");
        }

        // Validate ContainsAny behavior
        var containsAnyNullText = StringUtility.ContainsAny(null, "test", "value");
        if (containsAnyNullText)
        {
            problems.Add("StringUtility.ContainsAny(null, values) should return false, but returned true.");
        }

        var containsAnyEmptyText = StringUtility.ContainsAny(string.Empty, "test", "value");
        if (containsAnyEmptyText)
        {
            problems.Add("StringUtility.ContainsAny(string.Empty, values) should return false, but returned true.");
        }

        var containsAnyNullValues = StringUtility.ContainsAny("test text", null);
        if (containsAnyNullValues)
        {
            problems.Add("StringUtility.ContainsAny(text, null) should return false, but returned true.");
        }

        // Validate ContainsAll behavior
        var containsAllNullText = StringUtility.ContainsAll(null, "test", "value");
        if (containsAllNullText)
        {
            problems.Add("StringUtility.ContainsAll(null, values) should return false, but returned true.");
        }

        var containsAllEmptyText = StringUtility.ContainsAll(string.Empty, "test", "value");
        if (containsAllEmptyText)
        {
            problems.Add("StringUtility.ContainsAll(string.Empty, values) should return false, but returned true.");
        }

        var containsAllNullValues = StringUtility.ContainsAll("test text", null);
        if (containsAllNullValues)
        {
            problems.Add("StringUtility.ContainsAll(text, null) should return false, but returned true.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if StringUtility behavior is valid.
    /// </summary>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid()
    {
        try
        {
            return Validate().Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures StringUtility behavior is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails with list of problems.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "StringUtility validation failed:" + Environment.NewLine + "- " +
                string.Join(Environment.NewLine + "- ", problems));
        }
    }
}