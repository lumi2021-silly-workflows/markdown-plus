using MarkdownPlus.Core;
using MarkdownPlus.Core.Caching;
using MarkdownPlus.Core.Exceptions;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Steam.Tags;

public static partial class SteamLibProcessor
{
    private static DateTimeOffset AssetsCacheExpiration = DateTimeOffset.UtcNow.AddDays(7).AddHours(50);
    private static readonly ModuleLogger logger = new("Steam Service");
    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    
    private static AstNode BuildCardHtmlMarkup(CachedGameData[] games, ICacheManager cache)
    {
        const int githubArticleMaxPx = 1061;

        var cards = new HtmlElementNode
        {
            TagName           = "p",
            SelfClosing       = false,
            TrailingLineBreak = true,
        };

        foreach (var game in games)
        {
            var thinPath = game.ThinResourcePath;
            var widePath = game.WideResourcePath;

            var a = new HtmlElementNode
            {
                TagName           = "a",
                SelfClosing       = false,
                TrailingLineBreak = true,
                Attributes =
                {
                    ["href"] = $"https://store.steampowered.com/app/{game.AppId}",
                    ["target"] = "_blank",
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
                                    ["media"] = $"(max-width: {githubArticleMaxPx}px)",
                                    ["width"] = "24%",
                                    ["srcset"] = thinPath,
                                },
                            },
                            new HtmlElementNode
                            {
                                TagName           = "source",
                                SelfClosing       = true,
                                TrailingLineBreak = false,
                                Attributes =
                                {
                                    ["media"] = $"(min-width: {githubArticleMaxPx}px)",
                                    ["width"] = "49%",
                                    ["srcset"] = widePath,
                                },
                            },
                            new HtmlElementNode
                            {
                                TagName           = "img",
                                SelfClosing       = true,
                                TrailingLineBreak = false,
                                Attributes =
                                {
                                    ["style"] = "max-width: 100%;",
                                    ["alt"] = $"{game.Name}",
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
            Attributes = { ["align"] = "center" },
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

        return new AstNodesGroup([cards, disclaimer]);
    }

    private static (string userId, string apiKey) Auth(IReadOnlyDictionary<string, string> envVars)
    {
        var userId = envVars.GetValueOrDefault(Constants.USER_ID_VAR);
        var apiKey = envVars.GetValueOrDefault(Constants.API_KEY_VAR);

        if (userId != null && apiKey != null) return (userId, apiKey);

        List<LacksEnvVarException> exceptions = [];
        if (userId == null) exceptions.Add(new LacksEnvVarException(Constants.USER_ID_VAR, "<your steam user ID>"));
        if (apiKey == null) exceptions.Add(new LacksEnvVarException(Constants.API_KEY_VAR, "<your steam API key>"));
        throw new AuthException([..exceptions]);
    }
}