using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories
{
    /// <summary>
    /// Extension methods for <see cref="ScheduleRepository"/> providing common schedule query operations.
    /// </summary>
    public static class ScheduleRepositoryExtensions
    {
        /// <summary>
        /// Gets the next due schedule for a specific user.
        /// </summary>
        /// <param name="repository">The schedule repository instance.</param>
        /// <param name="userId">The user identifier to filter schedules by.</param>
        /// <returns>The next due schedule for the specified user, or <see langword="null"/> if none exists.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
        public static async Task<UploadSchedule?> GetNextDueScheduleForUserAsync(
            this ScheduleRepository repository,
            Guid userId)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var dueSchedules = await repository.GetDueSchedulesAsync();
            return dueSchedules
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.NextScheduledTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets all schedules for a user with pagination support.
        /// </summary>
        /// <param name="repository">The schedule repository instance.</param>
        /// <param name="userId">The user identifier to filter schedules by.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A tuple containing the paginated schedules and the total count.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than 1.</exception>
        public static async Task<(List<UploadSchedule> Schedules, int Total)> GetByUserIdAsync(
            this ScheduleRepository repository,
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

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
        /// Gets active schedules filtered by frequency type.
        /// </summary>
        /// <param name="repository">The schedule repository instance.</param>
        /// <param name="frequency">The frequency type to filter by.</param>
        /// <returns>A list of active schedules matching the specified frequency, ordered by next scheduled time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
        public static async Task<List<UploadSchedule>> GetActiveSchedulesByFrequencyAsync(
            this ScheduleRepository repository,
            ScheduleFrequency frequency)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var activeSchedules = await repository.GetActiveSchedulesAsync();
            return activeSchedules
                .Where(s => s.Frequency == frequency)
                .OrderBy(s => s.NextScheduledTime)
                .ToList();
        }

        /// <summary>
        /// Gets schedules that are due within a specific time window.
        /// </summary>
        /// <param name="repository">The schedule repository instance.</param>
        /// <param name="startTime">The start of the time window (inclusive).</param>
        /// <param name="endTime">The end of the time window (inclusive).</param>
        /// <returns>A list of schedules due within the specified time window, ordered by next scheduled time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is after <paramref name="endTime"/></exception>
        public static async Task<List<UploadSchedule>> GetDueSchedulesInWindowAsync(
            this ScheduleRepository repository,
            DateTime startTime,
            DateTime endTime)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime);

            var dueSchedules = await repository.GetDueSchedulesAsync();
            return dueSchedules
                .Where(s => s.NextScheduledTime >= startTime && s.NextScheduledTime <= endTime)
                .OrderBy(s => s.NextScheduledTime)
                .ToList();
        }
    }
}