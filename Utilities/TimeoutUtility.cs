// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Utilities for handling timeouts and deadline management
/// Provides timeout configuration and expiration checking
/// </summary>
public static class TimeoutUtility
{
    public static bool IsExpired(DateTime deadline)
    {
        return DateTime.UtcNow > deadline;
    }

    public static bool IsExpired(DateTime deadline, out TimeSpan timeRemaining)
    {
        var remaining = deadline - DateTime.UtcNow;
        timeRemaining = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        return remaining <= TimeSpan.Zero;
    }

    public static TimeSpan GetTimeRemaining(DateTime deadline)
    {
        var remaining = deadline - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public static DateTime GetDeadline(TimeSpan timeout)
    {
        return DateTime.UtcNow.Add(timeout);
    }

    public static DateTime GetDeadline(int timeoutSeconds)
    {
        return DateTime.UtcNow.AddSeconds(timeoutSeconds);
    }

    public static DateTime GetDeadline(int timeoutMinutes, int timeoutSeconds = 0)
    {
        return DateTime.UtcNow.AddMinutes(timeoutMinutes).AddSeconds(timeoutSeconds);
    }

    public static CancellationToken CreateCancellationToken(TimeSpan timeout)
    {
        var cts = new CancellationTokenSource(timeout);
        return cts.Token;
    }

    public static CancellationToken CreateCancellationToken(int timeoutSeconds)
    {
        var cts = new CancellationTokenSource(timeoutSeconds * 1000);
        return cts.Token;
    }

    public static async Task<T?> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T?>> operation,
        int timeoutSeconds)
    {
        using (var cts = new CancellationTokenSource(timeoutSeconds * 1000))
        {
            try
            {
                return await operation(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return default;
            }
        }
    }

    public static async Task ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task> operation,
        int timeoutSeconds)
    {
        using (var cts = new CancellationTokenSource(timeoutSeconds * 1000))
        {
            try
            {
                await operation(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Timeout occurred, continue
            }
        }
    }

    public static T? ExecuteWithTimeout<T>(
        Func<CancellationToken, T?> operation,
        int timeoutSeconds)
    {
        using (var cts = new CancellationTokenSource(timeoutSeconds * 1000))
        {
            try
            {
                return operation(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return default;
            }
        }
    }

    public static bool IsWithinTimeWindow(DateTime targetTime, TimeSpan window)
    {
        var now = DateTime.UtcNow;
        return now >= targetTime - window && now <= targetTime + window;
    }

    public static TimeSpan CalculateBackoffDelay(int attemptNumber, int baseDelaySeconds = 1, int maxDelaySeconds = 300)
    {
        // Exponential backoff: 1s, 2s, 4s, 8s, etc., up to maxDelay
        var delaySeconds = Math.Min(
            baseDelaySeconds * (int)Math.Pow(2, attemptNumber - 1),
            maxDelaySeconds);

        return TimeSpan.FromSeconds(delaySeconds);
    }

    public static TimeSpan CalculateBackoffDelayWithJitter(
        int attemptNumber,
        int baseDelaySeconds = 1,
        int maxDelaySeconds = 300)
    {
        var baseDelay = CalculateBackoffDelay(attemptNumber, baseDelaySeconds, maxDelaySeconds);
        var jitterMs = Random.Shared.Next((int)(baseDelay.TotalMilliseconds * 0.1));
        return baseDelay.Add(TimeSpan.FromMilliseconds(jitterMs));
    }
}
