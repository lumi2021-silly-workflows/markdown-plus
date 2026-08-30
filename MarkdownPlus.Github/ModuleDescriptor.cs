using MarkdownPlus.Core;

namespace MarkdownPlus.Github;

public class GithubModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("github", Processor.GithubTagProcess),
    ];
    public override (string envVar, bool optional)[] EnvironmentVariables => [
        (Constants.API_TOKEN_VAR, false),
        (Constants.USERNAME_VAR, false),
    ];
}
