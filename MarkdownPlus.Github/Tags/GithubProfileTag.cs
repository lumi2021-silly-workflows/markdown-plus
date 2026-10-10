using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

public partial class GithubTags
{
    public static async Task<AstNode> GithubProfileProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var (token, self_username) = Auth(envVars);
        var user = node.Attributes.GetValueOrDefault("user", self_username)!;

        var cardStyles = CardStyles.Parse(StyleParser.Parse(node.Attributes.GetValueOrDefault("style"))); 
        
        logger.Info($"Requesting github's '{user}' data...");
        var stats = await API.GetGithubUserStatsAsync(user, token);
        logger.Info($"Processing github's '{user}' data...");

        var card = await CardGenerator.BuildProfileCard(stats, cardStyles);
        return card;
    }
}
