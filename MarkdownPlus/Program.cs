using MarkdownPlus.Core;
using MarkdownPlus.Markdown;
using MarkdownPlus.Markdown.Parser;

namespace MarkdownPlus;

static class Program
{
    static async Task Main(string[] args)
    {
        ModulesHandler.InitModules();
        Cache.Initialize(Environment.GetEnvironmentVariable("CACHE_DIR") ?? "./actions/cache");
        
        var content = await File.ReadAllTextAsync("README.template.md");
        var parser = new Parser(content);
        var document = parser.Parse();
        document = await TreeProcessor.Process(document);
        
        await File.WriteAllTextAsync("README.md", MarkdownRenderer.Render(document));
        Cache.PerformCleanup();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Finished");
        Console.ResetColor();
    }
    
}
