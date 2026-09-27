using MarkdownPlus.Core.Cache;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Steam.Tags;

public static partial class SteamLibProcessor
{
    public static async Task<AstNode[]> SteamLibPerfectedTag(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var cache = BuildCacheManager(envVars, "steam_perfect");

        Dictionary<string, CachedGameData> perfectedGameData;

        await CacheLock.WaitAsync();
        try
        {
            var cached = await cache.GetJsonAsync<SteamGameCacheMetadata>(MetadataKey);

            if (cached is null)
            {
                var (userId, apiKey) = Auth(envVars);

                logger.Info("Loading steam's owned games and achievements...");
                var owned = await SteamApi.GetOwnedGamesAsync(userId, apiKey);
                
                var perfectGames = owned
                    .Where(g => g.IsPerfected)
                    .OrderByDescending(g => g.LatestAchievementUnlockTime)
                    .Take(4)
                    .ToList();

                logger.Info($"Found {perfectGames.Count} perfected games.");

                perfectedGameData = new Dictionary<string, CachedGameData>();

                foreach (var game in perfectGames)
                {
                    await GameCardGenerator.GetResponsiveCardAsync(game, cache);
                    perfectedGameData[game.AppId] = new CachedGameData { AppId = game.AppId, Name = game.Name };
                }

                await cache.SetJsonAsync(MetadataKey, new SteamGameCacheMetadata
                {
                    LastUpdated = DateTime.UtcNow,
                    Games = perfectedGameData
                }, expiration: MetadataCacheExpiration);

                logger.Success("Fresh perfected games data generated.");
            }
            else
            {
                logger.Info("Using cached steam game data...");
                perfectedGameData = cached.Games;

                foreach (var appId in perfectedGameData.Keys)
                {
                    await cache.TouchAsync(CardCacheKeys.Wide(appId), GameCardGenerator.CardCacheExpiration);
                    await cache.TouchAsync(CardCacheKeys.Thin(appId), GameCardGenerator.CardCacheExpiration);
                }
            }

            var purged = await cache.PurgeExpiredAsync();
            if (purged.Count > 0)
                logger.Info($"Purged {purged.Count} expired cache entr{(purged.Count == 1 ? "y" : "ies")}.");
        }
        finally
        {
            CacheLock.Release();
        }

        return BuildCardHtmlMarkup(perfectedGameData, cache);
    }
}
