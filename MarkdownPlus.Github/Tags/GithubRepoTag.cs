using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

public partial class GithubTags
{
    public static async Task<AstNode> GithubRepoProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var (token, _) = Auth(envVars);
        var repository = node.Attributes.GetValueOrDefault("path");
        var repoSplit = repository?.Split('/', StringSplitOptions.TrimEntries);
        
        var cardStyles = CardStyles.Parse(StyleParser.Parse(node.Attributes.GetValueOrDefault("style"))); 
        
        if (repoSplit is not { Length: 2 })
            return new HtmlCommentNode("Error! attribute 'path' in format '{owner}/{repository}' expected");
        
        var ownerName = repoSplit[0];
        var repoName = repoSplit[1];
        
        logger.Info($"Requesting github's '{ownerName}/{repoName}' data...");
        var stats = await API.GetGithubRepositoryAsync(ownerName, repoName, token);
        logger.Info($"Processing github's '{ownerName}/{repoName}' data...");

        return await CardGenerator.BuildRepositoryCard(stats, cardStyles);
    }
}
