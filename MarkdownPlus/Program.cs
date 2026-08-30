using MarkdownPlus.Markdown;
using MarkdownPlus.Markdown.Parser;

namespace MarkdownPlus;

static class Program
{
    static async Task Main(string[] args)
    {
        ModulesHandler.InitModules();
        
        var content = await File.ReadAllTextAsync("README.template.md");
        var parser = new Parser(content);
        var document = parser.Parse();
        document = await TreeProcessor.Process(document);
        await File.WriteAllTextAsync("README.md", MarkdownRenderer.Render(document));
    }
    
}
