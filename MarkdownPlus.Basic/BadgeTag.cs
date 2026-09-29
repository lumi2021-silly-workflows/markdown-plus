using System.Net;
using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.ThirdParty;

public static class BadgeTag
{
    const string APP_URL = "https://img.shields.io/badge/";
    
    public static async Task<AstNode> ProcessBadgeTag(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        if (node.Children is not [HtmlTextNode @contentNode])
            return new HtmlCommentNode {  Content = "Expected element 'typing' to have a single text child node!" };

        var content = contentNode.Text;
        var icon = node.Attributes.GetValueOrDefault("icon");
        var style = node.Attributes.GetValueOrDefault("style");
        var color = node.Attributes.GetValueOrDefault("color", "ffffff")!;
        var iconColor = node.Attributes.GetValueOrDefault("icon-color", null);
        var labelColor = node.Attributes.GetValueOrDefault("label-color", null);
        var href = node.Attributes.GetValueOrDefault("href", null);
        
        var url = new UrlBuilder($"{APP_URL}{WebUtility.UrlEncode(content)}-{color}");
        if (icon != null) url.Query.Add("logo", icon); 
        if (style != null) url.Query.Add("style", style);
        if (iconColor != null) url.Query.Add("logoColor", iconColor);
        if (labelColor != null) url.Query.Add("labelColor", labelColor);

        AstNode result = new ImageNode
        {
            Alt = content,
            Src = url.ToString(),
        };
        if (href != null) result = new LinkNode
        {
            Href =  href,
            Children = { result },
        };

        return result;  //node.TrailingLineBreak
        //? new AstNodesGroup([result, new LineBreakNode()])
        //: result;
    }
}
