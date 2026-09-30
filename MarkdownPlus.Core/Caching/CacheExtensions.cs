using System.Text.Json;

namespace MarkdownPlus.Core.Caching;

public static class CacheStoreExtensions
{
    public static async Task<T?> GetJsonAsync<T>(
        this ICacheManager cache,
        string key,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var entryContent = await cache.TryGetContentAsync(key, cancellationToken);
        return entryContent is null ? default : JsonSerializer.Deserialize<T>(entryContent, options);
    }
    
    public static Task SetJsonAsync<T>(
        this ICacheManager cacheManager,
        string key,
        T value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        #if DEBUG
        options ??= new JsonSerializerOptions
        {
            WriteIndented =  true,
        };
        #endif
        
        var data = JsonSerializer.SerializeToUtf8Bytes(value, options);
        return cacheManager.SetContentAsync(key, data, cancellationToken);
    }
}
