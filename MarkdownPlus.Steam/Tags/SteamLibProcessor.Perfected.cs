using MarkdownPlus.Core;
using MarkdownPlus.Core.Caching;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Steam.Tags;

public static partial class SteamLibProcessor
{
    public static async Task<AstNode> SteamLibPerfectedTag(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        const string DescriptorCacheKey = "perfected-cached-data";
        var cache = Cache.GetServiceCache("steam");
        
        List<CachedGameData> perfectedGameData; ;
        var cached = await cache.GetJsonAsync<SteamGameCacheMetadata>(DescriptorCacheKey);
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

            perfectedGameData = [];

            foreach (var game in perfectGames)
            {
                var newCachedData = new CachedGameData
                {
                    AppId            = game.AppId,
                    Name             = game.Name,
                    AchievementsUnlocked = game.UnlockedAchievements.Count,
                };
                perfectedGameData.Add(newCachedData);
                
                var wideCard = await GameCardGenerator.MakeWideCardAsync(game);
                var thinCard = await GameCardGenerator.MakeThinCardAsync(game);

                var keyThin = CardCacheKeys.Thin(newCachedData);
                var keyWide = CardCacheKeys.Wide(newCachedData);
                
                cache.TouchResource(keyThin, "svg", AssetsCacheExpiration);
                cache.TouchResource(keyWide, "svg", AssetsCacheExpiration);
                await cache.SetContentAsync(keyThin, thinCard);
                await cache.SetContentAsync(keyWide, wideCard);
            }

            cache.TouchResource(DescriptorCacheKey, "json", AssetsCacheExpiration);
            await cache.SetJsonAsync(DescriptorCacheKey, new SteamGameCacheMetadata
            {
                LastUpdated = DateTime.UtcNow,
                Games = [.. perfectedGameData],
            });

            logger.Success("Fresh perfected games data generated.");
        }
        else
        {
            cache.TouchResource(DescriptorCacheKey);
            logger.Info("Using cached steam game data...");
            perfectedGameData = [.. cached.Games];

            foreach (var game in perfectedGameData)
            {
                cache.TouchResource(CardCacheKeys.Wide(game));
                cache.TouchResource(CardCacheKeys.Thin(game));
            }
        }

        return BuildCardHtmlMarkup([.. perfectedGameData], cache);
    }
}
