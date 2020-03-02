// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Represents errors returned from the YouTube API.
/// </summary>
public class YouTubeApiException : YoutubeShortsAutomatorException
{
    public int? ChannelId { get; set; }
    public string? ApiErrorCode { get; set; }
    public int? HttpStatusCode { get; set; }

    public YouTubeApiException(string message) : base(message) { }

    public YouTubeApiException(string message, Exception innerException)
        : base(message, innerException) { }

    public YouTubeApiException(string message, int channelId, string apiErrorCode, int httpStatusCode)
        : base(message)
    {
        ChannelId = channelId;
        ApiErrorCode = apiErrorCode;
        HttpStatusCode = httpStatusCode;
    }

    public bool IsTokenExpired()
    {
        // Checks if the error is due to token expiration
        return ApiErrorCode == "invalid_grant" || HttpStatusCode == 401;
    }

    public bool IsQuotaExceeded()
    {
        // Checks if the error is due to quota limit
        return ApiErrorCode == "quotaExceeded" || HttpStatusCode == 403;
    }
}
