using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Wakatime;

public class WakatimeModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("wakatime-weekly-langs", Processor.WakatimeTagProcess),
    ];
    public override string[] EnvironmentVariables => [
        Constants.API_KEY_VAR,
    ];
}
