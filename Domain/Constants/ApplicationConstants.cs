// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class ApplicationConstants
{
    public static class Video
    {
        public const int MaxTitleLength = 100;
        public const int MaxDescriptionLength = 5000;
        public const int MaxTagsCount = 500;
        public const int MaxTagLength = 30;
        public const long MaxFileSizeBytes = 4_294_967_296; // 4GB
        public const int MinDurationSeconds = 1;
        public const int MaxDurationSeconds = 3600; // 60 minutes
        public const int MaxShortsDurationSeconds = 180; // 3 minutes for shorts
        public static readonly string[] AllowedFormats = { "mp4", "mov", "avi", "mkv", "flv", "webm" };
        public static readonly string[] AllowedVideoCodecs = { "h264", "h265", "vp9", "av1" };
    }

    public static class User
    {
        public const int MaxDisplayNameLength = 100;
        public const int MaxEmailLength = 254;
        public const long FreeStorageQuotaBytes = 107_374_182_400; // 100GB
        public const long ProStorageQuotaBytes = 1_099_511_627_776; // 1TB
        public const long EnterpriseStorageQuotaBytes = 10_995_116_277_760; // 10TB
        public const int FreeVideosPerMonth = 5;
        public const int ProVideosPerMonth = 100;
    }

    public static class Processing
    {
        public const int MaxRetries = 3;
        public const int ProcessingTimeoutSeconds = 1800; // 30 minutes
        public const int EncodingTimeoutSeconds = 3600; // 60 minutes
        public const int DefaultWorkerThreads = 4;
        public const string DefaultOutputFormat = "mp4";
        public const int DefaultBitrateMbps = 15;
        public const int DefaultFrameRate = 30;
        public const int DefaultResolution = 1080;
    }

    public static class Upload
    {
        public const int MaxUploadRetries = 3;
        public const int UploadTimeoutSeconds = 3600; // 60 minutes per upload
        public const int ChunkSizeBytes = 262_144_000; // 250MB chunks
        public const string YouTubeCategory = "24"; // Entertainment
        public const bool DefaultPrivacy = false; // Public
        public static readonly string[] ProhibitedWords = { "restricted", "prohibited" };
    }

    public static class Analytics
    {
        public const int MetricsRefreshIntervalMinutes = 60;
        public const int MetricsRetentionDays = 365;
        public const double MinEngagementScoreThreshold = 2.0;
        public const double GoodRetentionSeconds = 30;
        public const int HighPerformanceViewCount = 1000;
    }

    public static class Cache
    {
        public const int DefaultCacheExpirationMinutes = 30;
        public const int AnalyticsCacheExpirationMinutes = 5;
        public const int UserCacheExpirationMinutes = 60;
    }

    public static class Validation
    {
        public const string EmailRegex = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
        public const string UrlRegex = @"^https?:\/\/.+$";
        public const string AlphanumericRegex = @"^[a-zA-Z0-9]+$";
    }

    public static class TimeZones
    {
        public const string DefaultTimeZone = "UTC";
        public static readonly string[] SupportedTimeZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => tz.Id)
            .ToArray();
    }

    public static class FeatureFlags
    {
        public const string AdvancedAnalytics = "advanced_analytics";
        public const string AutoOptimization = "auto_optimization";
        public const string ScheduledUploads = "scheduled_uploads";
        public const string MultiChannelSupport = "multi_channel_support";
        public const string AIThumbnailGeneration = "ai_thumbnail_generation";
    }
}
