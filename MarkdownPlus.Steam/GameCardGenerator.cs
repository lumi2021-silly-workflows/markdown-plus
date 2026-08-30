using System.Buffers.Binary;
using System.Text;
using System.Xml.Linq;

namespace MarkdownPlus.Steam;

public static class GameCardGenerator
{
    private const string BadgeBadColor = "#cc0000";
    private const string BadgeGoodColor = "#4CAF50";
    private const string BadgeMehColor = "#555555";
    private const string Disclaimer = "Disclaimer: "
        + "All game titles, arts, logos, and trademarks belong to Steam "
        + "(Valve Corporation) and their respective developers.";

    private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";
    private static readonly HttpClient Client = new();

    public static async Task<(string WidePath, string ThinPath)> GetResponsiveCardAsync(SteamGame game, string cacheDirectory)
    {
        var cacheDir = Path.Combine(cacheDirectory, "steam_cards_generated");
        Directory.CreateDirectory(cacheDir);

        var wideSvgPath = Path.Combine(cacheDir, $"{game.AppId}_wide.svg");
        var thinSvgPath = Path.Combine(cacheDir, $"{game.AppId}_thin.svg");

        if (!File.Exists(wideSvgPath))
        {
            var wideContent = await MakeWideCardAsync(game);
            var thinContent = await MakeThinCardAsync(game);

            await File.WriteAllTextAsync(wideSvgPath, wideContent, Encoding.UTF8);
            await File.WriteAllTextAsync(thinSvgPath, thinContent, Encoding.UTF8);
        }

        return (wideSvgPath, thinSvgPath);
    }

    private static async Task<string> MakeWideCardAsync(SteamGame game)
    {
        var (imageHero, _, _)                   = await ImageToBase64Async(game.Images.Hero);
        var (imageLogo, imageLogoW, imageLogoH) = await ImageToBase64Async(game.Images.Logo);

        const int canvasWidth = 460;
        const int canvasHeight = 215;

        const double imageLogoFw = canvasWidth / 2.0;
        var imageLogoFh = imageLogoW > 0 ? (imageLogoFw / imageLogoW) * imageLogoH : 0;

        var playtimeText = !string.IsNullOrEmpty(game.Playtime?.Time) ? game.Playtime.Time : "0h 0min";
        var playtimePillWidth = 65 + playtimeText.Length * 8;

        string badgeColor;
        string badgeText;
        var unlockedPercent = GetAchievementPercent(game);

        if (!unlockedPercent.HasValue)
        {
            badgeColor = BadgeMehColor;
            badgeText  = "NA";
        }
        else
        {
            badgeText  = $"🏆 {Math.Round(unlockedPercent.Value * 100)}%";
            badgeColor = LerpColor(BadgeBadColor, BadgeGoodColor, unlockedPercent.Value);
        }

        var doc = new XDocument(
            new XComment(Disclaimer),
            new XElement(SvgNs + "svg",
                new XAttribute("xmlns", SvgNs.NamespaceName),
                new XAttribute("width", canvasWidth),
                new XAttribute("height", canvasHeight),
                new XAttribute("viewBox", $"0 0 {canvasWidth} {canvasHeight}"),

                new XElement(SvgNs + "defs",
                    new XElement(SvgNs + "linearGradient",
                        new XAttribute("id", "fade"),
                        new XAttribute("x1", "0"), new XAttribute("y1", "0"),
                        new XAttribute("x2", "0"), new XAttribute("y2", "1"),
                        new XElement(SvgNs + "stop", new XAttribute("offset", "0%"), new XAttribute("stop-color", "transparent")),
                        new XElement(SvgNs + "stop", new XAttribute("offset", "100%"), new XAttribute("stop-color", "#111"))
                    ),
                    new XElement(SvgNs + "filter",
                        new XAttribute("id", "shadow"),
                        new XElement(SvgNs + "feDropShadow",
                            new XAttribute("dx", "0"), new XAttribute("dy", "2"),
                            new XAttribute("stdDeviation", "3"), new XAttribute("flood-opacity", ".45")
                        )
                    )
                ),

                // Hero Image
                new XElement(SvgNs + "image",
                    new XAttribute("href", imageHero),
                    new XAttribute("width", "460"), new XAttribute("height", "215"),
                    new XAttribute("preserveAspectRatio", "xMidYMid slice")
                ),

                // Gradient
                new XElement(SvgNs + "rect",
                    new XAttribute("width", "460"), new XAttribute("height", "215"),
                    new XAttribute("fill", "url(#fade)")
                ),

                // Icon / Logo
                new XElement(SvgNs + "image",
                    new XAttribute("x", 20), new XAttribute("y", 20),
                    new XAttribute("href", imageLogo),
                    new XAttribute("width", imageLogoFw), new XAttribute("height", imageLogoFh),
                    new XAttribute("preserveAspectRatio", "xMidYMid meet")
                ),

                // Playtime
                new XElement(SvgNs + "rect",
                    new XAttribute("x", "60"), new XAttribute("y", "175"),
                    new XAttribute("width", playtimePillWidth), new XAttribute("height", "28"),
                    new XAttribute("rx", "14"), new XAttribute("fill", "#000000"),
                    new XAttribute("fill-opacity", "0.55")
                ),
                new XElement(SvgNs + "text",
                    new XAttribute("x", 100), new XAttribute("y", "194"),
                    new XAttribute("fill", "#ffffff"), new XAttribute("font-size", "16"),
                    new XAttribute("font-weight", "600"),
                    new XAttribute("font-family", "system-ui, -apple-system, sans-serif"),
                    playtimeText
                ),

                // Badge
                new XElement(SvgNs + "rect",
                    new XAttribute("x", "10"), new XAttribute("y", "175"),
                    new XAttribute("width", "75"), new XAttribute("height", "28"),
                    new XAttribute("rx", "14"), new XAttribute("fill", badgeColor)
                ),
                new XElement(SvgNs + "text",
                    new XAttribute("x", "48"), new XAttribute("y", "194"),
                    new XAttribute("text-anchor", "middle"), new XAttribute("fill", "white"),
                    new XAttribute("font-size", "15"), new XAttribute("font-weight", "bold"),
                    new XAttribute("font-family", "Arial"),
                    badgeText
                )
            )
        );

        return doc.ToString();
    }

    private static async Task<string> MakeThinCardAsync(SteamGame game)
    {
        var (imageCover, _, _) = await ImageToBase64Async(game.Images.Cover);
        const int canvasWidth = 600;
        const int canvasHeight = 900;

        var playtimeText = !string.IsNullOrEmpty(game.Playtime?.Time) ? game.Playtime.Time : "0min";
        var unlockedPercent = GetAchievementPercent(game);
        var badgeText = unlockedPercent.HasValue ? $"🏆 {Math.Round(unlockedPercent.Value * 100)}%" : null;

        var svgElement = new XElement(SvgNs + "svg",
            new XAttribute("xmlns", SvgNs.NamespaceName),
            new XAttribute("width", canvasWidth),
            new XAttribute("height", canvasHeight),
            new XAttribute("viewBox", $"0 0 {canvasWidth} {canvasHeight}"),

            new XElement(SvgNs + "defs",
                new XElement(SvgNs + "linearGradient",
                    new XAttribute("id", "fade"),
                    new XAttribute("x1", "0"), new XAttribute("y1", "0"),
                    new XAttribute("x2", "0"), new XAttribute("y2", "1"),
                    new XElement(SvgNs + "stop", new XAttribute("offset", "50%"), new XAttribute("stop-color", "transparent")),
                    new XElement(SvgNs + "stop", new XAttribute("offset", "95%"), new XAttribute("stop-color", "#111"))
                ),
                new XElement(SvgNs + "filter",
                    new XAttribute("id", "shadow"),
                    new XElement(SvgNs + "feDropShadow",
                        new XAttribute("dx", "0"), new XAttribute("dy", "2"),
                        new XAttribute("stdDeviation", "3"), new XAttribute("flood-opacity", ".45")
                    )
                )
            ),

            // Cover Image
            new XElement(SvgNs + "image",
                new XAttribute("href", imageCover),
                new XAttribute("width", "600"), new XAttribute("height", "900"),
                new XAttribute("preserveAspectRatio", "xMidYMid slice")
            ),

            // Gradient
            new XElement(SvgNs + "rect",
                new XAttribute("width", "600"), new XAttribute("height", "900"),
                new XAttribute("fill", "url(#fade)")
            ),

            // Playtime
            new XElement(SvgNs + "text",
                new XAttribute("x", "10"), new XAttribute("y", "870"),
                new XAttribute("fill", "#ffffff"), new XAttribute("font-size", "50"),
                new XAttribute("font-weight", "600"), new XAttribute("text-anchor", "start"),
                new XAttribute("font-family", "system-ui, -apple-system, sans-serif"),
                playtimeText
            )
        );

        if (badgeText != null)
        {
            svgElement.Add(new XElement(SvgNs + "text",
                new XAttribute("x", "590"), new XAttribute("y", "870"),
                new XAttribute("fill", "white"), new XAttribute("font-size", "50"),
                new XAttribute("font-weight", "bold"), new XAttribute("text-anchor", "end"),
                new XAttribute("font-family", "system-ui, -apple-system, sans-serif"),
                badgeText
            ));
        }

        var doc = new XDocument(new XComment(Disclaimer), svgElement);
        return doc.ToString();
    }

    private static double? GetAchievementPercent(SteamGame game)
    {
        if (game.AllAchievements == null || game.AllAchievements.Count == 0) return null;
        var unlockedCount = game.UnlockedAchievements?.Count ?? 0;
        return (double)unlockedCount / game.AllAchievements.Count;
    }

    private static async Task<(string DataUrl, int Width, int Height)> ImageToBase64Async(string url)
    {
        try
        {
            var response = await Client.GetAsync(url);
            var buffer = await response.Content.ReadAsByteArrayAsync();
            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

            var width = 0;
            var height = 0;

            if (mimeType == "image/png" && buffer.Length >= 24)
            {
                width  = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(16));
                height = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(20));
            }
            else if ((mimeType == "image/jpeg" || mimeType == "image/jpg") && buffer.Length > 8)
            {
                var i = 0;
                while (i < buffer.Length - 8)
                {
                    if (buffer[i] == 0xFF && (buffer[i + 1] == 0xC0 || buffer[i + 1] == 0xC2))
                    {
                        height = BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan(i + 5));
                        width  = BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan(i + 7));
                        break;
                    }
                    i++;
                }
            }

            var base64 = Convert.ToBase64String(buffer);
            var dataUrl = $"data:{mimeType};base64,{base64}";

            return (dataUrl, width, height);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Image to base64 error: {ex.Message}");
            return (string.Empty, 0, 0);
        }
    }

    private static string LerpColor(string colorA, string colorB, double t)
    {
        var (rA, gA, bA) = HexToRgb(colorA);
        var (rB, gB, bB) = HexToRgb(colorB);

        var r = (int)(rA + (rB - rA) * t);
        var g = (int)(gA + (gB - gA) * t);
        var b = (int)(bA + (bB - bA) * t);

        return RgbToHex(r, g, b);
    }

    private static (int R, int G, int B) HexToRgb(string hex)
    {
        var clean = hex.Replace("#", "");
        var bigint = Convert.ToInt32(clean, 16);
        return ((bigint >> 16) & 255, (bigint >> 8) & 255, bigint & 255);
    }

    private static string RgbToHex(int r, int g, int b)
    {
        return $"#{Math.Clamp(r, 0, 255):X2}{Math.Clamp(g, 0, 255):X2}{Math.Clamp(b, 0, 255):X2}";
    }
}

public class GameImages
{
    public string Hero { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Cover { get; set; } = string.Empty;
}

public class PlaytimeInfo
{
    public string Time { get; set; } = string.Empty;
}