using System.Globalization;
using MarkdownPlus.Core.Caching;

namespace MarkdownPlus.Core;

public static class Cache
{
    private static string _cacheRoot = null!;
    
    // service -> (name -> file)
    internal static Dictionary<string, Dictionary<string, CacheEntry>> CacheEntries = null!;
    private static List<string> _toCleanUp = [];

    public static void Initialize(string cacheRoot)
    {
        if (CacheEntries != null) throw new InvalidOperationException("CacheManager already initialized");
        CacheEntries     = [];
        _cacheRoot = cacheRoot;
        
        if (!Directory.Exists(_cacheRoot)) Directory.CreateDirectory(_cacheRoot);
        var files = Directory.GetFiles(_cacheRoot);
        
        foreach (var i in files)
        {
            var fullPath = Path.GetFullPath(i);
            var relativePath = Path.GetRelativePath(".", fullPath);
            
            var split = Path.GetFileNameWithoutExtension(i).Split('_');
            var service = split[0];
            var resourceId = split[1];
            var timestamp = split[2];
            var extension = Path.GetExtension(i)[1..];

            var expiresAtTimestamp = ulong.Parse(timestamp, NumberStyles.HexNumber);
            DateTimeOffset? expiresAt = expiresAtTimestamp > long.MaxValue ? DateTimeOffset.FromUnixTimeSeconds(unchecked((long)expiresAtTimestamp)) : null;

            if (DateTimeOffset.UtcNow > expiresAt)
            {
                _toCleanUp.Add(i);
                continue;
            }
            
            if (!CacheEntries.TryGetValue(service, out var serviceEntries))
            {
                CacheEntries.Add(service, []);
                serviceEntries = CacheEntries[service];
            }
            
            serviceEntries.Add(resourceId, new CacheEntry(
                service,
                resourceId,
                extension,
                expiresAt,
                fullPath,
                relativePath
            ));
        }
    }

    public static ICacheManager GetServiceCache(string serviceName) => new FileCacheManager(serviceName);
    internal static void CreateEntry(string service, string resourceId, string extension, DateTimeOffset? expiresAt)
    {
        if (CacheEntries == null) throw new InvalidOperationException("CacheManager is not initialized");

        if (!CacheEntries.TryGetValue(service, out var serviceEntries))
            CacheEntries.Add(service, []);

        if (serviceEntries.ContainsKey(resourceId))
            throw new InvalidOperationException($"Cache entry '{service}_{resourceId}' already exists");

        var timestamp = expiresAt.HasValue
            ? expiresAt.Value.ToUnixTimeSeconds().ToString()
            : long.MaxValue.ToString();

        var entry = new CacheEntry(
            service,
            resourceId,
            extension,
            expiresAt,
            string.Empty,
            string.Empty
        );
        
        var fileName = entry.ToString();
        var fullPath = Path.GetFullPath(Path.Combine(_cacheRoot, fileName));
        var relativePath = Path.GetRelativePath(".", fullPath);

        entry.FullPath = fullPath;
        entry.RelativePath = relativePath;
        
        serviceEntries.Add(resourceId, entry);
    }
    
    public static void PerformCleanup()
    {
        foreach (var i in _toCleanUp) File.Delete(i);
    }
}

