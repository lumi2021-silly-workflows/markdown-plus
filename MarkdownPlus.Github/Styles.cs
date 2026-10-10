namespace MarkdownPlus.Github;

public enum Alignment { Left, Center, Right }

public record struct CardStyles(
    Alignment alignment = Alignment.Left,
    string? width = null
)
{
    public static CardStyles Parse(Dictionary<string, string>? styleStrings)
    {
        if (styleStrings is null) return default;

        var alignment = styleStrings.GetValueOrDefault("alignment") switch
        {
            "center" => Alignment.Center,
            "right" => Alignment.Right,
            _ => Alignment.Left,
        };
        var width = styleStrings.GetValueOrDefault("width");
        
        return new CardStyles(
            alignment,
            width
        );
    }
};
