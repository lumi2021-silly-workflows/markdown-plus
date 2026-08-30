using System.Text;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Markdown;

/// <summary>
/// Renders an AST back into formatted Markdown (+ inline/block HTML).
/// Not guaranteed to be byte-identical to the original source (whitespace,
/// attribute ordering and quote style are normalized), but it is valid,
/// re-parseable Markdown that preserves structure and content.
/// </summary>
public static class MarkdownRenderer
{
    /// <summary>
    /// Tag names that should be treated as inline even when the parser
    /// captured them as top-level (line-starting) blocks — e.g. skill
    /// badges meant to sit side by side rather than one per paragraph.
    /// </summary>
    private static readonly HashSet<string> InlineHtmlTags =
        new(StringComparer.OrdinalIgnoreCase) { "badge" };

    public static string Render(AstNode root)
    {
        var sb = new StringBuilder();
        if (root is DocumentNode doc)
        {
            WriteBlockSequence(doc.Children, sb, 0);
        }
        else
        {
            WriteBlock(root, sb, 0);
        }
        return sb.ToString().TrimEnd('\n') + "\n";
    }

    /// <summary>
    /// Avalia se um nó é do tipo inline e deve ser agrupado com os adjacentes.
    /// </summary>
    private static bool IsInlineNode(AstNode node)
    {
        return node is TextNode ||
               node is BoldNode ||
               node is ItalicNode ||
               node is InlineCodeNode ||
               node is ImageNode ||
               node is LinkNode ||
               node is LineBreakNode ||
               node is SoftBreakNode ||
               (node is HtmlElementNode he && InlineHtmlTags.Contains(he.TagName));
    }

    private static void WriteBlockSequence(List<AstNode> children, StringBuilder sb, int quoteDepth)
    {
        var i = 0;
        var firstGroup = true;
        
        while (i < children.Count)
        {
            if (!firstGroup) sb.Append('\n');
            firstGroup = false;

            if (IsInlineNode(children[i]))
            {
                // Agrupa todos os nós inline consecutivos num único bloco contínuo
                var inlineGroup = new List<AstNode>();
                while (i < children.Count && IsInlineNode(children[i]))
                {
                    inlineGroup.Add(children[i]);
                    i++;
                }
                WriteWrappedInlines(inlineGroup, sb, quoteDepth);
            }
            else
            {
                WriteBlock(children[i], sb, quoteDepth);
                i++;
            }
        }
    }

    // ------------------------------------------------------------
    // Block-level rendering
    // ------------------------------------------------------------

    private static void WriteBlock(AstNode node, StringBuilder sb, int quoteDepth)
    {
        switch (node)
        {
            case HeadingNode h:
                Prefix(sb, quoteDepth);
                sb.Append('#', h.Level).Append(' ');
                sb.Append(RenderInlines(h.Inlines));
                sb.Append('\n');
            break;

            case ParagraphNode p:
                WriteWrappedInlines(p.Inlines, sb, quoteDepth);
            break;

            case BlockquoteNode bq:
                WriteBlockSequence(bq.Children, sb, quoteDepth + 1);
            break;

            case ThematicBreakNode:
                Prefix(sb, quoteDepth);
                sb.Append("---\n");
            break;

            case CodeBlockNode cb:
                Prefix(sb, quoteDepth);
                sb.Append("```").Append(cb.Language).Append('\n');
                foreach (var line in cb.Code.Split('\n'))
                {
                    Prefix(sb, quoteDepth);
                    sb.Append(line).Append('\n');
                }
                Prefix(sb, quoteDepth);
                sb.Append("```\n");
            break;

            case ListNode l:
                foreach (var item in l.Items)
                {
                    Prefix(sb, quoteDepth);
                    sb.Append("- ");
                    WriteListItemContent(item, sb, quoteDepth);
                }
            break;

            case HtmlCommentNode hc:
                Prefix(sb, quoteDepth);
                sb.Append("<!-- ").Append(hc.Content).Append(" -->\n");
            break;

            case HtmlElementNode he:
                WriteHtmlBlockElement(he, sb, quoteDepth);
            break;

            default:
                Prefix(sb, quoteDepth);
                sb.Append(RenderInline(node));
                sb.Append('\n');
            break;
        }
    }

    private static void WriteListItemContent(ListItemNode item, StringBuilder sb, int quoteDepth)
    {
        var first = true;
        var i = 0;
        
        while (i < item.Children.Count)
        {
            if (IsInlineNode(item.Children[i]))
            {
                var inlineGroup = new List<AstNode>();
                while (i < item.Children.Count && IsInlineNode(item.Children[i]))
                {
                    inlineGroup.Add(item.Children[i]);
                    i++;
                }
                
                if (!first) Prefix(sb, quoteDepth, extraIndent: 2);
                sb.Append(RenderInlines(inlineGroup)).Append('\n');
                first = false;
            }
            else if (item.Children[i] is ParagraphNode para)
            {
                if (!first) Prefix(sb, quoteDepth, extraIndent: 2);
                sb.Append(RenderInlines(para.Inlines)).Append('\n');
                first = false;
                i++;
            }
            else
            {
                var inner = new StringBuilder();
                WriteBlock(item.Children[i], inner, 0);
                sb.Append(Indent(inner.ToString(), 2));
                first = false;
                i++;
            }
        }
    }

    private static void WriteHtmlBlockElement(HtmlElementNode he, StringBuilder sb, int quoteDepth)
    {
        Prefix(sb, quoteDepth);
        sb.Append('<').Append(he.TagName);
        foreach (var kv in he.Attributes)
            sb.Append(' ').Append(kv.Key).Append("=\"").Append(kv.Value).Append('"');

        if (he.SelfClosing)
        {
            sb.Append(" />");
            AppendTrailingBreak(he, sb);
            sb.Append('\n');
            return;
        }

        sb.Append('>');

        var onlyText = he.Children.Count == 1 && he.Children[0] is HtmlTextNode;
        var empty = he.Children.Count == 0;

        if (empty)
        {
            sb.Append("</").Append(he.TagName).Append('>');
            AppendTrailingBreak(he, sb);
            sb.Append('\n');
            return;
        }

        if (onlyText)
        {
            sb.Append(((HtmlTextNode)he.Children[0]).Text);
            sb.Append("</").Append(he.TagName).Append('>');
            AppendTrailingBreak(he, sb);
            sb.Append('\n');
            return;
        }

        sb.Append('\n');
        foreach (var child in he.Children)
        {
            switch (child)
            {
                case HtmlTextNode ht:
                    foreach (var line in ht.Text.Split('\n'))
                    {
                        Prefix(sb, quoteDepth);
                        sb.Append(line).Append('\n');
                    }
                break;
                
                case HtmlCommentNode hc:
                    Prefix(sb, quoteDepth);
                    sb.Append("<!-- ").Append(hc.Content).Append(" -->\n");
                break;
                
                case HtmlElementNode nested:
                    WriteHtmlBlockElement(nested, sb, quoteDepth); 
                break;
            }
        }
        Prefix(sb, quoteDepth);
        sb.Append("</").Append(he.TagName).Append(">\n");
    }

    private static void AppendTrailingBreak(HtmlElementNode he, StringBuilder sb)
    {
        if (he.TrailingLineBreak) sb.Append(" \\");
    }

    // ------------------------------------------------------------
    // Paragraph wrapping (preserves hard/soft breaks from the AST)
    // ------------------------------------------------------------

    private static void WriteWrappedInlines(List<AstNode> inlines, StringBuilder sb, int quoteDepth)
    {
        Prefix(sb, quoteDepth);
        for (var i = 0; i < inlines.Count; i++)
        {
            var node = inlines[i];
            switch (node)
            {
                case LineBreakNode:
                    sb.Append(" \\\n");
                    Prefix(sb, quoteDepth);
                break;
                
                case SoftBreakNode:
                    sb.Append('\n');
                    Prefix(sb, quoteDepth);
                break;
                
                case HtmlElementNode he when InlineHtmlTags.Contains(he.TagName):
                    sb.Append(RenderInlineHtml(he));
                    var nextIsSameGroup = (i + 1 < inlines.Count) && 
                        (inlines[i + 1] is HtmlElementNode nextHe && InlineHtmlTags.Contains(nextHe.TagName));
                    
                    if (nextIsSameGroup)
                    {
                        // Se duas tags HTML inline estiverem adjacentes no nível de bloco, 
                        // as separamos com uma quebra contínua exata (\n) e não com uma linha em branco.
                        if (he.TrailingLineBreak)
                        {
                            sb.Append(" \\\n");
                        }
                        else
                        {
                            sb.Append('\n');
                        }
                        Prefix(sb, quoteDepth);
                    }
                break;
                
                default: 
                    sb.Append(RenderInline(node)); 
                break;
            }
        }
        sb.Append('\n');
    }

    // ------------------------------------------------------------
    // Inline rendering
    // ------------------------------------------------------------

    private static string RenderInlines(List<AstNode> inlines)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < inlines.Count; i++)
        {
            var n = inlines[i];
            if (n is LineBreakNode) sb.Append(" \\\n");
            else if (n is SoftBreakNode) sb.Append('\n');
            else if (n is HtmlElementNode he && InlineHtmlTags.Contains(he.TagName))
            {
                sb.Append(RenderInlineHtml(he));
                var nextIsSameGroup = (i + 1 < inlines.Count) && 
                    (inlines[i + 1] is HtmlElementNode nextHe && InlineHtmlTags.Contains(nextHe.TagName));
                
                if (nextIsSameGroup)
                {
                    sb.Append(he.TrailingLineBreak ? " \\\n" : "\n");
                }
            }
            else sb.Append(RenderInline(n));
        }
        return sb.ToString();
    }

    private static string RenderInline(AstNode node)
    {
        switch (node)
        {
            case TextNode t:
                return t.Text;

            case BoldNode b:
                return "**" + RenderInlines(b.Children) + "**";

            case ItalicNode it:
                return "*" + RenderInlines(it.Children) + "*";

            case InlineCodeNode ic:
                return "`" + ic.Code + "`";

            case ImageNode im:
                return "![" + im.Alt + "](" + im.Src + ")";

            case LinkNode ln:
                return "[" + RenderInlines(ln.Children) + "](" + ln.Href + ")";

            case HtmlCommentNode hc:
                return "<!-- " + hc.Content + " -->";

            case HtmlElementNode he:
                return RenderInlineHtml(he);

            case LineBreakNode:
                return " \\\n";

            case SoftBreakNode:
                return "\n";

            default:
                return string.Empty;
        }
    }

    private static string RenderInlineHtml(HtmlElementNode he)
    {
        var sb = new StringBuilder();
        sb.Append('<').Append(he.TagName);
        foreach (var kv in he.Attributes)
            sb.Append(' ').Append(kv.Key).Append("=\"").Append(kv.Value).Append('"');

        if (he.SelfClosing)
        {
            sb.Append(" />");
            return sb.ToString();
        }

        sb.Append('>');
        foreach (var child in he.Children)
        {
            sb.Append(child switch
            {
                HtmlTextNode ht => ht.Text,
                HtmlElementNode nested => RenderInlineHtml(nested),
                HtmlCommentNode hc => "<!-- " + hc.Content + " -->",
                _ => string.Empty
            });
        }
        sb.Append("</").Append(he.TagName).Append('>');
        return sb.ToString();
    }

    // ------------------------------------------------------------
    // Small helpers
    // ------------------------------------------------------------

    private static void Prefix(StringBuilder sb, int quoteDepth, int extraIndent = 0)
    {
        for (var i = 0; i < quoteDepth; i++) sb.Append("> ");
        for (var i = 0; i < extraIndent; i++) sb.Append(' ');
    }

    private static string Indent(string text, int spaces)
    {
        var pad = new string(' ', spaces);
        var lines = text.TrimEnd('\n').Split('\n');
        var sb = new StringBuilder();
        foreach (var line in lines)
            sb.Append(pad).Append(line).Append('\n');
        return sb.ToString();
    }
}