using MarkdownPlus.Core;

namespace MarkdownPlus.LastFm;

public class LastFmModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("last-fm", Processor.LastfmTagProcessor),
    ];
    public override string[] EnvironmentVariables => [
        Constants.API_KEY_VAR,
        Constants.USERNAME_VAR,
    ];
}
