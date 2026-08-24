// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="StringUtilityBenchmarks"/> benchmark class.
/// Validates the results of benchmark methods to ensure they produce valid outputs
/// and prevent misleading benchmark results from invalid or edge-case data.
/// </summary>
public static class StringUtilityBenchmarksValidation
{
    /// <summary>
    /// Validates the results produced by <see cref="StringUtilityBenchmarks"/> benchmarks.
    /// </summary>
    /// <param name="truncateResult">The result of the Truncate benchmark.</param>
    /// <param name="toCamelCaseResult">The result of the ToCamelCase benchmark.</param>
    /// <param name="toPascalCaseResult">The result of the ToPascalCase benchmark.</param>
    /// <param name="toSlugResult">The result of the ToSlug benchmark.</param>
    /// <param name="splitByLengthResult">The result of the SplitByLength benchmark.</param>
    /// <param name="removeWhitespaceResult">The result of the RemoveWhitespace benchmark.</param>
    /// <param name="normalizeWhitespaceResult">The result of the NormalizeWhitespace benchmark.</param>
    /// <returns>A list of human-readable validation problems; empty if all results are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="truncateResult"/>, <paramref name="toCamelCaseResult"/>, <paramref name="toPascalCaseResult"/>, <paramref name="toSlugResult"/>, <paramref name="splitByLengthResult"/>, <paramref name="removeWhitespaceResult"/>, or <paramref name="normalizeWhitespaceResult"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(
        string truncateResult,
        string toCamelCaseResult,
        string toPascalCaseResult,
        string toSlugResult,
        string[] splitByLengthResult,
        string removeWhitespaceResult,
        string normalizeWhitespaceResult)
    {
        ArgumentNullException.ThrowIfNull(truncateResult);
        ArgumentNullException.ThrowIfNull(toCamelCaseResult);
        ArgumentNullException.ThrowIfNull(toPascalCaseResult);
        ArgumentNullException.ThrowIfNull(toSlugResult);
        ArgumentNullException.ThrowIfNull(splitByLengthResult);
        ArgumentNullException.ThrowIfNull(removeWhitespaceResult);
        ArgumentNullException.ThrowIfNull(normalizeWhitespaceResult);

        var problems = new List<string>();

        // Validate Truncate result
        if (truncateResult.Length > 30)
        {
            problems.Add("Truncate returned a string longer than 30 characters");
        }

        // Validate ToCamelCase result
        if (toCamelCaseResult.Length == 0)
        {
            problems.Add("ToCamelCase returned empty string");
        }
        else if (char.IsUpper(toCamelCaseResult[0]))
        {
            problems.Add("ToCamelCase did not produce camelCase (first character should be lowercase)");
        }

        // Validate ToPascalCase result
        if (toPascalCaseResult.Length == 0)
        {
            problems.Add("ToPascalCase returned empty string");
        }
        else if (!char.IsUpper(toPascalCaseResult[0]))
        {
            problems.Add("ToPascalCase did not produce PascalCase (first character should be uppercase)");
        }

        // Validate ToSlug result
        if (toSlugResult.Length == 0)
        {
            problems.Add("ToSlug returned empty string");
        }
        else if (toSlugResult.Contains(' '))
        {
            problems.Add("ToSlug result contains spaces");
        }

        // Validate SplitByLength result
        if (splitByLengthResult.Length == 0)
        {
            problems.Add("SplitByLength returned empty array");
        }
        else
        {
            foreach (var segment in splitByLengthResult)
            {
                if (segment is null)
                {
                    problems.Add("SplitByLength contains null segment");
                    break;
                }
            }
        }

        // Validate RemoveWhitespace result
        if (removeWhitespaceResult.Any(char.IsWhiteSpace))
        {
            problems.Add("RemoveWhitespace result still contains whitespace");
        }

        // Validate NormalizeWhitespace result
        if (normalizeWhitespaceResult.Length == 0)
        {
            problems.Add("NormalizeWhitespace returned empty string");
        }
        else
        {
            for (int i = 0; i < normalizeWhitespaceResult.Length - 1; i++)
            {
                if (char.IsWhiteSpace(normalizeWhitespaceResult[i]) &&
                    char.IsWhiteSpace(normalizeWhitespaceResult[i + 1]))
                {
                    problems.Add("NormalizeWhitespace result contains consecutive whitespace");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the benchmark results are valid.
    /// </summary>
    /// <param name="truncateResult">The result of the Truncate benchmark.</param>
    /// <param name="toCamelCaseResult">The result of the ToCamelCase benchmark.</param>
    /// <param name="toPascalCaseResult">The result of the ToPascalCase benchmark.</param>
    /// <param name="toSlugResult">The result of the ToSlug benchmark.</param>
    /// <param name="splitByLengthResult">The result of the SplitByLength benchmark.</param>
    /// <param name="removeWhitespaceResult">The result of the RemoveWhitespace benchmark.</param>
    /// <param name="normalizeWhitespaceResult">The result of the NormalizeWhitespace benchmark.</param>
    /// <returns><see langword="true"/> if all results are valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string truncateResult,
        string toCamelCaseResult,
        string toPascalCaseResult,
        string toSlugResult,
        string[] splitByLengthResult,
        string removeWhitespaceResult,
        string normalizeWhitespaceResult) =>
        Validate(
            truncateResult,
            toCamelCaseResult,
            toPascalCaseResult,
            toSlugResult,
            splitByLengthResult,
            removeWhitespaceResult,
            normalizeWhitespaceResult).Count == 0;

    /// <summary>
    /// Ensures that the benchmark results are valid.
    /// </summary>
    /// <param name="truncateResult">The result of the Truncate benchmark.</param>
    /// <param name="toCamelCaseResult">The result of the ToCamelCase benchmark.</param>
    /// <param name="toPascalCaseResult">The result of the ToPascalCase benchmark.</param>
    /// <param name="toSlugResult">The result of the ToSlug benchmark.</param>
    /// <param name="splitByLengthResult">The result of the SplitByLength benchmark.</param>
    /// <param name="removeWhitespaceResult">The result of the RemoveWhitespace benchmark.</param>
    /// <param name="normalizeWhitespaceResult">The result of the NormalizeWhitespace benchmark.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="truncateResult"/>, <paramref name="toCamelCaseResult"/>, <paramref name="toPascalCaseResult"/>, <paramref name="toSlugResult"/>, <paramref name="splitByLengthResult"/>, <paramref name="removeWhitespaceResult"/>, or <paramref name="normalizeWhitespaceResult"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if any result is invalid, containing a list of problems.</exception>
    public static void EnsureValid(
        string truncateResult,
        string toCamelCaseResult,
        string toPascalCaseResult,
        string toSlugResult,
        string[] splitByLengthResult,
        string removeWhitespaceResult,
        string normalizeWhitespaceResult)
    {
        // Validate will throw ArgumentNullException for null inputs
        var problems = Validate(
            truncateResult,
            toCamelCaseResult,
            toPascalCaseResult,
            toSlugResult,
            splitByLengthResult,
            removeWhitespaceResult,
            normalizeWhitespaceResult);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"StringUtilityBenchmarks results are invalid. Problems: {string.Join(", ", problems)}");
        }
    }
}
