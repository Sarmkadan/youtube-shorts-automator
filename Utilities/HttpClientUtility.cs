// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;
using System.Text.Json;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides HTTP client utilities for making API requests with retry logic and error handling
/// Supports JSON serialization, authentication headers, and timeout configuration
/// </summary>
public static class HttpClientUtility
{
    public static HttpClient CreateConfiguredClient(
        int timeoutSeconds = 30,
        string? userAgent = null)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        if (!string.IsNullOrEmpty(userAgent))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        return client;
    }

    public static void AddAuthorizationHeader(HttpClient client, string scheme, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
    }

    public static void AddApiKeyHeader(HttpClient client, string apiKey)
    {
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }

    public static void AddBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<T?> GetAsJsonAsync<T>(
        HttpClient client,
        string url,
        int maxRetries = 3)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }, maxRetries);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        HttpClient client,
        string url,
        T data,
        int maxRetries = 3)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }, maxRetries);
    }

    public static async Task<string> GetAsStringAsync(
        HttpClient client,
        string url,
        int maxRetries = 3)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }, maxRetries);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(
        HttpClient client,
        string url,
        T data,
        int maxRetries = 3)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return await client.PutAsync(url, content);
        }, maxRetries);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(
        HttpClient client,
        string url,
        int maxRetries = 3)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await client.DeleteAsync(url);
        }, maxRetries);
    }

    private static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException) when (attempt < maxRetries - 1)
            {
                attempt++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (TaskCanceledException) when (attempt < maxRetries - 1)
            {
                attempt++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }

        // Final attempt without catching
        return await operation();
    }

    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }

    public static bool IsClientError(int statusCode)
    {
        return statusCode >= 400 && statusCode < 500;
    }

    public static bool IsServerError(int statusCode)
    {
        return statusCode >= 500;
    }

    public static string GetStatusCodeDescription(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            _ => "Unknown"
        };
    }
}
