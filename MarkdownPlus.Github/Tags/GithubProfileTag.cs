using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

public partial class GithubTags
{
    public static async Task<AstNode> GithubProfileProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var (token, self_username) = Auth(envVars);
        var user = node.Attributes.GetValueOrDefault("user", self_username)!;
        var styleWidth = node.Attributes.GetValueOrDefault("width")!;
        
        logger.Info($"Requesting github's '{user}' data...");
        var stats = await API.GetGithubUserStatsAsync(user, token);
        logger.Info($"Processing github's '{user}' data...");

        var card = await CardGenerator.BuildProfileCard(stats, styleWidth);
        return card;
    }
}
