using System.Text.Json;

namespace MarkdownPlus.Core.Cache;

/// <summary>
/// Implementação de <see cref="ICacheManager"/> baseada em arquivos. Cada chave vira um
/// arquivo de dados em <c>{root}/{key}</c> (preservando a extensão da própria chave, ex.:
/// "cards/730_wide.svg" -> arquivo .svg de verdade em disco) mais um sidecar
/// "{key}.meta.json" com CreatedAt/ExpiresAt/LastAccessedAt.
/// </summary>
public sealed class FileCacheManager : ICacheManager
{
    private sealed record EntryMetadata(DateTime CreatedAt, DateTime? ExpiresAt, DateTime LastAccessedAt);

    private readonly string _root;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileCacheManager(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    public string GetDataPath(string key) => Path.Combine(_root, Sanitize(key));

    private string GetMetaPath(string key) => Path.Combine(_root, Sanitize(key) + ".meta.json");

    private static string Sanitize(string key)
    {
        // Preserva "/" como separador de subpasta (ex.: "cards/730_wide.svg").
        var parts = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return Path.Combine(parts);
    }

    public string? TryGetFilePath(string key) => GetDataPath(key);

    public async Task<CacheEntry?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var meta = await ReadMetaAsync(key, cancellationToken);
            if (meta is null) return null;

            if (meta.ExpiresAt.HasValue && DateTime.UtcNow >= meta.ExpiresAt.Value)
            {
                DeleteFiles(key);
                return null;
            }

            var dataPath = GetDataPath(key);
            if (!File.Exists(dataPath)) return null;

            var data = await File.ReadAllBytesAsync(dataPath, cancellationToken);

            var updated = meta with { LastAccessedAt = DateTime.UtcNow };
            await WriteMetaAsync(key, updated, cancellationToken);

            return new CacheEntry(key, data, updated.CreatedAt, updated.ExpiresAt, updated.LastAccessedAt);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync(
        string key,
        ReadOnlyMemory<byte> data,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var dataPath = GetDataPath(key);
            Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);
            await File.WriteAllBytesAsync(dataPath, data.ToArray(), cancellationToken);

            var now = DateTime.UtcNow;
            var meta = new EntryMetadata(now, expiration.HasValue ? now + expiration.Value : null, now);
            await WriteMetaAsync(key, meta, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        DeleteFiles(key);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> ListKeysAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_root)) return Task.FromResult<IReadOnlyCollection<string>>([]);

        const string suffix = ".meta.json";
        var keys = Directory.EnumerateFiles(_root, "*.meta.json", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(_root, p))
            .Select(p => p[..^suffix.Length].Replace(Path.DirectorySeparatorChar, '/'))
            .ToList();

        return Task.FromResult<IReadOnlyCollection<string>>(keys);
    }

    public async Task TouchAsync(
        string key,
        TimeSpan? extendExpirationBy = null,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var meta = await ReadMetaAsync(key, cancellationToken);
            if (meta is null) return;

            var now = DateTime.UtcNow;
            var expiresAt = extendExpirationBy.HasValue ? now + extendExpirationBy.Value : meta.ExpiresAt;
            await WriteMetaAsync(key, meta with { LastAccessedAt = now, ExpiresAt = expiresAt }, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyCollection<string>> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        var removed = new List<string>();
        foreach (var key in await ListKeysAsync(cancellationToken))
        {
            var meta = await ReadMetaAsync(key, cancellationToken);
            if (meta?.ExpiresAt.HasValue == true && DateTime.UtcNow >= meta.ExpiresAt.Value)
            {
                DeleteFiles(key);
                removed.Add(key);
            }
        }
        return removed;
    }

    public async Task<IReadOnlyCollection<string>> PurgeUnusedAsync(TimeSpan unusedFor, CancellationToken cancellationToken = default)
    {
        var removed = new List<string>();
        var threshold = DateTime.UtcNow - unusedFor;
        foreach (var key in await ListKeysAsync(cancellationToken))
        {
            var meta = await ReadMetaAsync(key, cancellationToken);
            if (meta != null && meta.LastAccessedAt <= threshold)
            {
                DeleteFiles(key);
                removed.Add(key);
            }
        }
        return removed;
    }

    private async Task<EntryMetadata?> ReadMetaAsync(string key, CancellationToken cancellationToken)
    {
        var metaPath = GetMetaPath(key);
        if (!File.Exists(metaPath)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(metaPath, cancellationToken);
            return JsonSerializer.Deserialize<EntryMetadata>(json);
        }
        catch
        {
            return null;
        }
    }

    private async Task WriteMetaAsync(string key, EntryMetadata meta, CancellationToken cancellationToken)
    {
        var metaPath = GetMetaPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(metaPath)!);
        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(meta), cancellationToken);
    }

    private void DeleteFiles(string key)
    {
        TryDelete(GetDataPath(key));
        TryDelete(GetMetaPath(key));
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // best-effort — não deve derrubar o processo de cache por um arquivo travado
        }
    }
}