using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.ThirdParty;

public static class TypingTag
{
    const string APP_URL = "https://readme-typing-svg.herokuapp.com";
    
    public static async Task<AstNode> ProcessTypingTag(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        if (node.Children is not [HtmlTextNode @content])
            return new HtmlCommentNode {  Content = "Expected element 'typing' to have a single text child node!" };

        var sanitizedContent = content.Text
            .Replace("\n\r", ";")
            .Replace('\n', ';')
            //.Replace(' ', '+')
            //.Replace(",", "%2C");
            ;
        
        var font = node.Attributes.GetValueOrDefault("font-family");
        var fontWeight = node.Attributes.GetValueOrDefault("font-weight");
        var fontSize = node.Attributes.GetValueOrDefault("font-size");
        var letterSpacing = node.Attributes.GetValueOrDefault("letter-spacing");
        var charDuration = node.Attributes.GetValueOrDefault("char-duration");
        var lineDuration = node.Attributes.GetValueOrDefault("line-duration");
        var width = node.Attributes.GetValueOrDefault("width", "400")!;
        var height = node.Attributes.GetValueOrDefault("height", "100")!;
        var repeat = node.Attributes.GetValueOrDefault("repeat", "on");
        
        var url = new UrlBuilder(APP_URL);
        
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
        
        var pictureElement = new HtmlElementNode
        {
            TagName =  "picture",
            Children = [
                new HtmlElementNode { SelfClosing = true, TagName = "source", Attributes =
                {
                    ["media"]  = "(prefers-color-scheme: dark)",
                    ["srcset"] = darkUrl.ToString(),
                }},
                new HtmlElementNode { SelfClosing = true, TagName = "source", Attributes =
                {
                    ["media"]  = "(prefers-color-scheme: light)",
                    ["srcset"] = lightUrl.ToString(),
                }},
                new HtmlElementNode {
                    SelfClosing = true,
                    TagName = "img",
                    Attributes =
                {
                    ["draggable"] = "false",
                    ["width"]     = "100%",
                }},
            ],
        };
        
        return pictureElement;
    }
}
