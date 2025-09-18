// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

public static class Constants
{
    // Video constraints
    public const int MAX_DURATION_SECONDS = 60;
    public const int MIN_DURATION_SECONDS = 1;
    public const int MAX_FILE_SIZE_MB = 512;
    public const int MIN_WIDTH_PIXELS = 360;
    public const int MIN_HEIGHT_PIXELS = 640;
    public const int MAX_WIDTH_PIXELS = 1920;
    public const int MAX_HEIGHT_PIXELS = 1080;

    // YouTube limits
    public const int MAX_TITLE_LENGTH = 100;
    public const int MAX_DESCRIPTION_LENGTH = 5000;
    public const int MAX_TAGS_COUNT = 500;
    public const int MAX_TAG_LENGTH = 30;

    // Processing
    public const int DEFAULT_RETRY_COUNT = 3;
    public const int DEFAULT_UPLOAD_TIMEOUT_SECONDS = 7200;
    public const int PROCESSING_QUEUE_LIMIT = 100;
    public const int BATCH_PROCESSING_SIZE = 5;

    // FFmpeg defaults
    public const string DEFAULT_VIDEO_CODEC = "libx264";
    public const string DEFAULT_AUDIO_CODEC = "aac";
    public const string DEFAULT_CONTAINER = "mp4";
    public const int DEFAULT_BITRATE_KBPS = 2000;
    public const int DEFAULT_AUDIO_BITRATE_KBPS = 128;
    public const int DEFAULT_FPS = 30;

    // Paths and formats
    public const string LOG_DIRECTORY = "logs";
    public const string PROCESSING_DIRECTORY = "processing";
    public const string OUTPUT_DIRECTORY = "output";
    public const string TEMP_DIRECTORY = "temp";
    public const string SUPPORTED_INPUT_FORMATS = "mp4,avi,mov,mkv,flv,wmv";
    public const string OUTPUT_FORMAT = "mp4";

    // Timeouts
    public const int API_TIMEOUT_SECONDS = 30;
    public const int DATABASE_TIMEOUT_SECONDS = 30;
    public const int FFMPEG_TIMEOUT_SECONDS = 3600;

    // Batch and scheduling
    public const int SCHEDULE_CHECK_INTERVAL_SECONDS = 60;
    public const int ANALYTICS_SYNC_INTERVAL_HOURS = 6;
    public const int TOKEN_REFRESH_BUFFER_HOURS = 1;

    // File validation
    public const long MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
    public const long MIN_FILE_SIZE_BYTES = 1024; // 1 KB minimum

    // Performance optimization
    public const int MAX_CONCURRENT_UPLOADS = 3;
    public const int MAX_CONCURRENT_PROCESSING_TASKS = 2;
    public const int CACHE_EXPIRATION_MINUTES = 30;
}
