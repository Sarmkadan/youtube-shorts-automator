// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// API endpoints for webhook management
/// Allows registration and management of webhook endpoints for event notifications
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly ICacheService _cacheService;

    public WebhookController(
        ILogger<WebhookController> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Register a new webhook
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(WebhookRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterWebhookAsync([FromBody] RegisterWebhookRequest request)
    {
        try
        {
            // Validate URL
            var (isValid, error) = ValidationUtility.ValidateUrl(request.Url);
            if (!isValid)
                return BadRequest(new { message = error });

            if (request.Events == null || request.Events.Length == 0)
                return BadRequest(new { message = "At least one event type is required" });

            var webhookId = Guid.NewGuid();

            var webhook = new WebhookRegistration
            {
                WebhookId = webhookId,
                Url = request.Url,
                Events = request.Events,
                Secret = EncodingUtility.GenerateRandomString(32),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _cacheService.Set($"webhook:{webhookId}", webhook, TimeSpan.FromDays(365));

            _logger.LogInformation("Webhook registered. WebhookId: {WebhookId}, Url: {Url}",
                webhookId, request.Url);

            return CreatedAtAction(nameof(GetWebhookAsync), new { webhookId },
                new WebhookRegistrationResponse
                {
                    WebhookId = webhookId,
                    Url = request.Url,
                    Status = "Active"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering webhook");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get webhook details
    /// </summary>
    [HttpGet("{webhookId}")]
    [ProducesResponseType(typeof(WebhookDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWebhookAsync(Guid webhookId)
    {
        try
        {
            var webhook = _cacheService.Get<WebhookRegistration>($"webhook:{webhookId}");
            if (webhook == null)
                return NotFound();

            return Ok(new WebhookDetails
            {
                WebhookId = webhook.WebhookId,
                Url = webhook.Url,
                Events = webhook.Events,
                IsActive = webhook.IsActive,
                CreatedAtUtc = webhook.CreatedAtUtc,
                LastTriedAtUtc = webhook.LastTriedAtUtc,
                SuccessfulDeliveries = webhook.SuccessfulDeliveries,
                FailedDeliveries = webhook.FailedDeliveries
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhook: {WebhookId}", webhookId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// List all registered webhooks
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WebhookListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListWebhooksAsync()
    {
        try
        {
            // Simulate fetching from database
            var webhooks = new List<WebhookListItem>
            {
                new()
                {
                    WebhookId = Guid.NewGuid(),
                    Url = "https://example.com/webhooks/uploads",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-7)
                }
            };

            var response = new WebhookListResponse
            {
                Webhooks = webhooks,
                TotalCount = webhooks.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing webhooks");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Update webhook configuration
    /// </summary>
    [HttpPut("{webhookId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWebhookAsync(Guid webhookId, [FromBody] UpdateWebhookRequest request)
    {
        try
        {
            var webhook = _cacheService.Get<WebhookRegistration>($"webhook:{webhookId}");
            if (webhook == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.Url))
            {
                var (isValid, error) = ValidationUtility.ValidateUrl(request.Url);
                if (!isValid)
                    return BadRequest(new { message = error });
                webhook.Url = request.Url;
            }

            if (request.Events != null && request.Events.Length > 0)
            {
                webhook.Events = request.Events;
            }

            if (request.IsActive.HasValue)
            {
                webhook.IsActive = request.IsActive.Value;
            }

            _cacheService.Set($"webhook:{webhookId}", webhook, TimeSpan.FromDays(365));

            _logger.LogInformation("Webhook updated. WebhookId: {WebhookId}", webhookId);

            return Ok(new { message = "Webhook updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating webhook: {WebhookId}", webhookId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Delete a webhook
    /// </summary>
    [HttpDelete("{webhookId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhookAsync(Guid webhookId)
    {
        try
        {
            await _cacheService.RemoveAsync($"webhook:{webhookId}");
            _logger.LogInformation("Webhook deleted. WebhookId: {WebhookId}", webhookId);
            return Ok(new { message = "Webhook deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook: {WebhookId}", webhookId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

#region Webhook Models

public class RegisterWebhookRequest
{
    public string Url { get; set; } = string.Empty;
    public string[] Events { get; set; } = Array.Empty<string>();
}

public class UpdateWebhookRequest
{
    public string? Url { get; set; }
    public string[]? Events { get; set; }
    public bool? IsActive { get; set; }
}

public class WebhookRegistrationResponse
{
    public Guid WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class WebhookRegistration
{
    public Guid WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string[] Events { get; set; } = Array.Empty<string>();
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastTriedAtUtc { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
}

public class WebhookDetails
{
    public Guid WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string[] Events { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastTriedAtUtc { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
}

public class WebhookListItem
{
    public Guid WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class WebhookListResponse
{
    public List<WebhookListItem> Webhooks { get; set; } = new();
    public int TotalCount { get; set; }
}

#endregion
