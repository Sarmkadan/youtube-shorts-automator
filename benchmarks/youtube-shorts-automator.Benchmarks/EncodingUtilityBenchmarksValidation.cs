// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="EncodingUtilityBenchmarks"/> to ensure
/// benchmark results are valid and meet expected constraints.
/// </summary>
public static class EncodingUtilityBenchmarksValidation
{
    /// <summary>
    /// Validates an <see cref="EncodingUtilityBenchmarks"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EncodingUtilityBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Sha256Hash - call the method property
        var sha256Hash = value.Sha256Hash();
        if (string.IsNullOrEmpty(sha256Hash))
        {
            errors.Add("Sha256Hash is null or empty");
        }
        else if (!IsValidHexString(sha256Hash, 64))
        {
            errors.Add("Sha256Hash is not a valid 64-character hexadecimal string");
        }

        // Validate Md5Hash
        var md5Hash = value.Md5Hash();
        if (string.IsNullOrEmpty(md5Hash))
        {
            errors.Add("Md5Hash is null or empty");
        }
        else if (!IsValidHexString(md5Hash, 32))
        {
            errors.Add("Md5Hash is not a valid 32-character hexadecimal string");
        }

        // Validate EncodeBase64
        var encodeBase64 = value.EncodeBase64();
        if (string.IsNullOrEmpty(encodeBase64))
        {
            errors.Add("EncodeBase64 is null or empty");
        }
        else if (!IsValidBase64String(encodeBase64))
        {
            errors.Add("EncodeBase64 is not a valid Base64 encoded string");
        }

        // Validate DecodeBase64
        var decodeBase64 = value.DecodeBase64();
        if (string.IsNullOrEmpty(decodeBase64))
        {
            errors.Add("DecodeBase64 is null or empty");
        }
        // DecodeBase64 returns the decoded original text, which can be any valid UTF-8 string
        // We just validate it's not null/empty

        // Validate GenerateRandomString
        var generateRandomString = value.GenerateRandomString();
        if (string.IsNullOrEmpty(generateRandomString))
        {
            errors.Add("GenerateRandomString is null or empty");
        }
        else if (generateRandomString.Length != 32)
        {
            errors.Add("GenerateRandomString has unexpected length (expected 32 characters)");
        }

        // Validate GenerateRandomHexString
        var generateRandomHexString = value.GenerateRandomHexString();
        if (string.IsNullOrEmpty(generateRandomHexString))
        {
            errors.Add("GenerateRandomHexString is null or empty");
        }
        else if (!IsValidHexString(generateRandomHexString, 32))
        {
            errors.Add("GenerateRandomHexString is not a valid 32-character hexadecimal string");
        }
        else if (generateRandomHexString.Length != 32)
        {
            errors.Add("GenerateRandomHexString has unexpected length (expected 32 characters)");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified string is a valid hexadecimal string of the expected length.
    /// </summary>
    /// <param name="hexString">The hexadecimal string to validate.</param>
    /// <param name="expectedLength">The expected length of the hexadecimal string (in characters).</param>
    /// <returns>True if the string is valid hexadecimal of the expected length; otherwise, false.</returns>
    private static bool IsValidHexString(string hexString, int expectedLength)
    {
        if (hexString.Length != expectedLength)
        {
            return false;
        }

        // Check if all characters are valid hex digits
        foreach (var c in hexString)
        {
            if (!IsHexChar(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified character is a valid hexadecimal digit.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is a valid hex digit; otherwise, false.</returns>
    private static bool IsHexChar(char c)
    {
        return (c >= '0' && c <= '9') ||
               (c >= 'a' && c <= 'f') ||
               (c >= 'A' && c <= 'F');
    }

    /// <summary>
    /// Determines whether the specified string is a valid Base64 encoded string.
    /// </summary>
    /// <param name="base64String">The Base64 string to validate.</param>
    /// <returns>True if the string is valid Base64; otherwise, false.</returns>
    private static bool IsValidBase64String(string base64String)
    {
        try
        {
            // Try to decode it to verify it's valid Base64
            Convert.FromBase64String(base64String);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="EncodingUtilityBenchmarks"/> instance is valid.
    /// </nsummary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this EncodingUtilityBenchmarks value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="EncodingUtilityBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the list of validation errors.</exception>
    public static void EnsureValid(this EncodingUtilityBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"EncodingUtilityBenchmarks validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", errors)}");
        }
    }
}
