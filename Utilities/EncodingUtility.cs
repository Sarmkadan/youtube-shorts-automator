// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides encoding and hashing utilities for data transformation.
/// Supports Base64, SHA-256, MD5, and URL encoding/decoding.
/// </summary>
public static class EncodingUtility
{
    public static string EncodeBase64(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes);
    }

    public static string DecodeBase64(string base64Text)
    {
        if (string.IsNullOrEmpty(base64Text))
            return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(base64Text);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ComputeSha256Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // SHA256.HashData is a static one-shot API — no Create/Dispose overhead,
        // no intermediate HashAlgorithm object on the heap.
        var inputBytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToHexStringLower(SHA256.HashData(inputBytes));
    }

    public static string ComputeSha256Hash(byte[] data)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        return Convert.ToHexStringLower(SHA256.HashData(data));
    }

    public static string ComputeMd5Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(text)));
    }

    public static string UrlEncode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return Uri.EscapeDataString(text);
    }

    public static string UrlDecode(string encodedText)
    {
        if (string.IsNullOrEmpty(encodedText))
            return string.Empty;

        try
        {
            return Uri.UnescapeDataString(encodedText);
        }
        catch
        {
            return encodedText;
        }
    }

    public static string GenerateRandomString(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // Rent from the shared pool to avoid a heap allocation for the byte buffer.
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            RandomNumberGenerator.Fill(buffer.AsSpan(0, length));
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                result.Append(chars[buffer[i] % chars.Length]);
            return result.ToString();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static string GenerateRandomHexString(int length = 32)
    {
        var byteCount = length / 2;
        var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            RandomNumberGenerator.Fill(buffer.AsSpan(0, byteCount));
            // Convert.ToHexStringLower on a span avoids copying into a new array.
            return Convert.ToHexStringLower(buffer.AsSpan(0, byteCount));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static Guid GenerateSecureGuid()
    {
        return Guid.NewGuid();
    }

    public static bool VerifyHash(string text, string hash)
    {
        var computedHash = ComputeSha256Hash(text);
        return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    public static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(queryString))
            return parameters;

        var parts = queryString.TrimStart('?').Split('&');
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            var keyValue = part.Split('=');
            if (keyValue.Length >= 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                if (!parameters.ContainsKey(key))
                {
                    parameters[key] = value;
                }
            }
        }

        return parameters;
    }

    public static string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return string.Empty;

        var parts = parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
        return string.Join("&", parts);
    }

    // Convert.ToHexStringLower (net9+) outputs lowercase hex directly from the
    // CLR without a StringBuilder loop — roughly 3× faster than the prior impl.
    private static string ConvertBytesToHexString(byte[] bytes) =>
        Convert.ToHexStringLower(bytes);

    public static string HtmlEncode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return System.Net.WebUtility.HtmlEncode(text);
    }

    public static string HtmlDecode(string encodedText)
    {
        if (string.IsNullOrEmpty(encodedText))
            return string.Empty;

        return System.Net.WebUtility.HtmlDecode(encodedText);
    }
}
