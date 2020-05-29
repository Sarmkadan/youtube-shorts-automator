using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortsAutomator.Models;

/// <summary>
/// Extension methods for <see cref="ApiResult"/> and <see cref="ApiResult{T}"/>.
/// </summary>
public static class ApiResultExtensions
{
    /// <summary>
    /// Ensures that the API result indicates success.
    /// </summary>
    /// <param name="result">The API result to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when result indicates failure.</exception>
    public static void EnsureSuccess(this ApiResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Operation failed: {result.Message} (Code: {result.ErrorCode})");
        }
    }

    /// <summary>
    /// Ensures that the API result indicates success and returns the contained data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="result">The API result to check.</param>
    /// <returns>The data contained in the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when result indicates failure.</exception>
    public static T EnsureSuccess<T>(this ApiResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException($"Operation failed: {result.Message} (Code: {result.ErrorCode})");
        }
        return result.Data;
    }

    /// <summary>
    /// Gets the errors from the API result as a read-only list of strings.
    /// </summary>
    /// <param name="result">The API result to get errors from.</param>
    /// <returns>A read-only list of error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    public static IReadOnlyList<string> GetErrorsAsReadOnlyList(this ApiResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Errors?.Values.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    /// <summary>
    /// Gets the errors from the generic API result as a read-only list of strings.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="result">The API result to get errors from.</param>
    /// <returns>A read-only list of error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    public static IReadOnlyList<string> GetErrorsAsReadOnlyList<T>(this ApiResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Errors?.Values.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
    }
}
