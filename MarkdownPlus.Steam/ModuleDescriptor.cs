using MarkdownPlus.Core;

namespace MarkdownPlus.Steam;

public class SteamModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("steam-lib", SteamLibProcessor.SteamLibProcess),
    ];
    public override (string envVar, bool optional)[] EnvironmentVariables => [
        (Constants.API_KEY_VAR, false),
        (Constants.USER_ID_VAR, false),
    ];
}
