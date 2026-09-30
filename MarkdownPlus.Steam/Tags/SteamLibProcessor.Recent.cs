using MarkdownPlus.Core;
using MarkdownPlus.Core.Caching;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Steam.Tags;

public static partial class SteamLibProcessor
{
    public static async Task<AstNode> SteamLibRecentTag(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        const string DescriptorCacheKey = "recent-cached-data";
        var cache = Cache.GetServiceCache("steam");

        List<CachedGameData> recentGameData;

        await CacheLock.WaitAsync();
        var cached = await cache.GetJsonAsync<SteamGameCacheMetadata>(DescriptorCacheKey);
        if (cached is null)
        {
            var (userId, apiKey) = Auth(envVars);

            logger.Info("Loading recent games...");
            logger.Info("Loading owned games data (it may take a while)...");
            var games = await SteamApi.GetOwnedGamesAsync(userId, apiKey);

            var recent = games
                .Where(g => g.LastPlayedTimestamp > 0)
                .OrderByDescending(g => g.LastPlayedTimestamp)
                .Take(4)
                .ToList();

            logger.Info($"Found {recent.Count} recent games.");

            recentGameData = [];

            foreach (var game in recent)
            {
                var newCachedData = new CachedGameData
                {
                    AppId = game.AppId,
                    Name  = game.Name,
                };
                
                var wideCard = await GameCardGenerator.MakeWideCardAsync(game);
                var thinCard = await GameCardGenerator.MakeThinCardAsync(game);

                var keyThin = CardCacheKeys.Thin(newCachedData);
                var keyWide = CardCacheKeys.Wide(newCachedData);
                
                cache.TouchResource(keyThin, "svg", AssetsCacheExpiration);
                cache.TouchResource(keyWide, "svg", AssetsCacheExpiration);
                await cache.SetContentAsync(keyThin, thinCard);
                await cache.SetContentAsync(keyWide, wideCard);

                newCachedData.ThinResourcePath = cache.GetPath(keyThin);
                newCachedData.WideResourcePath = cache.GetPath(keyWide);
                
                recentGameData.Add(newCachedData);
            }

            cache.TouchResource(DescriptorCacheKey, "json", AssetsCacheExpiration);
            await cache.SetJsonAsync(DescriptorCacheKey, new SteamGameCacheMetadata
            {
                LastUpdated = DateTime.UtcNow,
                Games = [.. recentGameData],
            });

            logger.Success("Fresh recent games generated.");
        }
        else
        {
            cache.TouchResource(DescriptorCacheKey);
            logger.Info("Using cached recent steam game data...");
            recentGameData = [.. cached.Games];
            
            foreach (var game in recentGameData)
            {
                cache.TouchResource(CardCacheKeys.Wide(game));
                cache.TouchResource(CardCacheKeys.Thin(game));
            }
        }

        return BuildCardHtmlMarkup([.. recentGameData], cache);
    }
}
