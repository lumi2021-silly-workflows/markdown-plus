using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

public partial class GithubTags
{
    public static async Task<AstNode> GithubProfileProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var (token, self_username) = Auth(envVars);
        var username = node.Attributes.GetValueOrDefault("username", self_username)!;
        
        logger.Info($"Requesting github's '{username}' data...");
        var stats = await API.GetGithubUserStatsAsync(username, token);
        logger.Info($"Processing github's '{username}' data...");

        var group = HtmlTextNode.Div([
            new HtmlCommentNode("github-profile Not implemented!"),
        ]);
        
        return group;
    }
}
