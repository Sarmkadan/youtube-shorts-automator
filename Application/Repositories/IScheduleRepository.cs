// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Repositories;

/// <summary>
/// Repository for UploadSchedule entity operations
/// </summary>
public interface IScheduleRepository : IRepository<UploadSchedule>
{
    Task<List<UploadSchedule>> GetByUserIdAsync(Guid userId);
    Task<List<UploadSchedule>> GetActiveSchedulesAsync();
    Task<List<UploadSchedule>> GetDueSchedulesAsync();
    Task<List<UploadSchedule>> GetByFrequencyAsync(ScheduleFrequency frequency);
    Task<ScheduledUpload?> GetScheduledUploadAsync(Guid uploadId);
    Task UpdateScheduledUploadAsync(ScheduledUpload scheduledUpload);
    Task<(List<UploadSchedule> Schedules, int Total)> GetPaginatedAsync(int pageNumber, int pageSize);
}
