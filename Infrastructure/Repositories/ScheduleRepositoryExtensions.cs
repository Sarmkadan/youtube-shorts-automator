using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories
{
    public static class ScheduleRepositoryExtensions
    {
        /// <summary>
        /// Gets the next due schedule for a specific user
        /// </summary>
        public static async Task<UploadSchedule?> GetNextDueScheduleForUserAsync(this ScheduleRepository repository, Guid userId)
        {
            var dueSchedules = await repository.GetDueSchedulesAsync();
            return dueSchedules
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.NextScheduledTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets all schedules for a user with pagination support
        /// </summary>
        public static async Task<(List<UploadSchedule> Schedules, int Total)> GetByUserIdAsync(
            this ScheduleRepository repository,
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var allSchedules = await repository.GetByUserIdAsync(userId);
            var total = allSchedules.Count;
            var paginated = allSchedules
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginated, total);
        }

        /// <summary>
        /// Gets active schedules filtered by frequency type
        /// </summary>
        public static async Task<List<UploadSchedule>> GetActiveSchedulesByFrequencyAsync(
            this ScheduleRepository repository,
            ScheduleFrequency frequency)
        {
            var activeSchedules = await repository.GetActiveSchedulesAsync();
            return activeSchedules
                .Where(s => s.Frequency == frequency)
                .OrderBy(s => s.NextScheduledTime)
                .ToList();
        }

        /// <summary>
        /// Gets schedules that are due within a specific time window
        /// </summary>
        public static async Task<List<UploadSchedule>> GetDueSchedulesInWindowAsync(
            this ScheduleRepository repository,
            DateTime startTime,
            DateTime endTime)
        {
            var dueSchedules = await repository.GetDueSchedulesAsync();
            return dueSchedules
                .Where(s => s.NextScheduledTime >= startTime && s.NextScheduledTime <= endTime)
                .OrderBy(s => s.NextScheduledTime)
                .ToList();
        }
    }
}