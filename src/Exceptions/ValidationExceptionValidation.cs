namespace YouTubeShortAutomator.Exceptions;

public static class ValidationExceptionValidation
{
    /// <summary>
    /// Validates the provided ValidationException instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The ValidationException instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.FieldName))
        {
            problems.Add("Field name is null or empty.");
        }

        if (string.IsNullOrEmpty(value.FieldValue))
        {
            problems.Add("Field value is null or empty.");
        }

        if (value.Errors.Count == 0)
        {
            problems.Add("No errors provided.");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the provided ValidationException instance is valid.
    /// </summary>
    /// <param name="value">The ValidationException instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    public static bool IsValid(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided ValidationException instance is valid.
    /// </summary>
    /// <param name="value">The ValidationException instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
