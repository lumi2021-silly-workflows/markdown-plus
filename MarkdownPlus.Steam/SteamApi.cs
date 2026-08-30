using System.Collections.Concurrent;
using System.Text.Json;
using MarkdownPlus.Core;

namespace MarkdownPlus.Steam;

public class SteamGame
{
    public string AppId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long LastPlayedTimestamp { get; set; } = 0;
    public GameImages Images { get; set; } = new();
    public PlaytimeInfo Playtime { get; set; } = new();
    public List<AchievementInfo> AllAchievements { get; set; } = [];
    public List<AchievementInfo> UnlockedAchievements { get; set; } = [];
    
    public bool IsPerfected => AllAchievements.Count > 0 && UnlockedAchievements.Count == AllAchievements.Count;
    public long LatestAchievementUnlockTime => UnlockedAchievements.FirstOrDefault()?.UnlockTime ?? 0;
}

public class AchievementInfo
{
    public string AppId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string ApiName { get; set; } = string.Empty;
    public bool Achieved { get; set; }
    public long UnlockTime { get; set; }
    public string? Icon { get; set; }
    public string? IconGray { get; set; }
}

public class ProfileData
{
    public List<SteamGame> Games { get; set; } = [];
    public List<AchievementInfo> RecentAchievements { get; set; } = [];
}

public static class SteamApi
{
   private static readonly ConcurrentDictionary<string, ProfileData> ProfileCache = new();
    private static readonly HttpClient HttpClient = new();
    private static readonly ModuleLogger Logger = new("Steam Service");

    public static async Task<List<SteamGame>> GetOwnedGamesAsync(string steamId, string apiKey)
    {
        var data = await EnsureProfileDataAsync(steamId, apiKey);
        return data.Games;
    }

    public static async Task<List<AchievementInfo>> GetRecentAchievementsAsync(string steamId, string apiKey)
    {
        var data = await EnsureProfileDataAsync(steamId, apiKey);
        return data.RecentAchievements;
    }

    private static async Task<ProfileData> EnsureProfileDataAsync(string steamId, string apiKey)
    {
        if (ProfileCache.TryGetValue(steamId, out var cachedData))
        {
            return cachedData;
        }

        Logger.Info($"Loading data for user ID: {steamId}...");

        var gamesJson = await SteamGetAsync("IPlayerService/GetOwnedGames/v1/", new Dictionary<string, string>
        {
            { "steamid", steamId },
            { "include_appinfo", "true" },
            { "include_played_free_games", "true" }
        }, apiKey);

        var responseElement = gamesJson.RootElement.GetProperty("response");
        if (!responseElement.TryGetProperty("games", out var rawGames))
        {
            var emptyData = new ProfileData();
            ProfileCache[steamId] = emptyData;
            return emptyData;
        }

        var allUnlockedAchievements = new ConcurrentBag<AchievementInfo>();

        var gameTasks = rawGames.EnumerateArray().Select(async gameElem =>
        {
            var appId = gameElem.GetProperty("appid").GetInt64().ToString();
            var name = gameElem.GetProperty("name").GetString() ?? string.Empty;
            var playtimeForever = gameElem.GetProperty("playtime_forever").GetInt32();
            var lastPlayed = gameElem.TryGetProperty("rtime_last_played", out var rtime) ? rtime.GetInt64() : 0;

            JsonDocument? resStats = null;
            JsonDocument? resSchema = null;

            try
            {
                resStats = await SteamGetAsync("ISteamUserStats/GetPlayerAchievements/v1/", new Dictionary<string, string>
                {
                    { "steamid", steamId },
                    { "appid", appId }
                }, apiKey);
            }
            catch
            {
                // ignored
            }

            try
            {
                resSchema = await SteamGetAsync("ISteamUserStats/GetSchemaForGame/v2/", new Dictionary<string, string>
                {
                    { "appid", appId }
                }, apiKey);
            }
            catch
            {
                // ignored
            }

            var schemaMap = new Dictionary<string, (string? Icon, string? IconGray)>();
            if (resSchema?.RootElement.TryGetProperty("game", out var gameSchema) == true &&
                gameSchema.TryGetProperty("availableGameStats", out var stats) == true &&
                stats.TryGetProperty("achievements", out var schemaAchArray))
            {
                foreach (var ach in schemaAchArray.EnumerateArray())
                {
                    var apiName = ach.GetProperty("name").GetString() ?? string.Empty;
                    var icon = ach.TryGetProperty("icon", out var ic) ? ic.GetString() : null;
                    var iconGray = ach.TryGetProperty("icongray", out var icg) ? icg.GetString() : null;
                    schemaMap[apiName] = (icon, iconGray);
                }
            }

            var fullAchievements = new List<AchievementInfo>();
            if (resStats?.RootElement.TryGetProperty("playerstats", out var playerStats) == true &&
                playerStats.TryGetProperty("achievements", out var achArray))
            {
                foreach (var ach in achArray.EnumerateArray())
                {
                    var apiName = ach.GetProperty("apiname").GetString() ?? string.Empty;
                    var achieved = ach.GetProperty("achieved").GetInt32() == 1;
                    var unlockTime = ach.GetProperty("unlocktime").GetInt64();

                    schemaMap.TryGetValue(apiName, out var schemaIcons);

                    fullAchievements.Add(new AchievementInfo
                    {
                        AppId = appId,
                        GameName = name,
                        ApiName = apiName,
                        Achieved = achieved,
                        UnlockTime = unlockTime,
                        Icon = schemaIcons.Icon,
                        IconGray = schemaIcons.IconGray
                    });
                }
            }

            // Ordena as conquistas desbloqueadas diretamente na montagem (da mais recente para a mais antiga)
            var unlockedSorted = fullAchievements
                .Where(a => a.Achieved)
                .OrderByDescending(a => a.UnlockTime)
                .ToList();

            foreach (var ach in unlockedSorted)
            {
                allUnlockedAchievements.Add(ach);
            }

            var mediaBaseUrl = $"https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/{appId}";

            return new SteamGame
            {
                AppId = appId,
                Name = name,
                Playtime = FormatPlaytime(playtimeForever),
                LastPlayedTimestamp = lastPlayed,
                AllAchievements = fullAchievements,
                UnlockedAchievements = unlockedSorted,
                Images = new GameImages
                {
                    Hero = $"{mediaBaseUrl}/library_hero.jpg",
                    Cover = $"{mediaBaseUrl}/library_600x900.jpg",
                    Logo = $"{mediaBaseUrl}/logo.png",
                }
            };
        });

        var processedGames = (await Task.WhenAll(gameTasks)).ToList();

        // Ordena a lista global de conquistas recentes por UnlockTime
        var recentAchievementsSorted = allUnlockedAchievements
            .OrderByDescending(a => a.UnlockTime)
            .ToList();

        var processedData = new ProfileData
        {
            Games = processedGames,
            RecentAchievements = recentAchievementsSorted
        };

        ProfileCache[steamId] = processedData;
        return processedData;
    }

    private static async Task<JsonDocument> SteamGetAsync(string endpoint, Dictionary<string, string> queryParams, string apiKey)
    {
        var queryList = new List<string> { $"key={Uri.EscapeDataString(apiKey)}" };
        queryList.AddRange(queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var queryString = string.Join("&", queryList);

        var url = $"https://api.steampowered.com/{endpoint}?{queryString}";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var response = await HttpClient.GetAsync(url);
                var statusCode = (int)response.StatusCode;

                if (statusCode >= 500 && statusCode <= 504)
                {
                    await Task.Delay(1000 * (int)Math.Pow(2, attempt));
                    continue;
                }

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(json);
            }
            catch when (attempt < 4)
            {
                await Task.Delay(1000 * (int)Math.Pow(2, attempt));
            }
        }

        throw new InvalidOperationException($"Failed to fetch {endpoint} after 5 attempts");
    }

    private static PlaytimeInfo FormatPlaytime(int totalMinutes)
    {
        var hours = totalMinutes / 60;
        var mins = totalMinutes % 60;
        var readable = hours > 0 ? $"{hours}h {mins}min" : $"{mins}min";

        return new PlaytimeInfo { Time = readable };
    }
}