using MarkdownPlus.Core;

namespace MarkdownPlus.Steam;

public class SteamModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("steam-lib", SteamLibProcessor.SteamLibProcess),
    ];
    public override string[] EnvironmentVariables => [
        Constants.API_KEY_VAR,
        Constants.USER_ID_VAR,
    ];
}
