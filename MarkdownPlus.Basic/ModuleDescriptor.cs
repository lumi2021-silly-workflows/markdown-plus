using MarkdownPlus.Core;

namespace MarkdownPlus.ThirdParty;

public class SteamModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("typing", TypingTag.ProcessTypingTag),
        ("badge", BadgeTag.ProcessBadgeTag),
    ];

    public override string[] EnvironmentVariables => [];
}
