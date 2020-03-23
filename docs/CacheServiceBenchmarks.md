# CacheServiceBenchmarks

Benchmark class for measuring the performance characteristics of the `ICacheService` implementation used in the YouTube Shorts Automator pipeline. It evaluates cache hit/miss latency, write throughput, eviction cost, and full round-trip scenarios under isolated, repeatable conditions using BenchmarkDotNet.

## API

### `public void Setup()`
Prepares the cache service instance and any required dependencies before each benchmark iteration. This method is invoked automatically by the benchmark harness and is not intended for direct consumption. It does not accept parameters, returns no value, and will propagate any exception thrown during dependency resolution or cache construction.

### `public async ValueTask<string?> GetAsync_Hit()`
Measures the time to retrieve an entry that is guaranteed to be present in the cache. The method asynchronously requests a known key, returns the cached string value (or `null` if an unexpected eviction occurs), and may throw if the underlying cache store is unavailable or the operation is cancelled.

### `public async ValueTask<string?> GetAsync_Miss()`
Measures the time to request a key that is guaranteed to be absent from the cache. Returns `null` on a clean miss. Throws under the same conditions as `GetAsync_Hit`—store failures, task cancellation, or configuration errors that prevent the cache from servicing the request.

### `public async ValueTask SetAsync()`
Measures the time to insert a new string entry into the cache. The method writes a fixed payload under a unique or pre-warmed key. It returns no value (the `ValueTask` completes upon successful insertion). Throws if the cache store rejects the write due to capacity limits, connectivity loss, or serialization faults.

### `public async ValueTask RemoveAsync()`
Measures the time to evict an existing entry from the cache. The method targets a key that was populated during setup or a prior iteration. Returns no value upon completion. Throws if the key is not found (depending on implementation semantics), the store is unavailable, or the operation is cancelled.

### `public async ValueTask RoundTrip()`
Measures the combined cost of a write followed immediately by a read of the same key. The method sets a value and then retrieves it, returning the retrieved string (or `null` if the read fails). Throws under any condition that would cause `SetAsync` or `GetAsync_Hit` to throw.

## Usage

```csharp
// Run all cache benchmarks from a console application
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<CacheServiceBenchmarks>();
Console.WriteLine($"Median hit latency: {summary.Reports
    .First(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == nameof(CacheServiceBenchmarks.GetAsync_Hit))
    .ResultStatistics.Median * 1_000_000:F2} µs");
```

```csharp
// Profile only miss and round-trip scenarios in a CI pipeline
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddFilter(new NameFilter(name => name.Contains("Miss") || name.Contains("RoundTrip")));

BenchmarkRunner.Run<CacheServiceBenchmarks>(config);
```

## Notes

- **Thread safety**: Each benchmark iteration runs on a dedicated thread with its own setup context. The underlying `ICacheService` implementation must be safe for concurrent access if multiple benchmarks are executed in parallel, though the default harness typically isolates iterations.
- **Edge cases**: `GetAsync_Hit` may return `null` if the cache implementation employs time-based expiry and the setup-to-benchmark delay exceeds the TTL. `RemoveAsync` may throw when the implementation treats removal of a non-existent key as an error rather than a no-op. `RoundTrip` can surface inconsistencies where the write succeeds but the subsequent read fails due to eventual consistency or replication lag in distributed cache backends.
- **Warmup**: The harness performs automatic warmup iterations; reported results exclude these. Cold-start effects such as JIT compilation and store connection establishment are therefore not reflected in the final metrics unless explicitly configured otherwise.
