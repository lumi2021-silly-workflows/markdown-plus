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

    private static async Task<AstNode[]> ProcessNode(AstNode node)
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
                return [node];

            case LinkNode l:
            {
                var newLink = new LinkNode { Href = l.Href };
                foreach (var i in l.Children)
                    newLink.Children.AddRange(await ProcessNode(i));
                return [newLink];
            }
            
            case ParagraphNode p:
            {
                var newParagraph = new ParagraphNode();
                foreach (var i in p.Inlines)
                    newParagraph.Inlines.AddRange(await ProcessNode(i));
                return [newParagraph];
            }

            case ListNode l:
            {
                var newList = new ListNode();
                foreach (var i in l.Items)
                    newList.Items.AddRange((await ProcessNode(i)).Select(e => (ListItemNode)e));
                return [newList];
            }
            case ListItemNode li:
            {
                var newListItem = new ListItemNode();
                foreach (var i in li.Children)
                    newListItem.Children.AddRange(await ProcessNode(i));
                return [newListItem];
            }
            
            case HtmlElementNode html:
                return await ProcessHtmlElement(html);
            
            default:
                return [new HtmlCommentNode { Content = $"Unknown node {node.GetType().Name}" }];
        }
    }

    private static async Task<AstNode[]> ProcessHtmlElement(HtmlElementNode htmlElement)
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
                return [new HtmlCommentNode(htmlElement.TagName)];
            }
        }

        switch (htmlElement.TagName)
        {
            case "typing":
            {
                if (htmlElement.Children is not [HtmlTextNode @content])
                    return [new HtmlCommentNode {  Content = "Expected element 'typing' to have a single text child node!" }];

                var sanitizedContent = content.Text
                    .Replace("\n\r", ";")
                    .Replace('\n', ';')
                    //.Replace(' ', '+')
                    //.Replace(",", "%2C");
                    ;
                
                var font = htmlElement.Attributes.GetValueOrDefault("font-family");
                var fontWeight = htmlElement.Attributes.GetValueOrDefault("font-weight");
                var fontSize = htmlElement.Attributes.GetValueOrDefault("font-size");
                var letterSpacing = htmlElement.Attributes.GetValueOrDefault("letter-spacing");
                var charDuration = htmlElement.Attributes.GetValueOrDefault("char-duration");
                var lineDuration = htmlElement.Attributes.GetValueOrDefault("line-duration");
                var width = htmlElement.Attributes.GetValueOrDefault("width", "400")!;
                var height = htmlElement.Attributes.GetValueOrDefault("height", "100")!;
                var repeat = htmlElement.Attributes.GetValueOrDefault("repeat", "on");
                
                var url = new UrlBuilder("https://readme-typing-svg.herokuapp.com");
                
                url.Query.Add("width", width);
                url.Query.Add("height", height);
                url.Query.Add("center", "true");
                url.Query.Add("vCenter", "true");
                url.Query.Add("multiline", "true");
                
                if (font != null) url.Query.Add("font", font);
                if (fontWeight != null) url.Query.Add("weight", fontWeight);
                if (fontSize != null) url.Query.Add("size", fontSize);
                if (letterSpacing != null) url.Query.Add("letterSpacing", letterSpacing);
                if (charDuration != null) url.Query.Add("duration", charDuration);
                if (lineDuration != null) url.Query.Add("pause", lineDuration);
                
                url.Query.Add("repeat", repeat is "on" ? "true" : "false");
                
                url.Query.Add("lines", sanitizedContent);
                
                var darkUrl = url.Copy(); darkUrl.Query.Add("color", "cfcfcf");
                var lightUrl = url.Copy(); lightUrl.Query.Add("color", "000000");
                
                var pictureElement = new HtmlElementNode { TagName =  "picture" };
                pictureElement.Children.Add(new HtmlElementNode { SelfClosing = true, TagName = "source", Attributes =
                {
                    ["media"] = "(prefers-color-scheme: dark)",
                    ["srcset"] = darkUrl.ToString(),
                }});
                pictureElement.Children.Add(new HtmlElementNode { SelfClosing = true, TagName = "source", Attributes =
                {
                    ["media"]  = "(prefers-color-scheme: light)",
                    ["srcset"] = lightUrl.ToString(),
                }});
                pictureElement.Children.Add(new HtmlElementNode { SelfClosing = true, TagName = "img", Attributes =
                {
                    ["draggable"]  = "false",
                    ["width"] = "100%",
                }});
                return [pictureElement];
            }

            case "badge":
            {
                if (htmlElement.Children is not [HtmlTextNode @contentNode])
                    return [new HtmlCommentNode {  Content = "Expected element 'typing' to have a single text child node!" }];

                var content = contentNode.Text;
                var icon = htmlElement.Attributes.GetValueOrDefault("icon");
                var style = htmlElement.Attributes.GetValueOrDefault("style");
                var color = htmlElement.Attributes.GetValueOrDefault("color", "ffffff")!;
                var iconColor = htmlElement.Attributes.GetValueOrDefault("icon-color", null);
                var labelColor = htmlElement.Attributes.GetValueOrDefault("label-color", null);
                
                var url = new UrlBuilder($"https://img.shields.io/badge/{WebUtility.UrlEncode(content)}-{color}");
                if (icon != null) url.Query.Add("logo", icon); 
                if (style != null) url.Query.Add("style", style);
                if (iconColor != null) url.Query.Add("logoColor", iconColor);
                if (labelColor != null) url.Query.Add("labelColor", labelColor);

                var result = new ImageNode
                {
                    Alt = content,
                    Src = url.ToString(),
                };
                
                return htmlElement.TrailingLineBreak
                    ? [result, new LineBreakNode()]
                    : [result];
            }
        }
        
        return [htmlElement];
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
