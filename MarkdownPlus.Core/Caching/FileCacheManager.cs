using System.Text.Json;

namespace MarkdownPlus.Core.Caching;

public sealed class FileCacheManager(string service) : ICacheManager
{
    private Dictionary<string, CacheEntry> ServiceCacheEntry
    {
        get
        {
            if (!Cache.CacheEntries.ContainsKey(service)) Cache.CacheEntries.Add(service, []);
            return Cache.CacheEntries[service];
        }
    }

    public async Task<string> GetContentAsync(string key, CancellationToken cancellationToken)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) throw new FileNotFoundException();
        return await value.ReadAllContentAsync();
    }
    public async Task<string?> TryGetContentAsync(string key, CancellationToken cancellationToken)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) return null;
        return await value.ReadAllContentAsync();
    }

    public string GetPath(string key, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) throw new FileNotFoundException();
        return value.RelativePath;
    }
    public string? TryGetPath(string key, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) return null;
        return value.RelativePath;
    }

    public async Task SetContentAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var entry)) throw new FileNotFoundException();
        await entry.UpdateContentAsync(value);
    }
    public async Task SetContentAsync(string key, byte[] value, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var entry)) throw new FileNotFoundException();
        await entry.UpdateContentAsync(value);
    }

    public void TouchResource(string key, string extension, DateTimeOffset? expiresAt)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value))
            Cache.CreateEntry(service, key, extension, expiresAt);
    }
}
