using MarkdownPlus.Core.Exceptions;

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
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) throw new InvalidCacheException();
        return await value.ReadAllContentAsync();
    }
    public async Task<string?> TryGetContentAsync(string key, CancellationToken cancellationToken)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) return null;
        return await value.ReadAllContentAsync();
    }

    public string GetPath(string key, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) throw new InvalidCacheException();
        return value.RelativePath;
    }
    public string? TryGetPath(string key, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value)) return null;
        return value.RelativePath;
    }

    public async Task SetContentAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var entry)) throw new InvalidCacheException();
        await entry.UpdateContentAsync(value);
    }
    public async Task SetContentAsync(string key, byte[] value, CancellationToken cancellationToken = default)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var entry)) throw new InvalidCacheException();
        await entry.UpdateContentAsync(value);
    }

    public void TouchResource(string key, string extension, DateTimeOffset? expiresAt)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value))
            Cache.CreateEntry(service, key, extension, expiresAt);
        else value.dirty = true;
    }
    public void TouchResource(string key)
    {
        if (!ServiceCacheEntry.TryGetValue(key, out var value))
        {
            throw new InvalidCacheException(
                $"Asset resource {service}:{key} not found. "
                + $"Use TouchResource(string key, string extension, DateTimeOffset? expiresAt) "
                + $"for creating a new one."
            );
        }
        value.dirty = true;
    }
}
