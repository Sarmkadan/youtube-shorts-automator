// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Integration;

/// <summary>
/// Publishes webhook events to configured endpoints
/// Handles retries, signing, and delivery tracking
/// </summary>
public interface IWebhookPublisher
{
    Task<bool> PublishEventAsync<T>(string eventType, T data, string webhookUrl);
    Task<bool> PublishBulkEventsAsync<T>(string eventType, IEnumerable<T> data, IEnumerable<string> webhookUrls);
}

public class WebhookPublisher : IWebhookPublisher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookPublisher> _logger;
    private readonly string _webhookSecret;

    public WebhookPublisher(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookPublisher> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _webhookSecret = configuration.GetValue<string>("Webhook:Secret") ?? "default-secret";
    }

    public async Task<bool> PublishEventAsync<T>(string eventType, T data, string webhookUrl)
    {
        try
        {
            var payload = new WebhookPayload<T>
            {
                EventType = eventType,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            };

            var client = _httpClientFactory.CreateWebhookClient();
            var json = JsonSerializer.Serialize(payload);
            var signature = GenerateSignature(json);

            var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Webhook-Signature", signature);
            request.Headers.Add("X-Webhook-Event", eventType);
            request.Headers.Add("X-Webhook-Id", payload.Id);

            var response = await client.SendAsync(request);
            var success = response.IsSuccessStatusCode;

            _logger.LogInformation("Webhook event published. EventType: {EventType}, Url: {Url}, Success: {Success}",
                eventType, webhookUrl, success);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing webhook event. EventType: {EventType}, Url: {Url}",
                eventType, webhookUrl);
            return false;
        }
    }

    public async Task<bool> PublishBulkEventsAsync<T>(string eventType, IEnumerable<T> data, IEnumerable<string> webhookUrls)
    {
        var tasks = webhookUrls.Select(url =>
            PublishEventAsync(eventType, data, url)
        );

        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    private string GenerateSignature(string payload)
    {
        var signature = EncodingUtility.ComputeSha256Hash($"{payload}{_webhookSecret}");
        return signature;
    }
}

public class WebhookPayload<T>
{
    public string EventType { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string Id { get; set; } = string.Empty;
}
