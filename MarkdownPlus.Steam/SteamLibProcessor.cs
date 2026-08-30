using System.Text;
using System.Text.Json;
using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Steam;

public static class SteamLibProcessor
{
    private static readonly ModuleLogger logger = new("Steam Service");
    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);
    
    public static async Task<AstNode[]> SteamLibProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        if (node.Attributes.TryGetValue("option", out var option))
        {
            switch (option)
            {
                case "recent": return await SteamLibRecentProcess(node, envVars);
                case "perfected": return await SteamLibPerfectedProcess(node, envVars);
            }
        }
        
        logger.Warn($"Unknown option: {option}");
        return [node];
    }
    
    private static async Task<AstNode[]> SteamLibRecentProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var cacheDir = Path.Combine(envVars.GetValueOrDefault("CACHE_DIR", "./actions/cache"), "steam_recent");
        var metadataPath = Path.Combine(cacheDir, "metadata.json");
        Directory.CreateDirectory(cacheDir);

        Dictionary<string, CachedGameData> recentGameData;

        await CacheLock.WaitAsync();
        try
        {
            var cache = await LoadCacheAsync<SteamGameCacheMetadata>(metadataPath);
            var isExpired = cache == null || (DateTime.UtcNow - cache.LastUpdated) >= CacheExpiration;

            if (isExpired)
            {
                if (cache?.Games != null)
                {
                    logger.Info("Cleaning up old cached recent images from disk...");
                    PurgeOldFiles(cache.Games.Values);
                }

                var userId = envVars[Constants.USER_ID_VAR];
                var apiKey = envVars[Constants.API_KEY_VAR];
                logger.Info("Loading recent games...");

                logger.Info("Loading owned games data (it may take a while)...");
                var games = await SteamApi.GetOwnedGamesAsync(userId, apiKey);

                var recent = games
                    .Where(g => (g.LastPlayedTimestamp) > 0)
                    .OrderByDescending(g => g.LastPlayedTimestamp)
                    .Take(4)
                    .ToList();

                logger.Info($"Found {recent.Count} recent games.");

                recentGameData = new Dictionary<string, CachedGameData>();

                foreach (var game in recent)
                {
                    var (widePath, thinPath) = await GameCardGenerator.GetResponsiveCardAsync(game, cacheDir);

                    recentGameData[game.AppId] = new CachedGameData
                    {
                        AppId = game.AppId,
                        Name = game.Name,
                        ThinPath = thinPath,
                        WidePath = widePath
                    };
                }

                var newCache = new SteamGameCacheMetadata
                {
                    LastUpdated = DateTime.UtcNow,
                    Games = recentGameData
                };

                await SaveCacheAsync(metadataPath, newCache);
                logger.Success("Fresh recent games generated, old assets purged.");
            }
            else
            {
                logger.Info("Using cached recent steam game data...");
                recentGameData = cache.Games;
            }
        }
        finally
        {
            CacheLock.Release();
        }

        return BuildCardHtmlMarkup(recentGameData);
    }

    private static async Task<AstNode[]> SteamLibPerfectedProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var cacheDir = Path.Combine(envVars.GetValueOrDefault("CACHE_DIR", "./actions/cache"), "steam_perfect");
        var metadataPath = Path.Combine(cacheDir, "metadata.json");
        Directory.CreateDirectory(cacheDir);

        Dictionary<string, CachedGameData> perfectedGameData;

        await CacheLock.WaitAsync();
        try
        {
            var cache = await LoadCacheAsync<SteamGameCacheMetadata>(metadataPath);
            var isExpired = cache == null || (DateTime.UtcNow - cache.LastUpdated) >= CacheExpiration;

            if (isExpired)
            {
                if (cache?.Games != null)
                {
                    logger.Info("Cleaning up old cached perfected images from disk...");
                    PurgeOldFiles(cache.Games.Values);
                }

                var userId = envVars[Constants.USER_ID_VAR];
                var apiKey = envVars[Constants.API_KEY_VAR];
                logger.Info("Loading steam's owned games and achievements...");

                var owned = await SteamApi.GetOwnedGamesAsync(userId, apiKey);

                // Filtragem e ordenação utilizando as propriedades calculadas da SteamApi
                var perfectGames = owned
                    .Where(g => g.IsPerfected)
                    .OrderByDescending(g => g.LatestAchievementUnlockTime)
                    .Take(4)
                    .ToList();

                logger.Info($"Found {perfectGames.Count} perfected games.");

                perfectedGameData = new Dictionary<string, CachedGameData>();

                foreach (var game in perfectGames)
                {
                    var (widePath, thinPath) = await GameCardGenerator.GetResponsiveCardAsync(game, cacheDir);

                    perfectedGameData[game.AppId] = new CachedGameData
                    {
                        AppId = game.AppId,
                        Name = game.Name,
                        ThinPath = thinPath,
                        WidePath = widePath
                    };
                }

                var newCache = new SteamGameCacheMetadata
                {
                    LastUpdated = DateTime.UtcNow,
                    Games = perfectedGameData
                };

                await SaveCacheAsync(metadataPath, newCache);
                logger.Success("Fresh perfected games data generated, old assets purged.");
            }
            else
            {
                logger.Info("Using cached steam game data...");
                perfectedGameData = cache.Games;
            }
        }
        finally
        {
            CacheLock.Release();
        }

        return BuildCardHtmlMarkup(perfectedGameData);
    }
    
    private static async Task<T?> LoadCacheAsync<T>(string path) where T : class
    {
        if (!File.Exists(path)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task SaveCacheAsync(string path, object gameCache)
    {
        try
        {
            var json = JsonSerializer.Serialize(gameCache, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            logger.Warn($"Failed to save cache metadata: {ex.Message}");
        }
    }

    private static void PurgeOldFiles(IEnumerable<CachedGameData> games)
    {
        foreach (var game in games)
        {
            DeleteFileIfExists(game.ThinPath);
            DeleteFileIfExists(game.WidePath);
        }
    }

    private static void DeleteFileIfExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            logger.Warn($"Could not delete old file: {path} - {ex.Message}");
        }
    }

    private static AstNode[] BuildCardHtmlMarkup(Dictionary<string, CachedGameData> games)
    {
        const int githubArticleMaxPx = 1061;

        var cards = new HtmlElementNode
        {
            TagName           = "p",
            SelfClosing       = false,
            TrailingLineBreak = true,
        };

        foreach (var (appId, game) in games)
        {
            var a = new HtmlElementNode
            {
                TagName           = "a",
                SelfClosing       = false,
                TrailingLineBreak = true,
                Attributes =
                {
                    { "href", $"https://store.steampowered.com/app/{appId}" },
                    { "target", "_blank" },
                },
                Children =
                {
                    new HtmlElementNode
                    {
                        TagName           = "picture",
                        SelfClosing       = false,
                        TrailingLineBreak = true,
                        Children =
                        {
                            new HtmlElementNode
                            {
                                TagName           = "source",
                                SelfClosing       = true,
                                TrailingLineBreak = false,
                                Attributes =
                                {
                                    { "media", $"(max-width: {githubArticleMaxPx}px)" },
                                    { "width", "24%" },
                                    { "srcset", $"{game.ThinPath}" },
                                },
                            },
                            new HtmlElementNode
                            {
                                TagName           = "source",
                                SelfClosing       = true,
                                TrailingLineBreak = false,
                                Attributes =
                                {
                                    { "media", $"(min-width: {githubArticleMaxPx}px)" },
                                    { "width", "49%" },
                                    { "srcset", $"{game.WidePath}" },
                                },
                            },
                            new HtmlElementNode
                            {
                                TagName           = "img",
                                SelfClosing       = true,
                                TrailingLineBreak = false,
                                Attributes =
                                {
                                    { "style", "max-width: 100%;" },
                                    { "alt", $"{game.Name}" },
                                },
                            },
                        },
                    },
                },
            };
            cards.Children.Add(a);
        }

        var disclaimer = new HtmlElementNode
        {
            TagName           = "p",
            SelfClosing       = false,
            TrailingLineBreak = true,
            Attributes = { {"align", "center"} },
            Children =
            {
                new HtmlElementNode
                {
                    TagName           = "sub",
                    SelfClosing       = false,
                    TrailingLineBreak = false,
                    Children =
                    {
                        new HtmlElementNode
                        {
                            TagName           = "i",
                            SelfClosing       = false,
                            TrailingLineBreak = false,
                            Children =
                            {
                                new HtmlTextNode(
                                    "Disclaimer: All game titles, arts, logos, and trademarks belong to Steam "
                                    + "(Valve Corporation) and their respective developers."
                                ),
                            },
                        },
                    },
                },
            },
        };
        
        return [cards, disclaimer];
    }
    
}
