using System.Text;

namespace MarkdownPlus.Markdown.Ast;

/// <summary>Pretty-prints an AST tree for inspection/debugging.</summary>
public static class AstPrinter
{
    public static string Print(AstNode node)
    {
        var sb = new StringBuilder();
        Write(node, sb, 0);
        return sb.ToString();
    }

    private static void Indent(StringBuilder sb, int depth) => sb.Append(' ', depth * 2);

    private static void Write(AstNode node, StringBuilder sb, int depth)
    {
        switch (node)
        {
            case DocumentNode doc:
                Indent(sb, depth); sb.AppendLine($"Document ({doc.Children.Count} children)");
                foreach (var child in doc.Children) Write(child, sb, depth + 1);
                break;

            case HeadingNode h:
                Indent(sb, depth); sb.AppendLine($"Heading level={h.Level}");
                foreach (var i in h.Inlines) Write(i, sb, depth + 1);
                break;

            case ParagraphNode p:
                Indent(sb, depth); sb.AppendLine("Paragraph");
                foreach (var i in p.Inlines) Write(i, sb, depth + 1);
                break;

            case BlockquoteNode bq:
                Indent(sb, depth); sb.AppendLine("Blockquote");
                foreach (var i in bq.Children) Write(i, sb, depth + 1);
                break;

            case ThematicBreakNode:
                Indent(sb, depth); sb.AppendLine("ThematicBreak");
                break;

            case CodeBlockNode cb:
                Indent(sb, depth); sb.AppendLine($"CodeBlock lang=\"{cb.Language}\" ({cb.Code.Length} chars)");
                break;

            case ListNode l:
                Indent(sb, depth); sb.AppendLine($"List ({l.Items.Count} items)");
                foreach (var it in l.Items) Write(it, sb, depth + 1);
                break;

            case ListItemNode li:
                Indent(sb, depth); sb.AppendLine("ListItem");
                foreach (var i in li.Children) Write(i, sb, depth + 1);
                break;

            case HtmlCommentNode hc:
                Indent(sb, depth); sb.AppendLine($"HtmlComment \"{Truncate(hc.Content)}\"");
                break;

            case HtmlElementNode he:
                Indent(sb, depth);
                sb.AppendLine($"HtmlElement <{he.TagName}> selfClosing={he.SelfClosing} " +
                               $"attrs={he.Attributes.Count} trailingBreak={he.TrailingLineBreak}");
                foreach (var kv in he.Attributes)
                {
                    Indent(sb, depth + 1); sb.AppendLine($"@{kv.Key}=\"{kv.Value}\"");
                }
                foreach (var i in he.Children) Write(i, sb, depth + 1);
                break;

            case HtmlTextNode ht:
                Indent(sb, depth); sb.AppendLine($"HtmlText \"{Truncate(ht.Text)}\"");
                break;

            case TextNode t:
                Indent(sb, depth); sb.AppendLine($"Text \"{Truncate(t.Text)}\"");
                break;

            case BoldNode b:
                Indent(sb, depth); sb.AppendLine("Bold");
                foreach (var i in b.Children) Write(i, sb, depth + 1);
                break;

            case ItalicNode it2:
                Indent(sb, depth); sb.AppendLine("Italic");
                foreach (var i in it2.Children) Write(i, sb, depth + 1);
                break;

            case InlineCodeNode ic:
                Indent(sb, depth); sb.AppendLine($"InlineCode \"{Truncate(ic.Code)}\"");
                break;

            case LinkNode ln:
                Indent(sb, depth); sb.AppendLine($"Link href=\"{ln.Href}\"");
                foreach (var i in ln.Children) Write(i, sb, depth + 1);
                break;

            case ImageNode im:
                Indent(sb, depth); sb.AppendLine($"Image alt=\"{im.Alt}\" src=\"{im.Src}\"");
                break;

            case LineBreakNode:
                Indent(sb, depth); sb.AppendLine("LineBreak");
                break;

            case SoftBreakNode:
                Indent(sb, depth); sb.AppendLine("SoftBreak");
                break;

            default:
                Indent(sb, depth); sb.AppendLine(node.GetType().Name);
                break;
        }
    }

    private static string Truncate(string s)
    {
        s = s.Replace("\n", "\\n");
        return s.Length > 60 ? s[..60] + "…" : s;
    }
}
