// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Configuration;

public class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string LogDirectory { get; set; } = "logs";
    public string ProcessingDirectory { get; set; } = "processing";
    public string OutputDirectory { get; set; } = "output";
    public int MaxConcurrentUploads { get; set; } = 3;
    public int MaxConcurrentProcessing { get; set; } = 2;
    public int DefaultRetryCount { get; set; } = 3;
    public int UploadTimeoutSeconds { get; set; } = 7200;
    public int ProcessingQueueLimit { get; set; } = 100;
    public bool EnableAnalyticsSyncing { get; set; } = true;
    public int AnalyticsSyncIntervalHours { get; set; } = 6;
    public string YouTubeApiKey { get; set; } = string.Empty;
    public string YouTubeClientId { get; set; } = string.Empty;
    public string YouTubeClientSecret { get; set; } = string.Empty;
    public int ScheduleCheckIntervalSeconds { get; set; } = 60;
    public bool EnableWatermark { get; set; } = false;
    public string? WatermarkImagePath { get; set; }
}
