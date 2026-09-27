using MarkdownPlus.Core;
using MarkdownPlus.Steam.Tags;

namespace MarkdownPlus.Steam;

public class SteamModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("steam-lib-recent", SteamLibProcessor.SteamLibRecentTag),
        ("steam-lib-perfected", SteamLibProcessor.SteamLibPerfectedTag),
    ];
    public override string[] EnvironmentVariables => [
        Constants.API_KEY_VAR,
        Constants.USER_ID_VAR,
    ];
}
