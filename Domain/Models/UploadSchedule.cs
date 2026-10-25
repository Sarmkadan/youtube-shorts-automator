// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public sealed class UploadSchedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Weekly;
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? NextScheduledTime { get; set; }
    public int TotalExecutions { get; set; }
    public List<ScheduledUpload> ScheduledUploads { get; set; } = new();

    /// <summary>
    /// Calculates the next scheduled upload time
    /// </summary>
    public DateTime CalculateNextScheduledTime() => CalculateNextScheduledTime(DateTime.UtcNow);

    /// <summary>
    /// Calculates the next scheduled upload time relative to the given UTC instant
    /// </summary>
    /// <param name="utcNow">Current time in UTC</param>
    public DateTime CalculateNextScheduledTime(DateTime utcNow)
    {
        if (!IsActive)
        {
            return DateTime.MaxValue;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        var nowInZone = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        var nextTime = nowInZone.Date.Add(ScheduledTime.ToTimeSpan());

        if (nextTime <= nowInZone)
        {
            nextTime = nextTime.AddDays(1);
        }

        return nextTime switch
        {
            _ when Frequency == ScheduleFrequency.Daily => nextTime,
            _ when Frequency == ScheduleFrequency.Weekly && DayOfWeek.HasValue =>
                FindNextOccurrence(nextTime, DayOfWeek.Value),
            _ when Frequency == ScheduleFrequency.Monthly && DayOfMonth.HasValue =>
                FindNextMonthlyOccurrence(nextTime, DayOfMonth.Value),
            _ => nextTime
        };
    }

    /// <summary>
    /// Records a successful execution
    /// </summary>
    public void RecordExecution() => RecordExecution(DateTime.UtcNow);

    /// <summary>
    /// Records a successful execution at the given UTC instant
    /// </summary>
    /// <param name="utcNow">Execution time in UTC</param>
    public void RecordExecution(DateTime utcNow)
    {
        LastExecutedAt = utcNow;
        TotalExecutions++;
        NextScheduledTime = CalculateNextScheduledTime(utcNow);
    }

    /// <summary>
    /// Validates the schedule configuration
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ScheduleName) || ScheduleName.Length > 100)
        {
            errors.Add("Schedule name must be between 1 and 100 characters");
        }

        if (Frequency == ScheduleFrequency.Weekly && !DayOfWeek.HasValue)
        {
            errors.Add("Day of week is required for weekly schedules");
        }

        if (Frequency == ScheduleFrequency.Monthly && (!DayOfMonth.HasValue || DayOfMonth < 1 || DayOfMonth > 31))
        {
            errors.Add("Valid day of month (1-31) is required for monthly schedules");
        }

        if (string.IsNullOrWhiteSpace(TimeZoneId))
        {
            errors.Add("Time zone is required");
        }

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch
        {
            errors.Add("Invalid time zone ID");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Checks if a schedule is due for execution
    /// </summary>
    public bool IsDueForExecution() => IsDueForExecution(DateTime.UtcNow);

    /// <summary>
    /// Checks if a schedule is due for execution at the given UTC instant
    /// </summary>
    /// <param name="utcNow">Current time in UTC</param>
    public bool IsDueForExecution(DateTime utcNow)
    {
        if (!IsActive || NextScheduledTime is null)
        {
            return false;
        }

        return utcNow >= NextScheduledTime;
    }

    /// <summary>
    /// Deactivates the schedule
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the schedule
    /// </summary>
    public void Activate() => Activate(DateTime.UtcNow);

    /// <summary>
    /// Activates the schedule and recalculates the next run relative to the given UTC instant
    /// </summary>
    /// <param name="utcNow">Current time in UTC</param>
    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        NextScheduledTime = CalculateNextScheduledTime(utcNow);
    }

    private DateTime FindNextOccurrence(DateTime from, DayOfWeek targetDay)
    {
        var daysUntilTarget = ((int)targetDay - (int)from.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && from.TimeOfDay > ScheduledTime.ToTimeSpan())
        {
            daysUntilTarget = 7;
        }

        return from.AddDays(daysUntilTarget).Date.Add(ScheduledTime.ToTimeSpan());
    }

    private DateTime FindNextMonthlyOccurrence(DateTime from, int dayOfMonth)
    {
        var nextMonth = from.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var targetDay = Math.Min(dayOfMonth, daysInMonth);

        return new DateTime(nextMonth.Year, nextMonth.Month, targetDay).Add(ScheduledTime.ToTimeSpan());
    }
}

public enum ScheduleFrequency
{
    Daily = 0,
    Weekly = 1,
    BiWeekly = 2,
    Monthly = 3,
    Quarterly = 4
}

public sealed class ScheduledUpload
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid? VideoId { get; set; }
    public DateTime ScheduledFor { get; set; }
    public ScheduledUploadStatus Status { get; set; } = ScheduledUploadStatus.Pending;
    public DateTime? ExecutedAt { get; set; }
    public string? ExecutionError { get; set; }
}

public enum ScheduledUploadStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Skipped = 4
}
