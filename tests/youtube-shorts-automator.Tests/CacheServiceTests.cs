// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using YouTubeShortsAutomator.Caching;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Unit tests for the CacheService class.
/// Tests cache retrieval, storage, expiration, removal, pattern matching, and asynchronous operations.
/// </summary>
public class CacheServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache;
    private readonly CacheService _service;

    /// <summary>
    /// Creates a cache service backed by a real in-memory cache for each test.
    /// </summary>
    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new CacheService(_memoryCache, NullLogger<CacheService>.Instance);
    }

    [Fact]
    public void Get_WhenKeyDoesNotExist_ReturnsDefault()
    {
        _service.Get<string>("missing-reference").Should().BeNull();
        _service.Get<int>("missing-value").Should().Be(default);
    }

    [Fact]
    public void Get_WhenKeyExists_ReturnsValue()
    {
        _service.Set("existing", "cached value");

        var result = _service.Get<string>("existing");

        result.Should().Be("cached value");
    }

    [Fact]
    public void SetThenGet_WithReferenceType_RoundTripsSameInstance()
    {
        var value = new CacheValue("reference value");

        _service.Set("reference", value);

        _service.Get<CacheValue>("reference").Should().BeSameAs(value);
    }

    [Fact]
    public void SetThenGet_WithValueType_RoundTripsValue()
    {
        _service.Set("value", 42);

        _service.Get<int>("value").Should().Be(42);
    }

    [Fact]
    public async Task Set_WithShortExpiration_ExpiresEntry()
    {
        _service.Set("expiring", "value", TimeSpan.FromMilliseconds(20));

        await Task.Delay(TimeSpan.FromMilliseconds(150));

        _service.Get<string>("expiring").Should().BeNull();
        _service.Exists("expiring").Should().BeFalse();
    }

    [Fact]
    public void Remove_WhenKeyExists_DeletesEntry()
    {
        _service.Set("removable", "value");

        _service.Remove("removable");

        _service.Get<string>("removable").Should().BeNull();
    }

    [Fact]
    public void Exists_ReflectsEntryPresence()
    {
        _service.Exists("presence").Should().BeFalse();

        _service.Set("presence", "value");

        _service.Exists("presence").Should().BeTrue();
    }

    [Fact]
    public void RemoveByPattern_RemovesOnlyMatchingKeys()
    {
        _service.Set("video:one", 1);
        _service.Set("video:two", 2);
        _service.Set("channel:one", 3);

        _service.RemoveByPattern("video:");

        _service.Exists("video:one").Should().BeFalse();
        _service.Exists("video:two").Should().BeFalse();
        _service.Get<int>("channel:one").Should().Be(3);
    }

    [Fact]
    public async Task AsyncOperations_BehaveLikeSyncOperations()
    {
        (await _service.GetAsync<string>("async")).Should().BeNull();

        await _service.SetAsync("async", "value");

        (await _service.GetAsync<string>("async")).Should().Be("value");
        _service.Exists("async").Should().BeTrue();

        await _service.RemoveAsync("async");

        (await _service.GetAsync<string>("async")).Should().BeNull();
        _service.Exists("async").Should().BeFalse();
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed record CacheValue(string Value);
}
