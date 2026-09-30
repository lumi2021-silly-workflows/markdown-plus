namespace MarkdownPlus.Core.Caching;

public interface ICacheManager
{
    public Task<string> GetContentAsync(string key, CancellationToken cancellationToken = default);
    public Task<string?> TryGetContentAsync(string key, CancellationToken cancellationToken = default);
    
    public string GetPath(string key, CancellationToken cancellationToken = default);
    public string? TryGetPath(string key, CancellationToken cancellationToken = default);
    
    public Task SetContentAsync(string key, string value, CancellationToken cancellationToken = default);
    public Task SetContentAsync(string key, byte[] value, CancellationToken cancellationToken = default);

    public void TouchResource(string key, string extension, DateTimeOffset? expiresAt);
    public void TouchResource(string key);
}
