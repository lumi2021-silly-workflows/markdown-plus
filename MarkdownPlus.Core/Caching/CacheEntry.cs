namespace MarkdownPlus.Core.Caching;

public sealed record CacheEntry(
    string Service,
    string Key,
    string Etension,
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

    public string FullPath = FullPath;
    public string RelativePath = FullPath;
    
    public bool dirty { get; private set; }
    public bool markForDeletion = false;
    
    public override string ToString()
    {
        var expiresAtStamp = (ulong?)ExpiresAt?.ToUnixTimeSeconds() ?? ulong.MaxValue;
        return $"{Service}_{Key}_{expiresAtStamp:x16}.{Etension}";
    }

    public Task<string> ReadAllContentAsync() => File.ReadAllTextAsync(FullPath);
    public Task UpdateContentAsync(string content) => File.WriteAllTextAsync(FullPath, content);
    public Task UpdateContentAsync(byte[] content) => File.WriteAllBytesAsync(FullPath, content);
}
