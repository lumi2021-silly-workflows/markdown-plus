using System.Text.Json;

namespace MarkdownPlus.Core.Cache;

public static class CacheStoreExtensions
{
    public static async Task<T?> GetJsonAsync<T>(
        this ICacheManager cache,
        string key,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var entry = await cache.GetAsync(key, cancellationToken);
        return entry is null ? default : JsonSerializer.Deserialize<T>(entry.Data.Span, options);
    }

    public static Task SetJsonAsync<T>(
        this ICacheManager cacheManager,
        string key,
        T value,
        TimeSpan? expiration = null,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(value, options);
        return cacheManager.SetAsync(key, data, expiration, cancellationToken);
    }
}
