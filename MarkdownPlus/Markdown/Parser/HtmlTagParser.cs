using System.Text;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Markdown.Parser;

/// <summary>
/// Parses a single HTML/XML-like element (optionally with nested children)
/// starting at '&lt;'. Shared by the block-level parser and the inline
/// parser so that e.g. &lt;badge&gt;...&lt;/badge&gt; is parsed the same way
/// whether it starts a line or sits inside a sentence. Tolerant of common
/// real-world markup issues (unterminated attribute quotes, mismatched
/// closing tags) so it never throws on messy input.
/// </summary>
public static class HtmlTagParser
{
    private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "img", "br", "hr", "input", "meta", "link", "source", "area",
        "base", "col", "embed", "param", "track", "wbr"
    };

    public static bool LooksLikeTagStart(Cursor c) => c.Current == '<' && char.IsLetter(c.Peek(1));

    public static HtmlCommentNode ParseComment(Cursor c)
    {
        var startLine = c.Line;
        var end = c.Text.IndexOf("-->", c.Pos, StringComparison.Ordinal);
        string content;
        if (end < 0)
        {
            content = c.Text.Substring(c.Pos + 4);
            while (!c.AtEnd) c.Advance();
        }
        else
        {
            content = c.Text.Substring(c.Pos + 4, end - (c.Pos + 4));
            var target = end + 3;
            while (c.Pos < target) c.Advance();
        }
        return new HtmlCommentNode { Line = startLine, Content = content.Trim() };
    }

    public static HtmlElementNode ParseElement(Cursor c)
    {
        var startLine = c.Line;
        if (c.Current == '<') c.Advance();
        var tagName = ReadName(c);
        var attributes = ReadAttributes(c);

        SkipSpaces(c);
        var selfClosing = false;
        if (c.Current == '/')
        {
            selfClosing = true;
            c.Advance();
        }
        if (c.Current == '>') c.Advance();

        var element = new HtmlElementNode { Line = startLine, TagName = tagName, SelfClosing = selfClosing };
        foreach (var kv in attributes) element.Attributes[kv.Key] = kv.Value;

        if (selfClosing || VoidTags.Contains(tagName))
            return element;

        var textBuffer = new StringBuilder();
        while (!c.AtEnd)
        {
            if (c.StartsWith("</"))
            {
                var save = c.Pos;
                c.Advance(); c.Advance();
                var closeName = ReadName(c);
                SkipSpaces(c);
                if (c.Current == '>') c.Advance();

                if (string.Equals(closeName, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    FlushText(textBuffer, element);
                    return element;
                }
                // Mismatched closing tag: treat it as plain text and keep scanning.
                textBuffer.Append(c.Text, save, c.Pos - save);
                continue;
            }

            if (c.StartsWith("<!--"))
            {
                FlushText(textBuffer, element);
                element.Children.Add(ParseComment(c));
                continue;
            }

            if (LooksLikeTagStart(c))
            {
                FlushText(textBuffer, element);
                element.Children.Add(ParseElement(c));
                continue;
            }

            textBuffer.Append(c.Current);
            c.Advance();
        }

        // Reached EOF without a matching closing tag - return what we have.
        FlushText(textBuffer, element);
        return element;
    }

    private static void FlushText(StringBuilder buffer, HtmlElementNode parent)
    {
        if (buffer.Length == 0) return;
        var raw = buffer.ToString();
        buffer.Clear();
        if (!string.IsNullOrWhiteSpace(raw))
            parent.Children.Add(new HtmlTextNode { Text = raw.Trim('\n', ' ', '\t') });
    }

    private static string ReadName(Cursor c)
    {
        var sb = new StringBuilder();
        while (!c.AtEnd && (char.IsLetterOrDigit(c.Current) || c.Current == '-' || c.Current == '_'))
        {
            sb.Append(c.Current);
            c.Advance();
        }
        return sb.ToString();
    }

    private static void SkipSpaces(Cursor c)
    {
        while (c is { AtEnd: false, Current: ' ' or '\t' or '\n' })
            c.Advance();
    }

    private static Dictionary<string, string> ReadAttributes(Cursor c)
    {
        var attrs = new Dictionary<string, string>();
        while (true)
        {
            SkipSpaces(c);
            if (c.AtEnd || c.Current == '/' || c.Current == '>') break;

            var name = new StringBuilder();
            while (!c.AtEnd && c.Current != '=' && c.Current != ' ' && c.Current != '\t' &&
                   c.Current != '\n' && c.Current != '/' && c.Current != '>')
            {
                name.Append(c.Current);
                c.Advance();
            }
            if (name.Length == 0) { c.Advance(); continue; }

            var value = string.Empty;
            SkipSpaces(c);
            if (c.Current == '=')
            {
                c.Advance();
                SkipSpaces(c);
                if (c.Current is '"' or '\'')
                {
                    var quote = c.Current;
                    c.Advance();
                    var val = new StringBuilder();
                    // Also bail out on '>' so an unterminated quote (real-world typo)
                    // doesn't swallow the rest of the document.
                    while (!c.AtEnd && c.Current != quote && c.Current != '>')
                    {
                        val.Append(c.Current);
                        c.Advance();
                    }
                    if (c.Current == quote) c.Advance();
                    value = val.ToString();
                }
                else
                {
                    var val = new StringBuilder();
                    while (!c.AtEnd && c.Current != ' ' && c.Current != '\t' && c.Current != '\n' &&
                           c.Current != '>' && c.Current != '/')
                    {
                        val.Append(c.Current);
                        c.Advance();
                    }
                    value = val.ToString();
                }
            }
            attrs[name.ToString()] = value;
        }
        return attrs;
    }
}
