// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Unit tests for the CacheService class.
/// Tests cache retrieval, storage, expiration, removal, pattern matching, and asynchronous operations.
/// </summary>
public class CacheServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache;
    private readonly Type _serviceType;
    private readonly object _service;

    /// <summary>
    /// Creates a cache service backed by a real in-memory cache for each test.
    /// </summary>
    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _serviceType = LoadCacheServiceType();
        var loggerType = typeof(NullLogger<>).MakeGenericType(_serviceType);
        var logger = loggerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!.GetValue(null);
        _service = Activator.CreateInstance(_serviceType, _memoryCache, logger)!;
    }

    [Fact]
    public void Get_WhenKeyDoesNotExist_ReturnsDefault()
    {
        Get<string>("missing-reference").Should().BeNull();
        Get<int>("missing-value").Should().Be(default);
    }

    [Fact]
    public void Get_WhenKeyExists_ReturnsValue()
    {
        Set("existing", "cached value");

        var result = Get<string>("existing");

        result.Should().Be("cached value");
    }

    [Fact]
    public void SetThenGet_WithReferenceType_RoundTripsSameInstance()
    {
        var value = new CacheValue("reference value");

        Set("reference", value);

        Get<CacheValue>("reference").Should().BeSameAs(value);
    }

    [Fact]
    public void SetThenGet_WithValueType_RoundTripsValue()
    {
        Set("value", 42);

        Get<int>("value").Should().Be(42);
    }

    [Fact]
    public async Task Set_WithShortExpiration_ExpiresEntry()
    {
        Set("expiring", "value", TimeSpan.FromMilliseconds(20));

        await Task.Delay(TimeSpan.FromMilliseconds(150));

        Get<string>("expiring").Should().BeNull();
        Exists("expiring").Should().BeFalse();
    }

    [Fact]
    public void Remove_WhenKeyExists_DeletesEntry()
    {
        Set("removable", "value");

        Invoke("Remove", "removable");

        Get<string>("removable").Should().BeNull();
    }

    [Fact]
    public void Exists_ReflectsEntryPresence()
    {
        Exists("presence").Should().BeFalse();

        Set("presence", "value");

        Exists("presence").Should().BeTrue();
    }

    [Fact]
    public void RemoveByPattern_RemovesOnlyMatchingKeys()
    {
        Set("video:one", 1);
        Set("video:two", 2);
        Set("channel:one", 3);

        Invoke("RemoveByPattern", "video:");

        Exists("video:one").Should().BeFalse();
        Exists("video:two").Should().BeFalse();
        Get<int>("channel:one").Should().Be(3);
    }

    [Fact]
    public async Task AsyncOperations_BehaveLikeSyncOperations()
    {
        (await GetAsync<string>("async")).Should().BeNull();

        await SetAsync("async", "value");

        (await GetAsync<string>("async")).Should().Be("value");
        Exists("async").Should().BeTrue();

        await (ValueTask)Invoke("RemoveAsync", "async")!;

        (await GetAsync<string>("async")).Should().BeNull();
        Exists("async").Should().BeFalse();
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    private T? Get<T>(string key) =>
        (T?)InvokeGeneric("Get", typeof(T), key);

    private void Set<T>(string key, T value, TimeSpan? expiration = null) =>
        InvokeGeneric("Set", typeof(T), key, value, expiration);

    private bool Exists(string key) =>
        (bool)Invoke("Exists", key)!;

    private async Task<T?> GetAsync<T>(string key)
    {
        var valueTask = InvokeGeneric("GetAsync", typeof(T), key)!;
        var task = (Task<T?>)valueTask.GetType().GetMethod("AsTask")!.Invoke(valueTask, null)!;
        return await task;
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var valueTask = (ValueTask)InvokeGeneric("SetAsync", typeof(T), key, value, expiration)!;
        await valueTask;
    }

    private object? Invoke(string methodName, params object?[] arguments) =>
        _serviceType.GetMethod(methodName)!.Invoke(_service, arguments);

    private object? InvokeGeneric(string methodName, Type typeArgument, params object?[] arguments) =>
        _serviceType.GetMethod(methodName)!.MakeGenericMethod(typeArgument).Invoke(_service, arguments);

    private static Type LoadCacheServiceType()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "YouTubeShortsAutomator.csproj")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the test output should be located under the repository root");
        var outputDirectory = Path.Combine(directory!.FullName, "bin", "YouTubeShortsAutomator");
        var assemblyPath = Directory.GetFiles(outputDirectory, "YouTubeShortsAutomator.dll", SearchOption.AllDirectories)
            .Single(path => path.Contains($"{Path.DirectorySeparatorChar}net10.0{Path.DirectorySeparatorChar}"));
        var assembly = Assembly.LoadFrom(assemblyPath);

        return assembly.GetType("YouTubeShortsAutomator.Caching.CacheService", throwOnError: true)!;
    }

    private sealed record CacheValue(string Value);
}
