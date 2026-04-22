// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Caching.Memory;

namespace YouTubeShortsAutomator.Caching;

/// <summary>
/// In-memory caching service with TTL support.
/// Provides key-value caching with automatic expiration.
/// </summary>
public interface ICacheService
{
    T? Get<T>(string key);
    // ValueTask avoids a Task object heap allocation on the sync-completion path
    // (IMemoryCache operations are always synchronous).
    ValueTask<T?> GetAsync<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    ValueTask RemoveAsync(string key);
    void RemoveByPattern(string pattern);
    bool Exists(string key);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly HashSet<string> _cacheKeys;
    private readonly object _lockObj = new();

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _cacheKeys = new HashSet<string>();
    }

    public T? Get<T>(string key)
    {
        try
        {
            _memoryCache.TryGetValue(key, out T? value);
            if (value != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
            }
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache. Key: {Key}", key);
            return default;
        }
    }

    public ValueTask<T?> GetAsync<T>(string key) =>
        ValueTask.FromResult(Get<T>(key));

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
            }
            else
            {
                // Default 1 hour expiration
                cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }

            _memoryCache.Set(key, value, cacheOptions);

            lock (_lockObj)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}ms",
                key, expiration?.TotalMilliseconds ?? 3600000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache. Key: {Key}", key);
        }
    }

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        Set(key, value, expiration);
        return ValueTask.CompletedTask;
    }

    public void Remove(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            lock (_lockObj)
            {
                _cacheKeys.Remove(key);
            }
            _logger.LogDebug("Cache entry removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry. Key: {Key}", key);
        }
    }

    public ValueTask RemoveAsync(string key)
    {
        Remove(key);
        return ValueTask.CompletedTask;
    }

    public void RemoveByPattern(string pattern)
    {
        try
        {
            lock (_lockObj)
            {
                var keysToRemove = _cacheKeys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.Remove(key);
                }

                _logger.LogInformation("Removed {Count} cache entries matching pattern: {Pattern}",
                    keysToRemove.Count, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
        }
    }

    public bool Exists(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }
}
