namespace YouTubeShortsAutomator.Domain.Exceptions;

/// <summary>
/// Extension methods for <see cref="DomainException"/> and derived types.
/// </summary>
public static class DomainExceptionExtensions
{
    /// <summary>
    /// Checks if the exception has the specified error code.
    /// </summary>
    /// <param name="exception">The domain exception.</param>
    /// <param name="errorCode">The error code to check.</param>
    /// <returns><c>true</c> if the error code matches; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool HasErrorCode(this DomainException exception, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return string.Equals(exception.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds context to the exception fluently.
    /// </summary>
    /// <param name="exception">The domain exception.</param>
    /// <param name="key">The context key.</param>
    /// <param name="value">The context value.</param>
    /// <returns>The same exception instance with the added context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> or <paramref name="key"/> is null.</exception>
    public static DomainException WithContext(this DomainException exception, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);
        
        exception.AddContext(key, value);
        return exception;
    }

    /// <summary>
    /// Safely retrieves a context value.
    /// </summary>
    /// <param name="exception">The domain exception.</param>
    /// <param name="key">The context key.</param>
    /// <param name="value">The retrieved value if found.</param>
    /// <returns><c>true</c> if the key exists and value is of type <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
    public static bool TryGetContextValue<T>(this DomainException exception, string key, out T? value)
    {
        ArgumentNullException.ThrowIfNull(exception);
        
        value = default;
        if (exception.Context != null && exception.Context.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }
        return false;
    }
}
