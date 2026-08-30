using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Wakatime;

public class WakatimeModule : ModuleDescriptor
{
    public override (string tagName, ProcessNodeDelegate callback)[] Tags => [
        ("wakatime", Processor.WakatimeTagProcess),
    ];
    public override (string envVar, bool optional)[] EnvironmentVariables => [
        (Constants.API_KEY_VAR, false),
    ];
}
