namespace MarkdownPlus.Markdown.Ast;

/// <summary>Base class for every node produced by the parser.</summary>
public abstract class AstNode
{
    /// <summary>1-based line number where this node starts in the source text.</summary>
    public int Line { get; set; }
}

// ---------------------------------------------------------------------
// Document
// ---------------------------------------------------------------------

public sealed class DocumentNode : AstNode
{
    public List<AstNode> Children { get; } = [];
}

// ---------------------------------------------------------------------
// Block-level nodes
// ---------------------------------------------------------------------

public sealed class HeadingNode : AstNode
{
    public int Level { get; set; }
    public List<AstNode> Inlines { get; } = [];
}

public sealed class ParagraphNode : AstNode
{
    public List<AstNode> Inlines { get; set; }
    public ParagraphNode() => Inlines = [];
    public ParagraphNode(string text) => Inlines = [new TextNode(text)];
}

public sealed class BlockquoteNode : AstNode
{
    public List<AstNode> Children { get; } = [];
}

public sealed class ThematicBreakNode : AstNode
{
}

public sealed class CodeBlockNode : AstNode
{
    public string Language { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public sealed class ListNode : AstNode
{
    public bool Ordered { get; set; }
    public List<ListItemNode> Items { get; set; } = [];
}

public sealed class ListItemNode : AstNode
{
    public List<AstNode> Children { get; set; } = [];
}

// ---------------------------------------------------------------------
// HTML nodes (used both as block-level elements and inline elements)
// ---------------------------------------------------------------------

public sealed class HtmlElementNode : AstNode
{
    public string TagName { get; set; } = string.Empty;
    public Dictionary<string, string?> Attributes { get; } = new();
    public List<AstNode> Children { get; } = [];
    public bool SelfClosing { get; set; }

    /// <summary>
    /// True when, at block level, the tag is immediately followed on the same
    /// line by a lone "\" — the Markdown hard-line-break marker.
    /// </summary>
    public bool TrailingLineBreak { get; set; }

    public static HtmlElementNode CreateBr() => new()
    {
        TagName           = "br",
        TrailingLineBreak = false,
        SelfClosing       = true,
    };
    
    public static HtmlElementNode CreateA(string text, string href) => new()
    {
        TagName           = "a",
        TrailingLineBreak = false,
        SelfClosing       = false,
        Attributes = { { "href", href } },
        Children = { new HtmlTextNode(text) },
    };
    public static HtmlElementNode CreateP(string text) => new()
    {
        TagName           = "p",
        TrailingLineBreak = false,
        SelfClosing       = false,
        Children          = { new HtmlTextNode(text) },
    };
    public static HtmlElementNode CreateStrong(string text) => new()
    {
        TagName           = "strong",
        TrailingLineBreak = false,
        SelfClosing       = true,
        Children          = { new HtmlTextNode(text) },
    };
}

public sealed class HtmlCommentNode : AstNode
{
    public string Content { get; set; } = string.Empty;
}

public sealed class HtmlTextNode(string text) : AstNode
{
    public string Text { get; set; } = text;
    public HtmlTextNode() : this(string.Empty) { }
}

// ---------------------------------------------------------------------
// Inline nodes
// ---------------------------------------------------------------------

public sealed class TextNode(string content) : AstNode
{
    public string Text { get; set; } = content;
    public TextNode() : this(string.Empty) { }
}

public sealed class BoldNode : AstNode
{
    public List<AstNode> Children { get; } = [];
}

public sealed class ItalicNode : AstNode
{
    public List<AstNode> Children { get; } = [];
}

public sealed class InlineCodeNode : AstNode
{
    public string Code { get; set; } = string.Empty;
}

public sealed class LinkNode : AstNode
{
    public string Href { get; set; }
    public List<AstNode> Children { get; }

    public LinkNode()
    {
        Href     = string.Empty;
        Children = [];
    }
    public LinkNode(string text, string href)
    {
        Href     = href;
        Children = [new TextNode(text)];
    }
}

public sealed class ImageNode : AstNode
{
    public string Src { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}

/// <summary>A hard line break (trailing "\" or two trailing spaces).</summary>
public sealed class LineBreakNode : AstNode
{
}

/// <summary>A plain newline inside a paragraph that renders as whitespace.</summary>
public sealed class SoftBreakNode : AstNode
{
}
