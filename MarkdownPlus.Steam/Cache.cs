namespace MarkdownPlus.Steam;

public class CachedGameData
{
    public string AppId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ThinPath { get; set; } = string.Empty;
    public string WidePath { get; set; } = string.Empty;
}

public class CachedAchievementData
{
    public string AppId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string ApiName { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public long UnlockTime { get; set; }
}

public class SteamGameCacheMetadata
{
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, CachedGameData> Games { get; set; } = new();
}

public class SteamAchievementCacheMetadata
{
    public DateTime LastUpdated { get; set; }
    public List<CachedAchievementData> Achievements { get; set; } = new();
}