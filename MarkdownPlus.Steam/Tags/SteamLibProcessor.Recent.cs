using MarkdownPlus.Core.Cache;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Steam.Tags;

public static partial class SteamLibProcessor
{
    public static async Task<AstNode[]> SteamLibRecentTag(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var cache = SteamLibProcessor.BuildCacheManager(envVars, "steam_recent");

        Dictionary<string, CachedGameData> recentGameData;

        await SteamLibProcessor.CacheLock.WaitAsync();
        try
        {
            var cached = await cache.GetJsonAsync<SteamGameCacheMetadata>(SteamLibProcessor.MetadataKey);

            if (cached is null)
            {
                var (userId, apiKey) = SteamLibProcessor.Auth(envVars);

                logger.Info("Loading recent games...");
                logger.Info("Loading owned games data (it may take a while)...");
                var games = await SteamApi.GetOwnedGamesAsync(userId, apiKey);

                var recent = games
                    .Where(g => g.LastPlayedTimestamp > 0)
                    .OrderByDescending(g => g.LastPlayedTimestamp)
                    .Take(4)
                    .ToList();

                SteamLibProcessor.logger.Info($"Found {recent.Count} recent games.");

                recentGameData = new Dictionary<string, CachedGameData>();

                foreach (var game in recent)
                {
                    await GameCardGenerator.GetResponsiveCardAsync(game, cache);
                    recentGameData[game.AppId] = new CachedGameData { AppId = game.AppId, Name = game.Name };
                }

                await cache.SetJsonAsync(SteamLibProcessor.MetadataKey, new SteamGameCacheMetadata
                {
                    LastUpdated = DateTime.UtcNow,
                    Games = recentGameData
                }, expiration: SteamLibProcessor.MetadataCacheExpiration);

                SteamLibProcessor.logger.Success("Fresh recent games generated.");
            }
            else
            {
                SteamLibProcessor.logger.Info("Using cached recent steam game data...");
                recentGameData = cached.Games;
                foreach (var appId in recentGameData.Keys)
                {
                    await cache.TouchAsync(CardCacheKeys.Wide(appId), GameCardGenerator.CardCacheExpiration);
                    await cache.TouchAsync(CardCacheKeys.Thin(appId), GameCardGenerator.CardCacheExpiration);
                }
            }
            
            var purged = await cache.PurgeExpiredAsync();
            if (purged.Count > 0) SteamLibProcessor.logger.Info($"Purged {purged.Count} expired cache entr{(purged.Count == 1 ? "y" : "ies")}.");
        }
        finally
        {
            SteamLibProcessor.CacheLock.Release();
        }

        return SteamLibProcessor.BuildCardHtmlMarkup(recentGameData, cache);
    }
}
