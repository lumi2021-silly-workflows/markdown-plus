using MarkdownPlus.Core;

namespace MarkdownPlus.Github;

public class GithubModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("github-profile", GithubTags.GithubProfileProcess),
        ("github-repo", GithubTags.GithubRepoProcess),
        ("github-activity", GithubTags.GithubActivityProcess),
    ];
    public override string[] EnvironmentVariables => [
        Constants.API_TOKEN_VAR,
        Constants.USERNAME_VAR,
    ];
}
