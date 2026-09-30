using System.Text.Json.Serialization;

namespace MarkdownPlus.Steam;

public class CachedGameData
{
    public string AppId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AchievementsUnlocked { get; set; }
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
    public CachedGameData[] Games { get; set; }
}

public class SteamAchievementCacheMetadata
{
    public DateTime LastUpdated { get; set; }
    public List<CachedAchievementData> Achievements { get; set; } = new();
}
