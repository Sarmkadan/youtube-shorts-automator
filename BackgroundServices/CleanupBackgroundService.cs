// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.BackgroundServices;

/// <summary>
/// Background service for cleanup operations
/// Removes old files, logs, and expired cache entries
/// </summary>
public class CleanupBackgroundService : BackgroundService
{
    private readonly ILogger<CleanupBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public CleanupBackgroundService(
        ILogger<CleanupBackgroundService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cleanup Background Service started");

        // Allow service to start before first cleanup
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cleanup Background Service is shutting down");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting cleanup operations");

        try
        {
            // Clean temporary files
            await CleanTemporaryFilesAsync();

            // Clean old logs
            await CleanOldLogsAsync();

            // Clean temporary upload directories
            await CleanTemporaryDirectoriesAsync();

            _logger.LogInformation("Cleanup operations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup operations");
        }
    }

    private async Task CleanTemporaryFilesAsync()
    {
        var tempDir = _configuration.GetValue<string>("Storage:TempDirectory") ?? Path.GetTempPath();

        if (!Directory.Exists(tempDir))
            return;

        try
        {
            var files = Directory.GetFiles(tempDir);
            var cutoffTime = DateTime.UtcNow.AddHours(-24);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffTime)
                {
                    await FileSystemUtility.DeleteFileAsync(file);
                    _logger.LogDebug("Deleted temporary file: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning temporary files");
        }
    }

    private async Task CleanOldLogsAsync()
    {
        var logsDir = _configuration.GetValue<string>("Logging:LogsDirectory") ?? "logs";

        if (!Directory.Exists(logsDir))
            return;

        try
        {
            var files = Directory.GetFiles(logsDir, "*.txt");
            var cutoffTime = DateTime.UtcNow.AddDays(-7);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffTime)
                {
                    await FileSystemUtility.DeleteFileAsync(file);
                    _logger.LogDebug("Deleted old log file: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning old logs");
        }
    }

    private async Task CleanTemporaryDirectoriesAsync()
    {
        var baseDir = _configuration.GetValue<string>("Storage:BaseDirectory") ?? "videos";
        var tempSubDir = Path.Combine(baseDir, "temp");

        if (!Directory.Exists(tempSubDir))
            return;

        try
        {
            var directories = Directory.GetDirectories(tempSubDir);
            var cutoffTime = DateTime.UtcNow.AddHours(-12);

            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                if (dirInfo.LastWriteTimeUtc < cutoffTime)
                {
                    var size = FileSystemUtility.GetDirectorySizeBytes(dir);
                    await FileSystemUtility.DeleteDirectoryAsync(dir, recursive: true);
                    _logger.LogInformation("Deleted temporary directory: {Directory} ({SizeMB}MB)",
                        dir, size / 1_048_576);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning temporary directories");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleanup Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
