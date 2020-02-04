// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UploadSchedule entity
/// </summary>
public class ScheduleRepository : Repository<UploadSchedule>, IScheduleRepository
{
    public ScheduleRepository(ApplicationDbContext context, ILogger<ScheduleRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets schedules for a user
    /// </summary>
    public async Task<List<UploadSchedule>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving schedules for user {userId}");
        return await _dbSet
            .Where(s => s.UserId == userId)
            .Include(s => s.ScheduledUploads)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active schedules
    /// </summary>
    public async Task<List<UploadSchedule>> GetActiveSchedulesAsync()
    {
        _logger.LogInformation("Retrieving active schedules");
        return await _dbSet
            .Where(s => s.IsActive)
            .Include(s => s.ScheduledUploads)
            .OrderBy(s => s.NextScheduledTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets schedules due for execution
    /// </summary>
    public async Task<List<UploadSchedule>> GetDueSchedulesAsync()
    {
        _logger.LogInformation("Retrieving schedules due for execution");
        var now = DateTime.UtcNow;

        return await _dbSet
            .Where(s => s.IsActive &&
                       s.NextScheduledTime != null &&
                       s.NextScheduledTime <= now)
            .Include(s => s.ScheduledUploads)
            .OrderBy(s => s.NextScheduledTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets schedules by frequency
    /// </summary>
    public async Task<List<UploadSchedule>> GetByFrequencyAsync(ScheduleFrequency frequency)
    {
        _logger.LogInformation($"Retrieving schedules with frequency {frequency}");
        return await _dbSet
            .Where(s => s.Frequency == frequency && s.IsActive)
            .Include(s => s.ScheduledUploads)
            .OrderBy(s => s.NextScheduledTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a scheduled upload
    /// </summary>
    public async Task<ScheduledUpload?> GetScheduledUploadAsync(Guid uploadId)
    {
        _logger.LogInformation($"Retrieving scheduled upload {uploadId}");
        return await _context.ScheduledUploads
            .FirstOrDefaultAsync(su => su.Id == uploadId);
    }

    /// <summary>
    /// Updates a scheduled upload
    /// </summary>
    public async Task UpdateScheduledUploadAsync(ScheduledUpload scheduledUpload)
    {
        _logger.LogInformation($"Updating scheduled upload {scheduledUpload.Id}");
        _context.ScheduledUploads.Update(scheduledUpload);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets paginated schedules
    /// </summary>
    public async Task<(List<UploadSchedule> Schedules, int Total)> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation($"Retrieving paginated schedules: page {pageNumber}, size {pageSize}");

        var total = await _dbSet.CountAsync();
        var schedules = await _dbSet
            .OrderByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.ScheduledUploads)
            .ToListAsync();

        return (schedules, total);
    }
}
