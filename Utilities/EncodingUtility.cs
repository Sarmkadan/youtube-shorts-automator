// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides encoding and hashing utilities for data transformation
/// Supports Base64, SHA256, MD5, and URL encoding/decoding
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

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return ConvertBytesToHexString(hashedBytes);
        }
    }

    public static string ComputeSha256Hash(byte[] data)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(data);
            return ConvertBytesToHexString(hashedBytes);
        }
    }

    public static string ComputeMd5Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using (var md5 = MD5.Create())
        {
            var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            return ConvertBytesToHexString(hashedBytes);
        }
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
        using (var rng = RandomNumberGenerator.Create())
        {
            var data = new byte[length];
            rng.GetBytes(data);

            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }
            return result.ToString();
        }
    }

    public static string GenerateRandomHexString(int length = 32)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var data = new byte[length / 2];
            rng.GetBytes(data);
            return ConvertBytesToHexString(data);
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

    private static string ConvertBytesToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

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
