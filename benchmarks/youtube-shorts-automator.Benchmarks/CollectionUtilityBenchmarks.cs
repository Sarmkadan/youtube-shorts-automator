// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Benchmarks for collection utilities, focusing on throughput of batch processing 
/// operations like chunking, grouping, and distinct filtering on collections.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class CollectionUtilityBenchmarks
{
    private List<int> _numbers = null!;
    private const int CollectionSize = 1000;

    [GlobalSetup]
    public void Setup()
    {
        _numbers = Enumerable.Range(0, CollectionSize).ToList();
    }

    [Benchmark(Description = "ChunkBy — 1000 items into 50 chunks")]
    public List<List<int>> ChunkBy() =>
        CollectionUtility.ChunkBy(_numbers, 20);

    [Benchmark(Description = "GroupByToDictionary — group 1000 items")]
    public Dictionary<int, List<int>> GroupByToDictionary() =>
        CollectionUtility.GroupByToDictionary(_numbers, n => n % 10);

    [Benchmark(Description = "DistinctBy — filter 1000 items (100 unique)")]
    public List<int> DistinctBy() =>
        CollectionUtility.DistinctBy(_numbers, n => n % 100).ToList();
}
