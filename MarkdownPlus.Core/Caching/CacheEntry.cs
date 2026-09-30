namespace MarkdownPlus.Core.Caching;

public sealed record CacheEntry(
    string Service,
    string Key,
    string Extension,
    DateTimeOffset? ExpiresAt,
    string FullPath,
    string RelativePath
)
{
    public DateTimeOffset? ExpiresAt
    {
        get;
        set
        {
            field = value;
            dirty = true;
        }
    } = ExpiresAt;
    
    public bool dirty = false;

    public static string GetFileName(string service, string key, DateTimeOffset? expiresAt, string extension)
    {
        var expiresAtStamp = (ulong?)expiresAt?.ToUnixTimeSeconds() ?? ulong.MaxValue;
        return $"{service}_{key}_{expiresAtStamp:x16}.{extension}";
    }
    public override string ToString() => GetFileName(Service, Key, ExpiresAt, Extension);

    public Task<string> ReadAllContentAsync() => File.ReadAllTextAsync(FullPath);
    public Task UpdateContentAsync(string content) => File.WriteAllTextAsync(FullPath, content);
    public Task UpdateContentAsync(byte[] content) => File.WriteAllBytesAsync(FullPath, content);
}
