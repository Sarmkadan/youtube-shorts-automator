// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using YouTubeShortsAutomator.Caching;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Benchmarks for the in-memory cache service.  All async methods complete
/// synchronously against IMemoryCache, so these measure the ValueTask overhead
/// and the cache lookup cost — the two most frequent operations in the
/// video processing and upload scheduling hot paths.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class CacheServiceBenchmarks
{
    private ICacheService _cache = null!;

    // Pre-populated key — simulates the "processed video path" lookup done
    // before each upload attempt to avoid redundant FFmpeg runs.
    private const string ExistingKey =
        "benchmark:video:processed:channel-42:2026-05";

    private const string MissingKey =
        "benchmark:video:missing:channel-99:2099-99";

    private const string CacheValue =
        "/var/data/output/channel-42/short_1080p_h264.mp4";

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection()
            .AddMemoryCache()
            .AddLogging()
            .AddSingleton<ICacheService, CacheService>();

        _cache = services.BuildServiceProvider()
                         .GetRequiredService<ICacheService>();

        _cache.Set(ExistingKey, CacheValue);
    }

    [Benchmark(Description = "GetAsync — cache hit (sync ValueTask path)")]
    public async ValueTask<string?> GetAsync_Hit() =>
        await _cache.GetAsync<string>(ExistingKey);

    [Benchmark(Description = "GetAsync — cache miss (sync ValueTask path)")]
    public async ValueTask<string?> GetAsync_Miss() =>
        await _cache.GetAsync<string>(MissingKey);

    [Benchmark(Description = "SetAsync — write entry with default 1-hour TTL")]
    public async ValueTask SetAsync() =>
        await _cache.SetAsync(ExistingKey, CacheValue);

    [Benchmark(Description = "RemoveAsync — evict single entry")]
    public async ValueTask RemoveAsync() =>
        await _cache.RemoveAsync(ExistingKey);

    [Benchmark(Description = "Round-trip: SetAsync → GetAsync → RemoveAsync")]
    public async ValueTask RoundTrip()
    {
        const string key = "benchmark:roundtrip:tmp";
        await _cache.SetAsync(key, CacheValue, TimeSpan.FromSeconds(30));
        await _cache.GetAsync<string>(key);
        await _cache.RemoveAsync(key);
    }
}
