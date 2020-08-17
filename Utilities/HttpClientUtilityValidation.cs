// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for HttpClientUtility operations
/// Validates HttpClient instances, URLs, tokens, and other parameters used with HttpClientUtility methods
/// </summary>
public static class HttpClientUtilityValidation
{
    /// <summary>
    /// Validates HttpClient instance for use with HttpClientUtility methods
    /// </summary>
    /// <param name="client">HttpClient instance to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if client is null</exception>
    public static IReadOnlyList<string> ValidateHttpClient(HttpClient? client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        var problems = new List<string>();

        // HttpClient should have a non-default timeout
        if (client.Timeout.TotalSeconds <= 0)
        {
            problems.Add("HttpClient timeout must be greater than zero seconds.");
        }

        // HttpClient should have UserAgent header set (common practice)
        if (client.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            problems.Add("HttpClient should have User-Agent header configured.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if HttpClient instance is valid for use with HttpClientUtility methods
    /// </summary>
    /// <param name="client">HttpClient instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidHttpClient(HttpClient? client)
    {
        try
        {
            return ValidateHttpClient(client).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures HttpClient instance is valid for use with HttpClientUtility methods, throwing if not
    /// </summary>
    /// <param name="client">HttpClient instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    /// <exception cref="ArgumentNullException">Thrown if client is null</exception>
    public static void EnsureValidHttpClient(HttpClient? client)
    {
        var problems = ValidateHttpClient(client);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HttpClient is invalid for HttpClientUtility operations:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates URL for use with HttpClientUtility methods
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if url is null</exception>
    public static IReadOnlyList<string> ValidateUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(url))
        {
            problems.Add("URL cannot be null or whitespace.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
        {
            problems.Add($"URL '{url}' is not a valid absolute URI.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if URL is valid for use with HttpClientUtility methods
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidUrl(string url)
    {
        try
        {
            return ValidateUrl(url).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures URL is valid for use with HttpClientUtility methods, throwing if not
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown if url is null</exception>
    public static void EnsureValidUrl(string url)
    {
        var problems = ValidateUrl(url);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"URL is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates scheme parameter for AddAuthorizationHeader and AddBearerToken methods
    /// </summary>
    /// <param name="scheme">Authentication scheme (e.g., "Bearer", "Basic")</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if scheme is null</exception>
    public static IReadOnlyList<string> ValidateScheme(string scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme, nameof(scheme));

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(scheme))
        {
            problems.Add("Scheme cannot be null or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if scheme is valid for authentication headers
    /// </summary>
    /// <param name="scheme">Scheme to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidScheme(string scheme)
    {
        try
        {
            return ValidateScheme(scheme).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures scheme is valid for authentication headers, throwing if not
    /// </summary>
    /// <param name="scheme">Scheme to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown if scheme is null</exception>
    public static void EnsureValidScheme(string scheme)
    {
        var problems = ValidateScheme(scheme);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Scheme is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates token parameter for authentication methods
    /// </summary>
    /// <param name="token">Authentication token</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if token is null</exception>
    public static IReadOnlyList<string> ValidateToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(token))
        {
            problems.Add("Token cannot be null or whitespace.");
        }

        // Basic token validation - should not contain spaces
        if (token.Contains(' '))
        {
            problems.Add("Token should not contain spaces.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if token is valid for authentication
    /// </summary>
    /// <param name="token">Token to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidToken(string token)
    {
        try
        {
            return ValidateToken(token).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures token is valid for authentication, throwing if not
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown if token is null</exception>
    public static void EnsureValidToken(string token)
    {
        var problems = ValidateToken(token);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Token is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates API key parameter
    /// </summary>
    /// <param name="apiKey">API key to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if apiKey is null</exception>
    public static IReadOnlyList<string> ValidateApiKey(string apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey, nameof(apiKey));

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            problems.Add("API key cannot be null or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if API key is valid
    /// </summary>
    /// <param name="apiKey">API key to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidApiKey(string apiKey)
    {
        try
        {
            return ValidateApiKey(apiKey).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures API key is valid, throwing if not
    /// </summary>
    /// <param name="apiKey">API key to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown if apiKey is null</exception>
    public static void EnsureValidApiKey(string apiKey)
    {
        var problems = ValidateApiKey(apiKey);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"API key is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates data parameter for serialization methods (PostAsJsonAsync, PutAsJsonAsync)
    /// </summary>
    /// <typeparam name="T">Type of data being serialized</typeparam>
    /// <param name="data">Data to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if data is null</exception>
    public static IReadOnlyList<string> ValidateData<T>(T data)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if data is valid for serialization
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Data to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidData<T>(T data)
    {
        try
        {
            return ValidateData(data).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures data is valid for serialization, throwing if not
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Data to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown if data is null</exception>
    public static void EnsureValidData<T>(T data)
    {
        var problems = ValidateData(data);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Data is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates timeoutSeconds parameter for CreateConfiguredClient method
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateTimeoutSeconds(int timeoutSeconds)
    {
        var problems = new List<string>();

        if (timeoutSeconds <= 0)
        {
            problems.Add("Timeout must be greater than zero seconds.");
        }

        if (timeoutSeconds > 300)
        {
            problems.Add("Timeout cannot exceed 300 seconds (5 minutes).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if timeoutSeconds is valid for CreateConfiguredClient
    /// </summary>
    /// <param name="timeoutSeconds">Timeout to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidTimeoutSeconds(int timeoutSeconds)
    {
        return ValidateTimeoutSeconds(timeoutSeconds).Count == 0;
    }

    /// <summary>
    /// Ensures timeoutSeconds is valid for CreateConfiguredClient, throwing if not
    /// </summary>
    /// <param name="timeoutSeconds">Timeout to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidTimeoutSeconds(int timeoutSeconds)
    {
        var problems = ValidateTimeoutSeconds(timeoutSeconds);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Timeout is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates maxRetries parameter for HTTP request methods
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateMaxRetries(int maxRetries)
    {
        var problems = new List<string>();

        if (maxRetries < 0)
        {
            problems.Add("Max retries cannot be negative.");
        }

        if (maxRetries > 10)
        {
            problems.Add("Max retries cannot exceed 10.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if maxRetries is valid for HTTP request methods
    /// </summary>
    /// <param name="maxRetries">Max retries to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidMaxRetries(int maxRetries)
    {
        return ValidateMaxRetries(maxRetries).Count == 0;
    }

    /// <summary>
    /// Ensures maxRetries is valid for HTTP request methods, throwing if not
    /// </summary>
    /// <param name="maxRetries">Max retries to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidMaxRetries(int maxRetries)
    {
        var problems = ValidateMaxRetries(maxRetries);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Max retries is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates userAgent parameter for CreateConfiguredClient method
    /// </summary>
    /// <param name="userAgent">User agent string</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateUserAgent(string? userAgent)
    {
        var problems = new List<string>();

        if (userAgent != null && userAgent.Length > 500)
        {
            problems.Add("User agent cannot exceed 500 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if userAgent is valid for CreateConfiguredClient
    /// </summary>
    /// <param name="userAgent">User agent to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidUserAgent(string? userAgent)
    {
        return ValidateUserAgent(userAgent).Count == 0;
    }

    /// <summary>
    /// Ensures userAgent is valid for CreateConfiguredClient, throwing if not
    /// </summary>
    /// <param name="userAgent">User agent to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidUserAgent(string? userAgent)
    {
        var problems = ValidateUserAgent(userAgent);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"User agent is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates statusCode for IsSuccessStatusCode, IsClientError, IsServerError methods
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateStatusCode(int statusCode)
    {
        var problems = new List<string>();

        if (statusCode < 0)
        {
            problems.Add("Status code cannot be negative.");
        }

        if (statusCode > 999)
        {
            problems.Add("Status code cannot exceed 999.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if statusCode is valid for status code methods
    /// </summary>
    /// <param name="statusCode">Status code to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidStatusCode(int statusCode)
    {
        return ValidateStatusCode(statusCode).Count == 0;
    }

    /// <summary>
    /// Ensures statusCode is valid for status code methods, throwing if not
    /// </summary>
    /// <param name="statusCode">Status code to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidStatusCode(int statusCode)
    {
        var problems = ValidateStatusCode(statusCode);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Status code is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}