// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for MetricsController response models
// Provides validation for metrics API response data integrity
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Validation helpers for MetricsController response models
/// Validates metrics response data integrity
/// </summary>
public static class MetricsControllerValidation
{
    /// <summary>
    /// Validates the SystemMetricsResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The SystemMetricsResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this SystemMetricsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate CapturedAtUtc
        if (value.CapturedAtUtc == default)
        {
            problems.Add("CapturedAtUtc must be set to a non-default DateTime value");
        }

        // Validate TotalApiCalls
        if (value.TotalApiCalls < 0)
        {
            problems.Add("TotalApiCalls cannot be negative");
        }

        // Validate ProcessingMetrics collection
        if (value.ProcessingMetrics is null)
        {
            problems.Add("ProcessingMetrics collection is null");
        }
        else
        {
            foreach (var metric in value.ProcessingMetrics)
            {
                if (string.IsNullOrWhiteSpace(metric?.ProcessType))
                {
                    problems.Add("ProcessingMetrics contains entries with null or empty ProcessType");
                    break;
                }

                if (metric.Count < 0)
                {
                    problems.Add("ProcessingMetrics contains entries with negative Count");
                    break;
                }

                if (metric.AverageDurationMs < 0)
                {
                    problems.Add("ProcessingMetrics contains entries with negative AverageDurationMs");
                    break;
                }

                if (metric.MinDurationMs < 0)
                {
                    problems.Add("ProcessingMetrics contains entries with negative MinDurationMs");
                    break;
                }

                if (metric.MaxDurationMs < 0)
                {
                    problems.Add("ProcessingMetrics contains entries with negative MaxDurationMs");
                    break;
                }

                if (metric.TotalBytesProcessed < 0)
                {
                    problems.Add("ProcessingMetrics contains entries with negative TotalBytesProcessed");
                    break;
                }
            }
        }

        // Validate ErrorCounts dictionary
        if (value.ErrorCounts is null)
        {
            problems.Add("ErrorCounts dictionary is null");
        }
        else
        {
            foreach (var kvp in value.ErrorCounts)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("ErrorCounts contains entries with null or empty error code keys");
                    break;
                }

                if (kvp.Value < 0)
                {
                    problems.Add("ErrorCounts contains entries with negative error counts");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ProcessingMetricResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ProcessingMetricResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ProcessingMetricResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ProcessType))
        {
            problems.Add("ProcessType cannot be null or empty");
        }

        if (value.Count < 0)
        {
            problems.Add("Count cannot be negative");
        }

        if (value.AverageDurationMs < 0)
        {
            problems.Add("AverageDurationMs cannot be negative");
        }

        if (value.MinDurationMs < 0)
        {
            problems.Add("MinDurationMs cannot be negative");
        }

        if (value.MaxDurationMs < 0)
        {
            problems.Add("MaxDurationMs cannot be negative");
        }

        if (value.TotalBytesProcessed < 0)
        {
            problems.Add("TotalBytesProcessed cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ApiCallsResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ApiCallsResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ApiCallsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.TotalCalls < 0)
        {
            problems.Add("TotalCalls cannot be negative");
        }

        if (value.Groups is null)
        {
            problems.Add("Groups collection is null");
        }
        else
        {
            foreach (var group in value.Groups)
            {
                if (string.IsNullOrWhiteSpace(group?.Endpoint))
                {
                    problems.Add("Groups contains entries with null or empty Endpoint");
                    break;
                }

                if (group.CallCount < 0)
                {
                    problems.Add("Groups contains entries with negative CallCount");
                    break;
                }

                if (group.AverageDurationMs < 0)
                {
                    problems.Add("Groups contains entries with negative AverageDurationMs");
                    break;
                }

                if (group.SuccessRate < 0 || group.SuccessRate > 100)
                {
                    problems.Add("Groups contains entries with SuccessRate outside valid range [0-100]");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ApiCallGroup instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ApiCallGroup instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ApiCallGroup value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Endpoint))
        {
            problems.Add("Endpoint cannot be null or empty");
        }

        if (value.CallCount < 0)
        {
            problems.Add("CallCount cannot be negative");
        }

        if (value.AverageDurationMs < 0)
        {
            problems.Add("AverageDurationMs cannot be negative");
        }

        if (value.SuccessRate < 0 || value.SuccessRate > 100)
        {
            problems.Add("SuccessRate must be between 0 and 100");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ErrorStatsResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ErrorStatsResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ErrorStatsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.TotalErrors < 0)
        {
            problems.Add("TotalErrors cannot be negative");
        }

        if (value.ErrorsByCode is null)
        {
            problems.Add("ErrorsByCode collection is null");
        }
        else
        {
            foreach (var error in value.ErrorsByCode)
            {
                if (string.IsNullOrWhiteSpace(error?.ErrorCode))
                {
                    problems.Add("ErrorsByCode contains entries with null or empty ErrorCode");
                    break;
                }

                if (error.Count < 0)
                {
                    problems.Add("ErrorsByCode contains entries with negative Count");
                    break;
                }

                if (error.Percentage < 0 || error.Percentage > 100)
                {
                    problems.Add("ErrorsByCode contains entries with Percentage outside valid range [0-100]");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ErrorCount instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ErrorCount instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ErrorCount value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode cannot be null or empty");
        }

        if (value.Count < 0)
        {
            problems.Add("Count cannot be negative");
        }

        if (value.Percentage < 0 || value.Percentage > 100)
        {
            problems.Add("Percentage must be between 0 and 100");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ProcessingPerformanceResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The ProcessingPerformanceResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ProcessingPerformanceResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate CapturedAtUtc
        if (value.CapturedAtUtc == default)
        {
            problems.Add("CapturedAtUtc must be set to a non-default DateTime value");
        }

        // Validate Metrics collection
        if (value.Metrics is null)
        {
            problems.Add("Metrics collection is null");
        }
        else
        {
            foreach (var metric in value.Metrics)
            {
                if (string.IsNullOrWhiteSpace(metric?.ProcessType))
                {
                    problems.Add("Metrics contains entries with null or empty ProcessType");
                    break;
                }

                if (metric.Count < 0)
                {
                    problems.Add("Metrics contains entries with negative Count");
                    break;
                }

                if (metric.AverageDurationMs < 0)
                {
                    problems.Add("Metrics contains entries with negative AverageDurationMs");
                    break;
                }

                if (metric.MinDurationMs < 0)
                {
                    problems.Add("Metrics contains entries with negative MinDurationMs");
                    break;
                }

                if (metric.MaxDurationMs < 0)
                {
                    problems.Add("Metrics contains entries with negative MaxDurationMs");
                    break;
                }

                if (metric.TotalBytesProcessed < 0)
                {
                    problems.Add("Metrics contains entries with negative TotalBytesProcessed");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the SystemMetricsResponse instance is valid
    /// </summary>
    /// <param name="value">The SystemMetricsResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this SystemMetricsResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if the ProcessingMetricResponse instance is valid
    /// </summary>
    /// <param name="value">The ProcessingMetricResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this ProcessingMetricResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if the ApiCallsResponse instance is valid
    /// </summary>
    /// <param name="value">The ApiCallsResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this ApiCallsResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if the ApiCallGroup instance is valid
    /// </summary>
    /// <param name="value">The ApiCallGroup instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this ApiCallGroup value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if the ErrorStatsResponse instance is valid
    /// </summary>
    /// <param name="value">The ErrorStatsResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this ErrorStatsResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if the ErrorCount instance is valid
    /// </summary>
    /// <param name="value">The ErrorCount instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this ErrorCount value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if the ProcessingPerformanceResponse instance is valid
    /// </summary>
    /// <param name="value">The ProcessingPerformanceResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this ProcessingPerformanceResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the SystemMetricsResponse instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The SystemMetricsResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this SystemMetricsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SystemMetricsResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the ProcessingMetricResponse instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The ProcessingMetricResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this ProcessingMetricResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingMetricResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the ApiCallsResponse instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The ApiCallsResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this ApiCallsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiCallsResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the ApiCallGroup instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The ApiCallGroup instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this ApiCallGroup value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiCallGroup validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the ErrorStatsResponse instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The ErrorStatsResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this ErrorStatsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorStatsResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the ErrorCount instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The ErrorCount instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this ErrorCount value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorCount validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the ProcessingPerformanceResponse instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The ProcessingPerformanceResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(this ProcessingPerformanceResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingPerformanceResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}
