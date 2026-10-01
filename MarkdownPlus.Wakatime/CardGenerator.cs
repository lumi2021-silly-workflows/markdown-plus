using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Wakatime;

public static class CardGenerator
{
    public static async Task<AstNode> GenerateDisplayBlock(JsonElement data, int maxLines)
    {
        var cache = Cache.GetServiceCache("wakatime");

        var languages = data.GetProperty("languages");
        var count = Math.Min(maxLines, languages.GetArrayLength());

        XNamespace ns = "http://www.w3.org/2000/svg";

        const double width = 600;
        const double rowHeight = 20;
        const double headerHeight = 50;
        const double barWidth = 280;
        const double barHeight = 12;

        var height = headerHeight + count * rowHeight + 10;

        var svg = new XElement(
            ns + "svg",
            new XAttribute("width", width),
            new XAttribute("height", height),
            new XAttribute("viewBox", $"0 0 {width} {height}"),

            new XElement(
                ns + "style",
                """
                text { color: #777; }
                """
            ),

            new XElement(
                ns + "text",
                new XAttribute("x", 20),
                new XAttribute("y", 30),
                new XAttribute("fill", "currentColor"),
                new XAttribute("font-family", "monospace"),
                new XAttribute("font-size", 16),
                $"Total time: {data.GetProperty("human_readable_total").GetString()}"
            )
        );

        for (var i = 0; i < count; i++)
        {
            var item = languages[i];

            var name = item.GetProperty("name").GetString()!;
            var percent = item.GetProperty("percent").GetDouble() / 100.0;
            var text = item.GetProperty("text").GetString()!;

            var y = headerHeight + i * rowHeight;

            var hue = i * 360.0 / count;
            var color = $"hsl({hue:0.##}, 70%, 55%)";

            svg.Add(
                new XElement(
                    ns + "text",
                    new XAttribute("x", 20),
                    new XAttribute("y", y + 5),
                    new XAttribute("fill", "currentColor"),
                    new XAttribute("font-family", "monospace"),
                    new XAttribute("font-size", 14),
                    name
                ),

                new XElement(
                    ns + "rect",
                    new XAttribute("x", 150),
                    new XAttribute("y", y - 8),
                    new XAttribute("width", barWidth),
                    new XAttribute("height", barHeight),
                    new XAttribute("rx", 3),
                    new XAttribute("fill", "#30363d")
                ),

                new XElement(
                    ns + "rect",
                    new XAttribute("x", 150),
                    new XAttribute("y", y - 8),
                    new XAttribute("width", barWidth * percent),
                    new XAttribute("height", barHeight),
                    new XAttribute("rx", 3),
                    new XAttribute("fill", color)
                ),

                new XElement(
                    ns + "text",
                    new XAttribute("x", 440),
                    new XAttribute("y", y + 5),
                    new XAttribute("fill", "currentColor"),
                    new XAttribute("font-family", "monospace"),
                    new XAttribute("font-size", 14),
                    text
                )
            );
        }

        var xml = svg.DumpString();
        var resourceKey = $"weekly-langs-{count}";

        cache.TouchResource(resourceKey, "svg", DateTimeOffset.UtcNow.AddHours(23));
        await cache.SetContentAsync(resourceKey, xml);
        var path = cache.GetPath(resourceKey);

        return HtmlElementNode.AlignCenter(
            [
                new HtmlElementNode
                {
                    TagName     = "img",
                    SelfClosing = true,
                    Attributes  = { ["src"] = path },
                },
            ]
        );
    }
    
    public static AstNode GenerateDisplayCode(JsonElement data, int maxLines, string levels)
    {
        var content = new StringBuilder();
        
        var languages = data.GetProperty("languages");
        var limit = Math.Min(maxLines, languages.GetArrayLength());
        
        content.AppendLine($"Total Time: {data.GetProperty("human_readable_total").GetString()}");
        content.AppendLine();
        
        for (var i = 0; i < limit; i++)
        {
            var item = languages[i];
            var name = item.GetProperty("name").GetString();
            var percent = item.GetProperty("percent").GetDouble() / 100.0;
            var text = item.GetProperty("text").GetString();

            var line = "- ";
            line += ('"' + Truncate(name, 13) + '"').PadRight(16);
            line += CreateProgressBar(percent, 30, levels) + " ";
            line += text;

            content.AppendLine(line);
        }

        var codeBlock = new CodeBlockNode
        {
            Language = "rust",
            Code     = content.ToString().TrimEnd(),
        };

        return codeBlock;
    }
    
    private static string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength) return str;
        return str[..(maxLength - 3)] + "...";
    }
    private static string CreateProgressBar(double percent, int width, string levels = "# ")
    {
        percent = Math.Max(0.0, Math.Min(1.0, percent));

        var maxLevel = levels.Length - 1;
        var outBuilder = new StringBuilder();

        for (var i = 0; i < width; i++)
        {
            var cellStart = (double)i / width;
            var cellEnd = (double)(i + 1) / width;
            var fill = (percent - cellStart) / (cellEnd - cellStart);
            var clamped = Math.Max(0.0, Math.Min(1.0, fill));
            var levelIndex = (int)Math.Round((1.0 - clamped) * maxLevel);
            
            outBuilder.Append(levels[levelIndex]);
        }

        return outBuilder.ToString();
    }
}
