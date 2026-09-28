using MarkdownPlus.Core.Style;

namespace MarkdownPlus.Core;

public static class StyleParser
{
    public static Dictionary<string, string> Parse(string style)
    {
        var result = new Dictionary<string, string>();

        foreach (var declaration in SplitDeclarations(style))
        {
            var separator = declaration.IndexOf(':');

            if (separator < 0) continue;

            var property = declaration[..separator].Trim();
            var value = declaration[(separator + 1)..].Trim();

            if (property.Length == 0) continue;

            result[property] = value;
        }

        return result;
    }

    private static IEnumerable<string> SplitDeclarations(string style)
    {
        var start = 0;
        var quote = '\0';
        var parentheses = 0;

        for (var i = 0; i < style.Length; i++)
        {
            var c = style[i];

            if (quote != '\0')
            {
                if (c == quote && (i == 0 || style[i - 1] != '\\'))
                    quote = '\0';
                continue;
            }

            if (c is '"' or '\'')
            {
                quote = c;
                continue;
            }

            switch (c)
            {
                case '(':
                    parentheses++;
                break;

                case ')':
                    if (parentheses > 0)
                        parentheses--;
                break;

                case ';' when parentheses == 0:
                    yield return style[start..i];
                    start = i + 1;
                break;
            }
        }

        if (start < style.Length)
            yield return style[start..];
    }
}