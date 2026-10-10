using System.Globalization;
using System.Xml.Linq;
using MarkdownPlus.Core;
using MarkdownPlus.Core.Caching;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

internal static class CardGenerator
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly ICacheManager cache = Cache.GetServiceCache("github");
    private static readonly HttpClient HttpClient = new();

    private const int width = 480;
    private const int height = 250;
    
    public static async Task<AstNode> BuildProfileCard(GithubUserStats stats, string? styleWidth = null)
    {
        var doc = new XElement(Svg + "svg",
            new XAttribute("width", width.ToString()),
            new XAttribute("height", height.ToString()),
            new XAttribute("viewBox", $"0 0 {width} {height}"),
            new XAttribute("fill", "none"),
            
            new XElement(Svg + "style", """
            
                 text { color: #777; }
                .title { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; font-size: 18px; font-weight: 600; fill: #58a6ff; }
                .subtitle { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; font-size: 12px; fill: #8b949e; }
                .badge { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; font-size: 10px; font-weight: bold; fill: #3fb950; }
                .label { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; font-size: 12px; fill: currentColor; }
                .value { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; font-size: 13px; font-weight: 600; fill: currentColor; }
            
            """),
            
            // Avatar clip
            new XElement(Svg + "defs",
                new XElement(Svg + "clipPath", new XAttribute("id", "avatar-clip"),
                    new XElement(Svg + "circle",
                        new XAttribute("cx", "50"),
                        new XAttribute("cy", "40"),
                        new XAttribute("r", "26")))),
            
            // Avatar
            new XElement(Svg + "g", new XAttribute("transform", "translate(20, 10)"),
                new XElement(Svg + "image",
                    new XAttribute("href", await GetImageAsBase64Async(stats.AvatarUrl)),
                    new XAttribute("x", "24"),
                    new XAttribute("y", "14"),
                    new XAttribute("width", "52"),
                    new XAttribute("height", "52"),
                    new XAttribute("clip-path", "url(#avatar-clip)")),

                // Username
                new XElement(Svg + "text",
                    new XAttribute("x", "90"),
                    new XAttribute("y", "30"),
                    new XAttribute("class", "title"),
                    stats.Username),

                // Account type
                new XElement(Svg + "rect",
                    new XAttribute("x", "90"),
                    new XAttribute("y", "38"),
                    new XAttribute("width", stats.Type.ToString().Length * 7 + 12),
                    new XAttribute("height", "16"),
                    new XAttribute("rx", "8"),
                    new XAttribute("fill", "#238636"),
                    new XAttribute("fill-opacity", "0.2")),
                new XElement(Svg + "text",
                    new XAttribute("x", "96"),
                    new XAttribute("y", "50"),
                    new XAttribute("class", "badge"),
                    stats.Type.ToString().ToUpperInvariant()),

                // Entering date
                new XElement(Svg + "text",
                    new XAttribute("x", "440"),
                    new XAttribute("y", "38"),
                    new XAttribute("text-anchor", "end"),
                    new XAttribute("class", "subtitle"),
                    $"Member since {stats.JoinedAt:MMM yyyy}")
            ),

            // Line
            new XElement(Svg + "line",
                new XAttribute("x1", "20"), new XAttribute("y1", "100"),
                new XAttribute("x2", "460"), new XAttribute("y2", "100"),
                new XAttribute("stroke", "#21262d"),
                new XAttribute("stroke-width", "1")),

            // Metrics
            new XElement(Svg + "g", new XAttribute("transform", "translate(30, 120)"),
                CreateStatItem("Repositories", FormatNumber(stats.Repositories), 0, 0),
                CreateStatItem("Stars", FormatNumber(stats.Stars), 150, 0),
                CreateStatItem("Forks", FormatNumber(stats.Forks), 300, 0),
                
                CreateStatItem("Followers", FormatNumber(stats.Followers), 0, 45),
                CreateStatItem("Following", FormatNumber(stats.Following), 150, 45),
                CreateStatItem("Releases", FormatNumber(stats.Releases), 300, 45),
                
                CreateStatItem("Storage Used", FormatStorage(stats.StorageKb), 0, 90)
            )
        );

        var xml = doc.DumpString();
        var resourceKey = $"github-profile-{stats.Username}";

        cache.TouchResource(resourceKey, "svg", DateTimeOffset.UtcNow.AddHours(23));
        await cache.SetContentAsync(resourceKey, xml);
        var path = cache.GetPath(resourceKey);

        var image = new HtmlElementNode
        {
            TagName     = "img",
            SelfClosing = true,
            Attributes  = { ["src"] = path },
        };
        if (styleWidth != null) image.Attributes.Add("width", styleWidth);
        
        return HtmlElementNode.AlignCenter(image);
    }

    public static async Task<AstNode> BuildRepositoryCard(GithubRepositoryStats repository, string? styleWidth = null)
    {

        var description = string.IsNullOrWhiteSpace(repository.Description) ? "" : repository.Description;
        if (description.Length > 72) description = description[..69] + "...";

        var doc = new XElement(Svg + "svg",
            new XAttribute("width", width),
            new XAttribute("height", height),
            new XAttribute("viewBox", $"0 0 {width} {height}"),
            new XAttribute("fill", "none"),

            new XElement(Svg + "style",
                """
                text {
                    color: #777;
                }

                .title {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    font-size: 18px;
                    font-weight: 600;
                    fill: #58a6ff;
                }

                .subtitle {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    font-size: 12px;
                    fill: #8b949e;
                }

                .badge {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    font-size: 10px;
                    font-weight: bold;
                    fill: #3fb950;
                }

                .label {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    font-size: 12px;
                    fill: currentColor;
                }

                .value {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    font-size: 13px;
                    font-weight: 600;
                    fill: currentColor;
                }
                """),
            
            // Repository owner
            new XElement(Svg + "text",
                new XAttribute("x", 20),
                new XAttribute("y", 35),
                new XAttribute("class", "subtitle"),
                repository.Owner),

            // Repository name
            new XElement(Svg + "text",
                new XAttribute("x", 20),
                new XAttribute("y", 62),
                new XAttribute("class", "title"),
                repository.Name),

            // Description
            new XElement(Svg + "text",
                new XAttribute("x", 20),
                new XAttribute("y", 88),
                new XAttribute("class", "subtitle"),
                description),

            // Divider
            new XElement(Svg + "line",
                new XAttribute("x1", 20),
                new XAttribute("y1", 112),
                new XAttribute("x2", width - 20),
                new XAttribute("y2", 112),
                new XAttribute("stroke", "#21262d"),
                new XAttribute("stroke-width", 1)),

            // Repository metrics
            new XElement(Svg + "g",
                new XAttribute("transform", "translate(30, 135)"),

                // Language
                new XElement(Svg + "circle",
                    new XAttribute("cx", 5),
                    new XAttribute("cy", 0),
                    new XAttribute("r", 5),
                    new XAttribute("fill", GetLanguageColor(repository.Language))),

                new XElement(Svg + "text",
                    new XAttribute("x", 17),
                    new XAttribute("y", 4),
                    new XAttribute("class", "label"),
                    repository.Language ?? "Unknown"),

                // Stars
                CreateStatItem("Stars", FormatNumber(repository.Stars), 205, 0),

                // Forks
                CreateStatItem("Forks", FormatNumber(repository.Forks), 330, 0)
            )
        );

        var xml = doc.ToString(SaveOptions.DisableFormatting);

        var resourceKey = $"github-repository-{repository.Owner}-{repository.Name}";
        cache.TouchResource(resourceKey, "svg", DateTimeOffset.UtcNow.AddHours(23));
        await cache.SetContentAsync(resourceKey, xml);

        var image = new HtmlElementNode
        {
            TagName = "img",
            SelfClosing = true,
            Attributes =
            {
                ["src"] = cache.GetPath(resourceKey),
                ["alt"] = $"{repository.Owner}/{repository.Name} - {repository.Description}",
            },
        };
        if (styleWidth != null) image.Attributes.Add("width", styleWidth);

        return HtmlElementNode.A(
            repository.HtmlUrl,
            HtmlElementNode.AlignCenter(image)
        );
    }
    
    private static XElement CreateStatItem(string label, string value, int x, int y)
    {
        return new XElement(Svg + "g", new XAttribute("transform", $"translate({x}, {y})"),
            new XElement(Svg + "text",
                new XAttribute("x", "0"),
                new XAttribute("y", "0"),
                new XAttribute("class", "label"),
                label),
            new XElement(Svg + "text",
                new XAttribute("x", "0"),
                new XAttribute("y", "18"),
                new XAttribute("class", "value"),
                value)
        );
    }
    private static string FormatStorage(long kib)
    {
        string[] units = ["KiB", "MiB", "GiB", "TiB"];
        double value = kib;
        int unitIndex = 0;
        
        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024.0;
            unitIndex++;
        }

        return $"{value:0.#} {units[unitIndex]}";
    }
    public static async Task<string> GetImageAsBase64Async(string imageUrl)
    {
        try
        {
            var response = await HttpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/png";

            return $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
        }
        catch
        {
            // Empty image
            return "data:image/png;base64,"
                + "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
        }
    }
    private static string FormatNumber(long n) => n >= 1000 ? $"{n / 1000.0:0.#}k" : n.ToString("N0");
    private static string GetLanguageColor(string? language)
    {
        if (string.IsNullOrWhiteSpace(language)) return "#8b949e";
        var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(language.Trim());
        var hue = (uint)hash % 360;
        return $"hsl({hue}, 65%, 55%)";
    }
}
