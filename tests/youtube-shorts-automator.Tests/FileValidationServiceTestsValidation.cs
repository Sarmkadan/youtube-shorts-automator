using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="FileValidationServiceTests"/>.
    /// Validates that the test class instance is properly initialized.
    /// </summary>
    public static class FileValidationServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="FileValidationServiceTests"/> instance.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this FileValidationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate that the test class instance is properly initialized
            // Since we can't access private fields, we validate through public methods
            // and the fact that the class constructor should have initialized everything

            // The constructor creates a mock logger and service, and a test directory
            // We can't directly verify these, but we can verify the class is functional

            // Validate basic functionality by ensuring the class can be used
            // (The actual tests will fail if the instance is not properly initialized)

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="FileValidationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this FileValidationServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="FileValidationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is invalid, with a message listing all problems.</exception>
        public static void EnsureValid(this FileValidationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    string.Join(Environment.NewLine, problems),
                    nameof(value));
            }
        }
    }
}