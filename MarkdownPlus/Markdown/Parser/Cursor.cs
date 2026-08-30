namespace MarkdownPlus.Markdown.Parser;

/// <summary>
/// A simple forward-only cursor over a source string, tracking the current
/// 1-based line number so AST nodes can carry (best-effort) source locations.
/// Shared by <see cref="Parser"/>, <see cref="InlineParser"/> and
/// <see cref="HtmlTagParser"/>.
/// </summary>
public sealed class Cursor
{
    public string Text { get; }
    public int Pos { get; set; }
    public int Line { get; private set; } = 1;

    public Cursor(string text)
    {
        Text = text ?? string.Empty;
        Pos = 0;
    }

    public bool AtEnd => Pos >= Text.Length;

    public char Current => AtEnd ? '\0' : Text[Pos];

    public char Peek(int offset = 1)
    {
        int p = Pos + offset;
        return (p >= 0 && p < Text.Length) ? Text[p] : '\0';
    }

    public void Advance()
    {
        if (AtEnd) return;
        if (Text[Pos] == '\n') Line++;
        Pos++;
    }

    public bool StartsWith(string s)
    {
        if (Pos + s.Length > Text.Length) return false;
        return string.CompareOrdinal(Text, Pos, s, 0, s.Length) == 0;
    }
}
