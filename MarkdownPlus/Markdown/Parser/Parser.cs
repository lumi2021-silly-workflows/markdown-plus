using System.Text;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Markdown.Parser;

/// <summary>
/// Top-level, block-structure parser for the Markdown+HTML hybrid used in
/// GitHub profile READMEs. Splits the source into block-level constructs
/// (headings, paragraphs, blockquotes, lists, code fences, HTML blocks,
/// HTML comments, thematic breaks) and delegates inline content to
/// <see cref="InlineParser"/>.
/// </summary>
public sealed class Parser
{
    private readonly Cursor _c;

    public Parser(string? source)
    {
        var normalized = (source ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        _c = new Cursor(normalized);
    }

    public DocumentNode Parse()
    {
        var doc = new DocumentNode { Line = 1 };
        SkipBlankLines();
        while (!_c.AtEnd)
        {
            var before = _c.Pos;
            var block = ParseBlock();
            if (block != null) doc.Children.Add(block);
            if (_c.Pos == before) _c.Advance(); // safety net against accidental infinite loops
            SkipBlankLines();
        }
        return doc;
    }

    // ----------------------------------------------------------------
    // Low-level line helpers
    // ----------------------------------------------------------------

    private void SkipBlankLines()
    {
        while (!_c.AtEnd)
        {
            var p = _c.Pos;
            while (p < _c.Text.Length && (_c.Text[p] == ' ' || _c.Text[p] == '\t')) p++;
            if (p < _c.Text.Length && _c.Text[p] == '\n')
            {
                while (_c.Pos <= p) _c.Advance();
            }
            else break;
        }
    }

    private void SkipSpacesOnLine()
    {
        while (!_c.AtEnd && (_c.Current == ' ' || _c.Current == '\t')) _c.Advance();
    }

    private string PeekLine()
    {
        var end = _c.Text.IndexOf('\n', _c.Pos);
        if (end < 0) end = _c.Text.Length;
        return _c.Text.Substring(_c.Pos, end - _c.Pos);
    }

    private string ReadLine()
    {
        var end = _c.Text.IndexOf('\n', _c.Pos);
        string line;
        int target;
        if (end < 0)
        {
            line = _c.Text.Substring(_c.Pos);
            target = _c.Text.Length;
        }
        else
        {
            line = _c.Text.Substring(_c.Pos, end - _c.Pos);
            target = end + 1;
        }
        while (_c.Pos < target) _c.Advance();
        return line;
    }

    // ----------------------------------------------------------------
    // Block dispatch
    // ----------------------------------------------------------------

    private AstNode ParseBlock()
    {
        SkipSpacesOnLine();
        if (_c.AtEnd) return null;

        if (_c.StartsWith("<!--")) return HtmlTagParser.ParseComment(_c);
        if (HtmlTagParser.LooksLikeTagStart(_c)) return ParseHtmlBlock();
        if (_c.StartsWith("```") || _c.StartsWith("~~~")) return ParseCodeFence();
        if (_c.Current == '#') return ParseHeading();
        if (_c.Current == '>') return ParseBlockquote();
        if (IsThematicBreakLine()) return ParseThematicBreak();
        if (IsListMarker()) return ParseList();
        return ParseParagraph();
    }

    private AstNode ParseHtmlBlock()
    {
        var node = HtmlTagParser.ParseElement(_c);
        SkipSpacesOnLine();
        // A lone trailing "\" is Markdown's hard-line-break marker.
        if (_c is { AtEnd: false, Current: '\\' } && (_c.Peek(1) == '\n' || _c.Peek(1) == '\0'))
        {
            node.TrailingLineBreak = true;
            _c.Advance();
        }
        return node;
    }

    private AstNode ParseCodeFence()
    {
        var startLine = _c.Line;
        var fenceLine = ReadLine();
        var trimmedFence = fenceLine.TrimStart();
        var marker = trimmedFence.Substring(0, 3);
        var language = trimmedFence.Length > 3 ? trimmedFence.Substring(3).Trim() : string.Empty;

        var sb = new StringBuilder();
        while (!_c.AtEnd)
        {
            if (PeekLine().TrimStart().StartsWith(marker))
            {
                ReadLine();
                break;
            }
            sb.Append(ReadLine()).Append('\n');
        }
        return new CodeBlockNode { Line = startLine, Language = language, Code = sb.ToString().TrimEnd('\n') };
    }

    private AstNode ParseHeading()
    {
        var startLine = _c.Line;
        var level = 0;
        while (_c.Current == '#' && level < 6) { level++; _c.Advance(); }
        SkipSpacesOnLine();
        var content = ReadLine();
        var heading = new HeadingNode { Line = startLine, Level = level };
        heading.Inlines.AddRange(InlineParser.Parse(content));
        return heading;
    }

    private AstNode ParseBlockquote()
    {
        var startLine = _c.Line;
        var innerLines = new List<string>();
        while (_c is { AtEnd: false, Current: '>' })
        {
            _c.Advance(); // consume '>'
            if (_c.Current == ' ') _c.Advance(); // optional single space after '>'
            innerLines.Add(ReadLine());
        }
        var innerSource = string.Join("\n", innerLines);
        var innerDoc = new Parser(innerSource).Parse();
        var block = new BlockquoteNode { Line = startLine };
        block.Children.AddRange(innerDoc.Children);
        return block;
    }

    private bool IsThematicBreakLine()
    {
        var line = PeekLine().Trim();
        if (line.Length == 0) return false;
        var first = line[0];
        if (first != '-' && first != '*' && first != '_') return false;
        var count = 0;
        foreach (var ch in line)
        {
            if (ch == ' ') continue;
            if (ch != first) return false;
            count++;
        }
        return count >= 3;
    }

    private AstNode ParseThematicBreak()
    {
        var startLine = _c.Line;
        ReadLine();
        return new ThematicBreakNode { Line = startLine };
    }

    private bool IsListMarker()
    {
        var trimmed = PeekLine().TrimStart();
        return trimmed.StartsWith("- ") || trimmed.StartsWith("* ") || trimmed.StartsWith("+ ");
    }

    private AstNode ParseList()
    {
        var startLine = _c.Line;
        var list = new ListNode { Line = startLine, Ordered = false };
        while (!_c.AtEnd && IsListMarker())
        {
            SkipSpacesOnLine();
            _c.Advance(); // marker char (-, *, +)
            SkipSpacesOnLine();
            var itemLine = _c.Line;
            var content = ReadLine();
            var item = new ListItemNode { Line = itemLine };
            var para = new ParagraphNode { Line = itemLine };
            para.Inlines.AddRange(InlineParser.Parse(content));
            item.Children.Add(para);
            list.Items.Add(item);
        }
        return list;
    }

    private AstNode ParseParagraph()
    {
        var startLine = _c.Line;
        var lines = new List<string>();
        while (!_c.AtEnd)
        {
            var trimmed = PeekLine().TrimStart();
            if (trimmed.Length == 0) break; // blank line ends the paragraph
            if (lines.Count > 0 && StartsBlockConstruct(trimmed)) break;
            lines.Add(ReadLine());
        }
        var text = string.Join("\n", lines);
        var para = new ParagraphNode { Line = startLine };
        para.Inlines.AddRange(InlineParser.Parse(text));
        return para;
    }

    private static bool StartsBlockConstruct(string trimmedLine)
    {
        if (trimmedLine.StartsWith("#")) return true;
        if (trimmedLine.StartsWith(">")) return true;
        if (trimmedLine.StartsWith("```") || trimmedLine.StartsWith("~~~")) return true;
        if (trimmedLine.StartsWith("<!--")) return true;
        if (trimmedLine.Length > 1 && trimmedLine[0] == '<' && char.IsLetter(trimmedLine[1])) return true;
        if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* ") || trimmedLine.StartsWith("+ ")) return true;
        return false;
    }
}
