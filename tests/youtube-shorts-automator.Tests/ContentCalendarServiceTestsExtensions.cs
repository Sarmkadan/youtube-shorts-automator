// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for <see cref="ContentCalendarServiceTests"/> to assist in test execution.
/// </summary>
public static class ContentCalendarServiceTestsExtensions
{
    /// <summary>
    /// Executes a comprehensive suite of content calendar entry creation tests.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task RunCreateEntrySuiteAsync(this ContentCalendarServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.CreateEntryAsync_WithValidEntry_ReturnsPersistedEntry();
        await tests.CreateEntryAsync_NullEntry_ThrowsArgumentNullException();
        await tests.CreateEntryAsync_EmptyTitle_ThrowsValidationException();
        await tests.CreateEntryAsync_TitleTooLong_ThrowsValidationException();
        await tests.CreateEntryAsync_InvalidChannelId_ThrowsValidationException();
        await tests.CreateEntryAsync_TooManyTags_ThrowsValidationException();
        await tests.CreateEntryAsync_SetsCreatedAtAndUpdatedAt();
    }

    /// <summary>
    /// Executes a comprehensive suite of content calendar entry retrieval tests.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task RunGetEntrySuiteAsync(this ContentCalendarServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.GetEntryAsync_ExistingEntry_ReturnsIt();
        await tests.GetEntryAsync_NonExistingEntry_ReturnsNull();
        await tests.GetEntriesInRangeAsync_StartAfterEnd_ThrowsArgumentException();
        await tests.GetEntriesInRangeAsync_ValidRange_DelegatesToRepository();
    }

    /// <summary>
    /// Executes a suite of content calendar entry deletion tests.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task RunDeleteEntrySuiteAsync(this ContentCalendarServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.DeleteEntryAsync_ExistingEntry_ReturnsTrue();
        await tests.DeleteEntryAsync_NonExistingEntry_ReturnsFalse();
    }
}
