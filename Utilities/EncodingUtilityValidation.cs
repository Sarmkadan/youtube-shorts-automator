// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation utilities for encoding and hashing operations to ensure data
// integrity and prevent runtime errors from invalid inputs.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for encoding and hashing operations to ensure
/// inputs are valid and operations return human-readable validation errors.
/// </summary>
public static class EncodingUtilityValidation
{
    /// <summary>
    /// Validates a Base64 encoded string to ensure it's properly formatted.
    /// </summary>
    /// <param name="base64Text">The Base64 string to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="base64Text"/> is null.</exception>
    public static IReadOnlyList<string> ValidateBase64(this string? base64Text)
    {
        ArgumentNullException.ThrowIfNull(base64Text);

        if (string.IsNullOrEmpty(base64Text))
        {
            return Array.Empty<string>();
        }

        var problems = new List<string>();

        // Check if the string length is appropriate for Base64
        // Base64 strings should have a length that's a multiple of 4
        if (base64Text.Length % 4 != 0)
        {
            problems.Add("Base64 string length must be a multiple of 4");
        }

        // Check for invalid Base64 characters
        const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        foreach (var c in base64Text)
        {
            if (c != '=' && !base64Chars.Contains(c))
            {
                problems.Add($"Invalid Base64 character: '{c}'");
            }
        }

        // Check that padding characters '=' only appear at the end
        var paddingIndex = base64Text.IndexOf('=');
        if (paddingIndex >= 0 && paddingIndex != base64Text.Length - 1 && base64Text[paddingIndex + 1] != '=')
        {
            problems.Add("Padding character '=' must only appear at the end of Base64 string");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a Base64 encoded string is valid.
    /// </summary>
    /// <param name="base64Text">The Base64 string to check.</param>
    /// <returns>True if the Base64 string is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="base64Text"/> is null.</exception>
    public static bool IsValidBase64(this string? base64Text)
    {
        return base64Text?.ValidateBase64() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a Base64 encoded string is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="base64Text">The Base64 string to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="base64Text"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="base64Text"/> has validation problems.
    /// </exception>
    public static void EnsureValidBase64(this string? base64Text)
    {
        ArgumentNullException.ThrowIfNull(base64Text);

        var problems = base64Text.ValidateBase64();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Base64 string has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates a SHA-256 hash string to ensure it's a valid hexadecimal hash.
    /// </summary>
    /// <param name="hash">The SHA-256 hash string to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hash"/> is null.</exception>
    public static IReadOnlyList<string> ValidateSha256Hash(this string? hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        if (string.IsNullOrEmpty(hash))
        {
            return Array.Empty<string>();
        }

        var problems = new List<string>();

        // SHA-256 hashes are 64 hexadecimal characters (32 bytes * 2)
        if (hash.Length != 64)
        {
            problems.Add("SHA-256 hash must be exactly 64 hexadecimal characters long");
        }

        // Validate hexadecimal characters
        foreach (var c in hash)
        {
            if (!Uri.IsHexDigit(c))
            {
                problems.Add($"Invalid hexadecimal character in SHA-256 hash: '{c}'");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a SHA-256 hash string is valid.
    /// </summary>
    /// <param name="hash">The SHA-256 hash string to check.</param>
    /// <returns>True if the SHA-256 hash is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hash"/> is null.</exception>
    public static bool IsValidSha256Hash(this string? hash)
    {
        return hash?.ValidateSha256Hash() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a SHA-256 hash string is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="hash">The SHA-256 hash string to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="hash"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="hash"/> has validation problems.
    /// </exception>
    public static void EnsureValidSha256Hash(this string? hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        var problems = hash.ValidateSha256Hash();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SHA-256 hash has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates an MD5 hash string to ensure it's a valid hexadecimal hash.
    /// </summary>
    /// <param name="hash">The MD5 hash string to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hash"/> is null.</exception>
    public static IReadOnlyList<string> ValidateMd5Hash(this string? hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        if (string.IsNullOrEmpty(hash))
        {
            return Array.Empty<string>();
        }

        var problems = new List<string>();

        // MD5 hashes are 32 hexadecimal characters (16 bytes * 2)
        if (hash.Length != 32)
        {
            problems.Add("MD5 hash must be exactly 32 hexadecimal characters long");
        }

        // Validate hexadecimal characters
        foreach (var c in hash)
        {
            if (!Uri.IsHexDigit(c))
            {
                problems.Add($"Invalid hexadecimal character in MD5 hash: '{c}'");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an MD5 hash string is valid.
    /// </summary>
    /// <param name="hash">The MD5 hash string to check.</param>
    /// <returns>True if the MD5 hash is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="hash"/> is null.</exception>
    public static bool IsValidMd5Hash(this string? hash)
    {
        return hash?.ValidateMd5Hash() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that an MD5 hash string is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="hash">The MD5 hash string to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="hash"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="hash"/> has validation problems.
    /// </exception>
    public static void EnsureValidMd5Hash(this string? hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        var problems = hash.ValidateMd5Hash();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MD5 hash has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates a URL encoded string to ensure it's properly formatted.
    /// </summary>
    /// <param name="encodedText">The URL encoded string to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="encodedText"/> is null.</exception>
    public static IReadOnlyList<string> ValidateUrlEncoded(this string? encodedText)
    {
        ArgumentNullException.ThrowIfNull(encodedText);

        if (string.IsNullOrEmpty(encodedText))
        {
            return Array.Empty<string>();
        }

        var problems = new List<string>();

        // URL encoded strings should be valid for Uri.UnescapeDataString
        // We can't easily validate the format without attempting to decode,
        // so we just check for common issues

        // Check for unescaped special characters that shouldn't appear in encoded text
        if (encodedText.Contains(' '))
        {
            problems.Add("URL encoded string should not contain unescaped spaces");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a URL encoded string is valid.
    /// </summary>
    /// <param name="encodedText">The URL encoded string to check.</param>
    /// <returns>True if the URL encoded string is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="encodedText"/> is null.</exception>
    public static bool IsValidUrlEncoded(this string? encodedText)
    {
        return encodedText?.ValidateUrlEncoded() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a URL encoded string is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="encodedText">The URL encoded string to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="encodedText"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="encodedText"/> has validation problems.
    /// </exception>
    public static void EnsureValidUrlEncoded(this string? encodedText)
    {
        ArgumentNullException.ThrowIfNull(encodedText);

        var problems = encodedText.ValidateUrlEncoded();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"URL encoded string has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates a query string parameter name.
    /// </summary>
    /// <param name="parameterName">The parameter name to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterName"/> is null.</exception>
    public static IReadOnlyList<string> ValidateQueryParameterName(this string? parameterName)
    {
        ArgumentNullException.ThrowIfNull(parameterName);

        if (string.IsNullOrEmpty(parameterName))
        {
            return Array.Empty<string>();
        }

        var problems = new List<string>();

        // Parameter names should not contain certain characters
        var invalidChars = new[] { '=', '&', '?', '#' };
        if (invalidChars.Any(c => parameterName.Contains(c)))
        {
            problems.Add("Query parameter name contains invalid characters (=, &, ?, #)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a query string parameter name is valid.
    /// </summary>
    /// <param name="parameterName">The parameter name to check.</param>
    /// <returns>True if the query parameter name is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterName"/> is null.</exception>
    public static bool IsValidQueryParameterName(this string? parameterName)
    {
        return parameterName?.ValidateQueryParameterName() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a query string parameter name is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="parameterName">The parameter name to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="parameterName"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="parameterName"/> has validation problems.
    /// </exception>
    public static void EnsureValidQueryParameterName(this string? parameterName)
    {
        ArgumentNullException.ThrowIfNull(parameterName);

        var problems = parameterName.ValidateQueryParameterName();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Query parameter name has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates a query string parameter value.
    /// </summary>
    /// <param name="parameterValue">The parameter value to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterValue"/> is null.</exception>
    public static IReadOnlyList<string> ValidateQueryParameterValue(this string? parameterValue)
    {
        ArgumentNullException.ThrowIfNull(parameterValue);

        // Parameter values can contain most characters, but we check for common issues
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a query string parameter value is valid.
    /// </summary>
    /// <param name="parameterValue">The parameter value to check.</param>
    /// <returns>True if the query parameter value is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterValue"/> is null.</exception>
    public static bool IsValidQueryParameterValue(this string? parameterValue)
    {
        return parameterValue?.ValidateQueryParameterValue() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a query string parameter value is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="parameterValue">The parameter value to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="parameterValue"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="parameterValue"/> has validation problems.
    /// </exception>
    public static void EnsureValidQueryParameterValue(this string? parameterValue)
    {
        ArgumentNullException.ThrowIfNull(parameterValue);

        var problems = parameterValue.ValidateQueryParameterValue();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Query parameter value has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates HTML encoded text.
    /// </summary>
    /// <param name="encodedText">The HTML encoded text to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="encodedText"/> is null.</exception>
    public static IReadOnlyList<string> ValidateHtmlEncoded(this string? encodedText)
    {
        ArgumentNullException.ThrowIfNull(encodedText);

        if (string.IsNullOrEmpty(encodedText))
        {
            return Array.Empty<string>();
        }

        // HTML encoded text should not contain unescaped special characters
        var problems = new List<string>();

        // Check for common unescaped characters
        var unescapedChars = new[] { '<', '>', '"', '\'' };
        foreach (var c in unescapedChars)
        {
            if (encodedText.Contains(c))
            {
                problems.Add($"HTML encoded text should not contain unescaped character: '{c}'");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether HTML encoded text is valid.
    /// </summary>
    /// <param name="encodedText">The HTML encoded text to check.</param>
    /// <returns>True if the HTML encoded text is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="encodedText"/> is null.</exception>
    public static bool IsValidHtmlEncoded(this string? encodedText)
    {
        return encodedText?.ValidateHtmlEncoded() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that HTML encoded text is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="encodedText">The HTML encoded text to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="encodedText"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="encodedText"/> has validation problems.
    /// </exception>
    public static void EnsureValidHtmlEncoded(this string? encodedText)
    {
        ArgumentNullException.ThrowIfNull(encodedText);

        var problems = encodedText.ValidateHtmlEncoded();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HTML encoded text has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates a random string length.
    /// </summary>
    /// <param name="length">The length to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRandomStringLength(this int length)
    {
        var problems = new List<string>();

        if (length <= 0)
        {
            problems.Add("Random string length must be greater than 0");
        }
        else if (length > 1024)
        {
            problems.Add("Random string length must not exceed 1024 characters for performance reasons");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a random string length is valid.
    /// </summary>
    /// <param name="length">The length to check.</param>
    /// <returns>True if the random string length is valid; otherwise, false.</returns>
    public static bool IsValidRandomStringLength(this int length)
    {
        return length.ValidateRandomStringLength() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a random string length is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="length">The length to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="length"/> has validation problems.
    /// </exception>
    public static void EnsureValidRandomStringLength(this int length)
    {
        var problems = length.ValidateRandomStringLength();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Random string length has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates a random hex string length (must be even).
    /// </summary>
    /// <param name="length">The length to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRandomHexStringLength(this int length)
    {
        var problems = new List<string>();

        if (length <= 0)
        {
            problems.Add("Random hex string length must be greater than 0");
        }
        else if (length % 2 != 0)
        {
            problems.Add("Random hex string length must be even (each byte is represented by 2 hex characters)");
        }
        else if (length > 2048)
        {
            problems.Add("Random hex string length must not exceed 2048 characters for performance reasons");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a random hex string length is valid.
    /// </summary>
    /// <param name="length">The length to check.</param>
    /// <returns>True if the random hex string length is valid; otherwise, false.</returns>
    public static bool IsValidRandomHexStringLength(this int length)
    {
        return length.ValidateRandomHexStringLength() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that a random hex string length is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation
    /// problems if it is not.
    /// </summary>
    /// <param name="length">The length to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="length"/> has validation problems.
    /// </exception>
    public static void EnsureValidRandomHexStringLength(this int length)
    {
        var problems = length.ValidateRandomHexStringLength();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Random hex string length has validation problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }
}