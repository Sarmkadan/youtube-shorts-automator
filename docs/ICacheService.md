# ICacheService

The `ICacheService` interface defines a consistent contract for caching operations within the application, abstracting the underlying storage mechanism. It provides both synchronous and asynchronous methods for retrieving, storing, and removing cached data, enabling efficient performance optimization across different services.

## API

*   **`T? Get<T>(string key)`**
    *   **Purpose:** Retrieves a value associated with the specified key from the cache.
    *   **Parameters:** `key` (string) - The unique identifier for the cached item.
    *   **Return Value:** The deserialized value of type `T`, or `default(T)` if the key does not exist or the value cannot be retrieved.
    *   **Exceptions:** May throw serialization exceptions if the cached data cannot be deserialized to type `T`.

*   **`ValueTask<T?> GetAsync<T>(string key)`**
    *   **Purpose:** Asynchronously retrieves a value associated with the specified key.
    *   **Parameters:** `key` (string) - The unique identifier for the cached item.
    *   **Return Value:** A `ValueTask` containing the deserialized value of type `T`, or `default(T)` if not found.

*   **`void Set<T>(string key, T value, TimeSpan? expiry = null)`**
    *   **Purpose:** Adds or updates a value in the cache with an optional expiration time.
    *   **Parameters:** `key` (string), `value` (T), `expiry` (TimeSpan?, optional).
    *   **Return Value:** `void`.

*   **`ValueTask SetAsync<T>(string key, T value, TimeSpan? expiry = null)`**
    *   **Purpose:** Asynchronously adds or updates a value in the cache with an optional expiration time.
    *   **Parameters:** `key` (string), `value` (T), `expiry` (TimeSpan?, optional).
    *   **Return Value:** A `ValueTask` representing the completion of the operation.

*   **`void Remove(string key)`**
    *   **Purpose:** Removes the item associated with the specified key from the cache.
    *   **Parameters:** `key` (string).
    *   **Return Value:** `void`.

*   **`ValueTask RemoveAsync(string key)`**
    *   **Purpose:** Asynchronously removes the item associated with the specified key.
    *   **Parameters:** `key` (string).
    *   **Return Value:** A `ValueTask` representing the completion of the operation.

*   **`void RemoveByPattern(string pattern)`**
    *   **Purpose:** Removes all cached items whose keys match the provided pattern.
    *   **Parameters:** `pattern` (string).
    *   **Return Value:** `void`.

*   **`bool Exists(string key)`**
    *   **Purpose:** Determines whether an item with the specified key exists in the cache.
    *   **Parameters:** `key` (string).
    *   **Return Value:** `true` if the key exists, `false` otherwise.

## Usage

### Synchronous Cache Access
```csharp
public void ConfigureService(ICacheService cache)
{
    string configKey = "app_config_settings";
    
    if (!cache.Exists(configKey))
    {
        var settings = LoadSettingsFromDatabase();
        cache.Set(configKey, settings, TimeSpan.FromHours(1));
    }
    
    var currentSettings = cache.Get<AppSettings>(configKey);
}
```

### Asynchronous Data Retrieval
```csharp
public async Task<VideoData> GetVideoAsync(ICacheService cache, string videoId)
{
    string cacheKey = $"video_data_{videoId}";
    var cachedVideo = await cache.GetAsync<VideoData>(cacheKey);
    
    if (cachedVideo != null)
    {
        return cachedVideo;
    }
    
    var video = await _videoService.FetchVideoAsync(videoId);
    await cache.SetAsync(cacheKey, video, TimeSpan.FromMinutes(30));
    
    return video;
}
```

## Notes

*   **Thread Safety:** While `ICacheService` is designed to be thread-safe, the actual thread-safety guarantees are determined by the specific implementation of `ICacheService` registered in the dependency injection container.
*   **Serialization:** Most implementations rely on serialization (e.g., JSON) to store complex types. Ensure that types stored in the cache are serializable and have parameterless constructors if required by the serializer.
*   **Performance:** `RemoveByPattern` can be an expensive operation, especially in distributed caching scenarios. Use it judiciously to avoid performance degradation.
*   **Exceptions:** Methods generally do not throw if a cache miss occurs; however, they may throw exceptions related to connectivity (for distributed caches) or serialization failures.
