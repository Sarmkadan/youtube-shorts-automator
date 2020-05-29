// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Moq;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension helper methods for <see cref="AnalyticsDataTests"/>.
/// </summary>
public static class AnalyticsDataTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="AnalyticsData"/> instance with the specified counts.
    /// </summary>
    /// <param name="sut">The test instance.</param>
    /// <param name="viewCount">The view count.</param>
    /// <param name="likeCount">The like count.</param>
    /// <param name="commentCount">The comment count.</param>
    /// <param name="shareCount">The share count.</param>
    /// <returns>A configured <see cref="AnalyticsData"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static AnalyticsData CreateAnalytics(
        this AnalyticsDataTests sut,
        long viewCount = 1000,
        long likeCount = 50,
        long commentCount = 20,
        long shareCount = 10)
    {
        ArgumentNullException.ThrowIfNull(sut);
        
        return new AnalyticsData
        {
            ViewCount = viewCount,
            LikeCount = likeCount,
            CommentCount = commentCount,
            ShareCount = shareCount
        };
    }

    /// <summary>
    /// Creates a mocked <see cref="IRepository{VideoShort}"/> configured to return the provided videos.
    /// </summary>
    /// <param name="sut">The test instance.</param>
    /// <param name="videos">The videos to be returned by the repository.</param>
    /// <returns>A configured <see cref="Mock{IRepository{VideoShort}}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> or <paramref name="videos"/> is null.</exception>
    public static Mock<IRepository<VideoShort>> CreateMockRepository(
        this AnalyticsDataTests sut,
        IEnumerable<VideoShort> videos)
    {
        ArgumentNullException.ThrowIfNull(sut);
        ArgumentNullException.ThrowIfNull(videos);

        var mockRepository = new Mock<IRepository<VideoShort>>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(videos.ToList());
        
        return mockRepository;
    }
}
