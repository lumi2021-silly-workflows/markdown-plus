using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Core;

public abstract class ModuleDescriptor
{
    public delegate Task<AstNode[]> ProcessNodeDelegate(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars);
    
    public abstract (string tagName, ProcessNodeDelegate callback)[] Tags { get; }
    public abstract (string envVar, bool optional)[] EnvironmentVariables { get; }
}
