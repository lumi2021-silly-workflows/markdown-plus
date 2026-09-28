using System.Net;
using MarkdownPlus.Core;
using MarkdownPlus.Core.Exceptions;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus;

public static class TreeProcessor
{
    private static List<LacksEnvVarException> _lacksEnvVarExceptions = null!;
    
    public static async Task<DocumentNode> Process(DocumentNode document)
    {
        var newDoc = new DocumentNode();
        _lacksEnvVarExceptions = [];
        
        foreach (var i in document.Children)
            newDoc.Children.AddRange(await ProcessNode(i));

        if (_lacksEnvVarExceptions.Count > 0) DumpDebug();
        _lacksEnvVarExceptions = null!;
        
        return newDoc;
    }

    private static async Task<AstNode> ProcessNode(AstNode node)
    {
        switch (node)
        {
            case HtmlCommentNode:
            case BlockquoteNode:
            case InlineCodeNode:
            case CodeBlockNode:
            case HeadingNode:
            case TextNode:
            case ImageNode:
            case LineBreakNode:
            case SoftBreakNode:
                return node;

            case LinkNode l:
            {
                var newLink = new LinkNode { Href = l.Href };
                foreach (var i in l.Children)
                    newLink.Children.AddRange(await ProcessNode(i));
                return newLink;
            }
            
            case ParagraphNode p:
            {
                var newParagraph = new ParagraphNode();
                foreach (var i in p.Inlines)
                    newParagraph.Inlines.AddRange(await ProcessNode(i));
                return newParagraph;
            }

            case ListNode l:
            {
                var newList = new ListNode();
                foreach (var i in l.Items)
                    newList.Items.Add((ListItemNode)await ProcessNode(i));
                return newList;
            }
            case ListItemNode li:
            {
                var newListItem = new ListItemNode();
                foreach (var i in li.Children)
                    newListItem.Children.AddRange(await ProcessNode(i));
                return newListItem;
            }
            
            case HtmlElementNode html:
                return await ProcessHtmlElement(html);
            
            default:
                return new HtmlCommentNode { Content = $"Unknown node {node.GetType().Name}" };
        }
    }

    private static async Task<AstNode> ProcessHtmlElement(HtmlElementNode htmlElement)
    {
        if (ModulesHandler.Delegates.TryGetValue(htmlElement.TagName, out var moduleTag))
        {
            try
            {
                return await moduleTag.Invoke(htmlElement, ModulesHandler.LoadedEnvironmentVariables);
            }
            catch (AuthException e)
            {
                _lacksEnvVarExceptions.AddRange(e.InnerExceptions);
                return new HtmlCommentNode(htmlElement.TagName);
            }
        }
        return htmlElement;
    }

    private static void DumpDebug()
    {
        var foundEnvVars = ModulesHandler.LoadedEnvironmentVariables;
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✔ Found environment variables:");
        if (foundEnvVars.Count > 0)
        {
            foreach (var (varName, _) in foundEnvVars)
                Console.WriteLine($"  - {varName}");
        }
        else
        {
            Console.WriteLine("  (none)");
        }
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n✖ Missing required environment variables:");
        if (foundEnvVars.Count > 0)
        {
            foreach (var envVar in _lacksEnvVarExceptions)
                Console.WriteLine($"  - {envVar.EnvVar} ('{envVar.Format}')");
        }
        else
        {
            Console.WriteLine("  (none)");
        }

        Console.ResetColor();
    }
}
