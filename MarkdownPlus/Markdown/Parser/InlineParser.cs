using System.Text;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Markdown.Parser;

/// <summary>
/// Parses the inline content of a single block (heading, paragraph, list
/// item, blockquote line, ...) into text runs, bold/italic emphasis, links,
/// images, inline code, inline HTML/comments and line breaks.
/// </summary>
public static class InlineParser
{
    public static List<AstNode> Parse(string text)
    {
        var c = new Cursor(text ?? string.Empty);
        return ParseUntil(c, null);
    }

    /// <summary>
    /// Parses inlines from <paramref name="c"/> until end of input, or, when
    /// <paramref name="stopAt"/> is given, until that character is reached.
    /// </summary>
    private static List<AstNode> ParseUntil(Cursor c, char? stopAt)
    {
        var nodes = new List<AstNode>();
        var buf = new StringBuilder();

        void Flush()
        {
            if (buf.Length == 0) return;
            nodes.Add(new TextNode { Text = buf.ToString() });
            buf.Clear();
        }

        while (!c.AtEnd)
        {
            if (stopAt.HasValue && c.Current == stopAt.Value) break;

            // Hard line break: backslash immediately followed by a newline.
            if (c.Current == '\\' && c.Peek(1) == '\n')
            {
                Flush();
                nodes.Add(new LineBreakNode());
                c.Advance(); c.Advance();
                continue;
            }

            // Escaped punctuation: backslash followed by an ASCII punctuation char.
            if (c.Current == '\\' && IsAsciiPunctuation(c.Peek(1)))
            {
                buf.Append(c.Peek(1));
                c.Advance(); c.Advance();
                continue;
            }

            if (c.Current == '\n')
            {
                Flush();
                nodes.Add(new SoftBreakNode());
                c.Advance();
                continue;
            }

            if (c.StartsWith("<!--"))
            {
                Flush();
                nodes.Add(HtmlTagParser.ParseComment(c));
                continue;
            }

            if (HtmlTagParser.LooksLikeTagStart(c))
            {
                Flush();
                nodes.Add(HtmlTagParser.ParseElement(c));
                continue;
            }

            if (c.Current == '!' && c.Peek(1) == '[')
            {
                var img = TryParseImage(c);
                if (img != null) { Flush(); nodes.Add(img); continue; }
            }

            if (c.Current == '[')
            {
                var link = TryParseLink(c);
                if (link != null) { Flush(); nodes.Add(link); continue; }
            }

            if (c.StartsWith("**") || c.StartsWith("__"))
            {
                var bold = TryParseEmphasis(c, 2);
                if (bold != null) { Flush(); nodes.Add(bold); continue; }
            }
            else if (c.Current == '*' || c.Current == '_')
            {
                var italic = TryParseEmphasis(c, 1);
                if (italic != null) { Flush(); nodes.Add(italic); continue; }
            }

            if (c.Current == '`')
            {
                var code = TryParseInlineCode(c);
                if (code != null) { Flush(); nodes.Add(code); continue; }
            }

            buf.Append(c.Current);
            c.Advance();
        }

        Flush();
        return nodes;
    }

    private static bool IsAsciiPunctuation(char ch) =>
        "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".IndexOf(ch) >= 0;

    private static AstNode TryParseImage(Cursor c)
    {
        int save = c.Pos;
        c.Advance(); c.Advance(); // "!["
        string alt = ReadBalanced(c, '[', ']');
        if (alt == null || c.Current != '(') { c.Pos = save; return null; }
        c.Advance();
        string src = ReadBalanced(c, '(', ')');
        if (src == null) { c.Pos   = save; return null; }
        return new ImageNode { Alt = alt, Src = src.Trim() };
    }

    private static AstNode TryParseLink(Cursor c)
    {
        int save = c.Pos;
        c.Advance(); // "["
        
        var inner = ReadBalanced(c, '[', ']');
        if (inner == null || c.Current != '(') { c.Pos = save; return null; }
    
        c.Advance();
        var href = ReadBalanced(c, '(', ')');
        if (href == null) { c.Pos = save; return null; }

        var link = new LinkNode { Href = href.Trim() };
        
        link.Children.AddRange(ParseUntil(new Cursor(inner), null));
        return link;
    }

    /// <summary>
    /// Reads text starting right after an already-consumed <paramref name="open"/>
    /// char, tracking nested pairs, and returns the content up to (but not
    /// including) the matching <paramref name="close"/> char. Leaves the cursor
    /// right after that closing char. Returns null if unterminated.
    /// </summary>
    private static string ReadBalanced(Cursor c, char open, char close)
    {
        var sb = new StringBuilder();
        int depth = 1;
        while (!c.AtEnd)
        {
            char ch = c.Current;
            if (ch == open)
            {
                depth++;
                sb.Append(ch);
                c.Advance();
                continue;
            }
            if (ch == close)
            {
                depth--;
                c.Advance();
                if (depth == 0) return sb.ToString();
                sb.Append(ch);
                continue;
            }
            sb.Append(ch);
            c.Advance();
        }
        return null;
    }

    private static AstNode TryParseEmphasis(Cursor c, int markerLen)
    {
        int save = c.Pos;
        string marker = new string(c.Current, markerLen);
        if (!c.StartsWith(marker)) return null;
        for (int i = 0; i < markerLen; i++) c.Advance();

        int contentStart = c.Pos;
        int closeIdx = c.Text.IndexOf(marker, c.Pos, StringComparison.Ordinal);
        if (closeIdx < 0) { c.Pos = save; return null; }

        string inner = c.Text.Substring(contentStart, closeIdx - contentStart);
        if (inner.Length == 0) { c.Pos = save; return null; }
        c.Pos = closeIdx + markerLen;

        var children = ParseUntil(new Cursor(inner), null);
        if (markerLen == 2)
        {
            var bold = new BoldNode();
            bold.Children.AddRange(children);
            return bold;
        }
        var italic = new ItalicNode();
        italic.Children.AddRange(children);
        return italic;
    }

    private static AstNode TryParseInlineCode(Cursor c)
    {
        int save = c.Pos;
        c.Advance(); // '`'
        int start = c.Pos;
        int closeIdx = c.Text.IndexOf('`', c.Pos);
        if (closeIdx < 0) { c.Pos = save; return null; }
        string code = c.Text.Substring(start, closeIdx - start);
        c.Pos = closeIdx + 1;
        return new InlineCodeNode { Code = code };
    }
}
