namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Configuration settings for the YouTube Shorts Automator application.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// The database connection string used by the application.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The file system path to the database file.
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// Directory where log files are stored.
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// Directory where files are processed before upload.
    /// </summary>
    public string ProcessingDirectory { get; set; } = "processing";

    /// <summary>
    /// Directory where processed files are saved.
    /// </summary>
    public string OutputDirectory { get; set; } = "output";

    /// <summary>
    /// Maximum number of uploads that can run concurrently.
    /// </summary>
    public int MaxConcurrentUploads { get; set; } = 3;

    /// <summary>
    /// Maximum number of processing tasks that can run concurrently.
    /// </summary>
    public int MaxConcurrentProcessing { get; set; } = 2;

    /// <summary>
    /// Default number of retry attempts for failed operations.
    /// </summary>
    public int DefaultRetryCount { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for upload operations.
    /// </summary>
    public int UploadTimeoutSeconds { get; set; } = 7200;

    /// <summary>
    /// Maximum number of items allowed in the processing queue.
    /// </summary>
    public int ProcessingQueueLimit { get; set; } = 100;

    /// <summary>
    /// Flag indicating whether analytics data should be synchronized with YouTube.
    /// </summary>
    public bool EnableAnalyticsSyncing { get; set; } = true;

    /// <summary>
    /// Interval in hours between analytics synchronization operations.
    /// </summary>
    public int AnalyticsSyncIntervalHours { get; set; } = 6;

    /// <summary>
    /// API key for accessing the YouTube Data API.
    /// </summary>
    public string YouTubeApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client ID for the YouTube API.
    /// </summary>
    public string YouTubeClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client secret for the YouTube API.
    /// </summary>
    public string YouTubeClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Interval in seconds between schedule checks for new tasks.
    /// </summary>
    public int ScheduleCheckIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Flag indicating whether a watermark should be applied to videos.
    /// </summary>
    public bool EnableWatermark { get; set; } = false;

    /// <summary>
    /// File path to the watermark image to be applied.
    /// </summary>
    public string? WatermarkImagePath { get; set; }
}
