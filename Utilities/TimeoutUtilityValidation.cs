// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation utilities for TimeoutUtility class
// Provides validation, checking, and exception throwing for timeout operations
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="TimeoutUtility"/> operations
/// </summary>
public static class TimeoutUtilityValidation
{
    /// <summary>
    /// Validates TimeoutUtility operations and returns a list of human-readable problems
    /// </summary>
    /// <param name="timeoutSeconds">Timeout value in seconds to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    public static IReadOnlyList<string> Validate(int timeoutSeconds)
    {
        if (timeoutSeconds < 0)
        {
            return new[] { "Timeout seconds cannot be negative" };
        }

        var errors = new List<string>();

        // Validate that TimeoutUtility methods work correctly with various timeout values
        // These validations test the actual TimeoutUtility behavior

        try
        {
            var deadlineFromTimeSpan = TimeoutUtility.GetDeadline(TimeSpan.FromSeconds(timeoutSeconds));
            if (timeoutSeconds == 0 && deadlineFromTimeSpan == DateTime.MinValue)
            {
                errors.Add("GetDeadline(TimeSpan) returns default DateTime.MinValue for zero timeout");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GetDeadline(TimeSpan) throws exception for timeout: {ex.Message}");
        }

        try
        {
            var deadlineFromSeconds = TimeoutUtility.GetDeadline(timeoutSeconds);
            if (timeoutSeconds == 0 && deadlineFromSeconds == DateTime.MinValue)
            {
                errors.Add("GetDeadline(int) returns default DateTime.MinValue for zero timeout");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GetDeadline(int) throws exception for timeout: {ex.Message}");
        }

        try
        {
            var deadlineFromMinutes = TimeoutUtility.GetDeadline(timeoutSeconds / 60, timeoutSeconds % 60);
            if (timeoutSeconds == 0 && deadlineFromMinutes == DateTime.MinValue)
            {
                errors.Add("GetDeadline(int, int) returns default DateTime.MinValue for zero timeout");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GetDeadline(int, int) throws exception for timeout: {ex.Message}");
        }

        // Validate cancellation token creation
        try
        {
            var token = TimeoutUtility.CreateCancellationToken(timeoutSeconds);
            if (timeoutSeconds <= 0 && !token.IsCancellationRequested)
            {
                errors.Add("CreateCancellationToken should immediately cancel for non-positive timeout seconds");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"CreateCancellationToken throws exception for timeout: {ex.Message}");
        }

        // Validate time remaining calculation with edge cases
        try
        {
            var maxDateTime = DateTime.MaxValue;
            var timeRemaining = TimeoutUtility.GetTimeRemaining(maxDateTime);
            if (timeRemaining != TimeSpan.Zero)
            {
                errors.Add("GetTimeRemaining(DateTime) should return TimeSpan.Zero for DateTime.MaxValue");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GetTimeRemaining throws exception: {ex.Message}");
        }

        // Validate IsExpired with edge cases
        try
        {
            var pastDeadline = DateTime.UtcNow.AddHours(-1);
            if (!TimeoutUtility.IsExpired(pastDeadline))
            {
                errors.Add("IsExpired(DateTime) should return true for past deadline");
            }

            var futureDeadline = DateTime.UtcNow.AddHours(1);
            if (TimeoutUtility.IsExpired(futureDeadline))
            {
                errors.Add("IsExpired(DateTime) should return false for future deadline");
            }

            var expiredWithOutParam = TimeoutUtility.IsExpired(pastDeadline, out var timeRemaining);
            if (!expiredWithOutParam || timeRemaining != TimeSpan.Zero)
            {
                errors.Add("IsExpired(DateTime, out TimeSpan) should return true with TimeSpan.Zero for past deadline");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"IsExpired validation throws exception: {ex.Message}");
        }

        // Validate time window calculation
        try
        {
            var now = DateTime.UtcNow;
            if (!TimeoutUtility.IsWithinTimeWindow(now, TimeSpan.FromMinutes(5)))
            {
                errors.Add("IsWithinTimeWindow should return true for current time within window");
            }

            if (TimeoutUtility.IsWithinTimeWindow(now, TimeSpan.Zero))
            {
                errors.Add("IsWithinTimeWindow should return false for zero time window");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"IsWithinTimeWindow validation throws exception: {ex.Message}");
        }

        // Validate backoff delay calculations
        try
        {
            var backoffDelay = TimeoutUtility.CalculateBackoffDelay(0);
            if (backoffDelay != TimeSpan.FromSeconds(1))
            {
                errors.Add("CalculateBackoffDelay should return 1 second for attempt 0 (minimum 1 second)");
            }

            var largeAttemptBackoff = TimeoutUtility.CalculateBackoffDelay(20);
            if (largeAttemptBackoff.TotalSeconds > 300)
            {
                errors.Add("CalculateBackoffDelay should respect maxDelaySeconds parameter");
            }

            var jitterBackoff = TimeoutUtility.CalculateBackoffDelayWithJitter(1);
            if (jitterBackoff.TotalSeconds < 0.9 || jitterBackoff.TotalSeconds > 1.1)
            {
                errors.Add("CalculateBackoffDelayWithJitter should maintain base delay with small jitter");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Backoff delay validation throws exception: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if timeout seconds value is valid
    /// </summary>
    /// <param name="timeoutSeconds">Timeout value in seconds to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(int timeoutSeconds)
    {
        return Validate(timeoutSeconds).Count == 0;
    }

    /// <summary>
    /// Ensures timeout seconds value is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="timeoutSeconds">Timeout value in seconds to validate</param>
    /// <exception cref="ArgumentException">Thrown when timeoutSeconds is negative</exception>
    public static void EnsureValid(int timeoutSeconds)
    {
        var errors = Validate(timeoutSeconds);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"TimeoutUtility validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}