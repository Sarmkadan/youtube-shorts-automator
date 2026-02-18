// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Integration;

/// <summary>
/// Factory for creating configured HTTP clients with specific behaviors
/// Centralizes HTTP client configuration and lifetime management
/// </summary>
public interface IHttpClientFactory
{
    HttpClient CreateYouTubeApiClient();
    HttpClient CreateStorageClient();
    HttpClient CreateWebhookClient();
    HttpClient CreateAnalyticsClient();
}

public class DefaultHttpClientFactory : IHttpClientFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DefaultHttpClientFactory> _logger;
    private HttpClient? _youtubeClient;
    private HttpClient? _storageClient;
    private HttpClient? _webhookClient;
    private HttpClient? _analyticsClient;

    public DefaultHttpClientFactory(IConfiguration configuration, ILogger<DefaultHttpClientFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public HttpClient CreateYouTubeApiClient()
    {
        if (_youtubeClient != null)
            return _youtubeClient;

        _logger.LogInformation("Creating YouTube API client");

        _youtubeClient = HttpClientUtility.CreateConfiguredClient(
            timeoutSeconds: 30,
            userAgent: "YouTubeShortsAutomator/1.0"
        );

        var apiKey = _configuration.GetValue<string>("YouTube:ApiKey");
        if (!string.IsNullOrEmpty(apiKey))
        {
            _youtubeClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }

        return _youtubeClient;
    }

    public HttpClient CreateStorageClient()
    {
        if (_storageClient != null)
            return _storageClient;

        _logger.LogInformation("Creating storage client");

        _storageClient = HttpClientUtility.CreateConfiguredClient(
            timeoutSeconds: 60,
            userAgent: "YouTubeShortsAutomator-Storage/1.0"
        );

        var accessKey = _configuration.GetValue<string>("Storage:AccessKey");
        if (!string.IsNullOrEmpty(accessKey))
        {
            HttpClientUtility.AddApiKeyHeader(_storageClient, accessKey);
        }

        return _storageClient;
    }

    public HttpClient CreateWebhookClient()
    {
        if (_webhookClient != null)
            return _webhookClient;

        _logger.LogInformation("Creating webhook client");

        _webhookClient = HttpClientUtility.CreateConfiguredClient(
            timeoutSeconds: 15,
            userAgent: "YouTubeShortsAutomator-Webhook/1.0"
        );

        return _webhookClient;
    }

    public HttpClient CreateAnalyticsClient()
    {
        if (_analyticsClient != null)
            return _analyticsClient;

        _logger.LogInformation("Creating analytics client");

        _analyticsClient = HttpClientUtility.CreateConfiguredClient(
            timeoutSeconds: 30,
            userAgent: "YouTubeShortsAutomator-Analytics/1.0"
        );

        return _analyticsClient;
    }
}
