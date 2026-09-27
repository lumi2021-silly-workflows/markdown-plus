namespace MarkdownPlus.Core.Cache;

public sealed record CacheEntry(
    string Key,
    ReadOnlyMemory<byte> Data,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime LastAccessedAt);
