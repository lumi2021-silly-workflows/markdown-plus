using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MarkdownPlus.LastFm;

internal static class CardGenerator
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private static readonly HttpClient Client = new();
    
    private static readonly ConcurrentDictionary<string, Lazy<Task<EmbeddedImage>>> ImageCache = new();

    private const int Width = 480;
    private const int HeaderHeight = 76;
    private const int RowHeight = 68;

    internal sealed record TrackEntry(
        string Name,
        string Artist,
        string TrackUrl,
        string ArtistUrl,
        string CoverUrl,
        int DurationSec);

    private sealed record EmbeddedImage(string DataUri, string Hash);

    public static async Task<XElement> CreateTrackSvgAsync(TrackEntry track, int index)
    {
        var image = await GetEmbeddedImageAsync(track.CoverUrl);

        XNamespace xlink = "http://www.w3.org/1999/xlink";

        return new XElement(
            Svg + "svg",
            new XAttribute("width", 360),
            new XAttribute("height", 80),
            new XAttribute("viewBox", "0 0 360 80"),
            
            // Index
            new XElement(
                Svg + "text",
                new XAttribute("x", 8),
                new XAttribute("y", 45),
                new XAttribute("fill", "#8b949e"),
                new XAttribute("font-family", "sans-serif"),
                new XAttribute("font-size", 14),
                new XAttribute("font-weight", 600),
                index.ToString()
            ),
            
            // Cover
            new XElement(
                Svg + "image",
                new XAttribute("x", 28),
                new XAttribute("y", 8),
                new XAttribute("width", 64),
                new XAttribute("height", 64),
                new XAttribute("href", image.DataUri),
                new XAttribute(xlink + "href", image.DataUri),
                new XAttribute("preserveAspectRatio", "xMidYMid slice")
            ),

            // Track name
            new XElement(
                Svg + "text",
                new XAttribute("x", 102),
                new XAttribute("y", 31),
                new XAttribute("fill", "#8b949e"),
                new XAttribute("font-family", "sans-serif"),
                new XAttribute("font-size", 14),
                new XAttribute("font-weight", 600),
                Truncate(track.Name, 34)
            ),

            // Artist
            new XElement(
                Svg + "text",
                new XAttribute("x", 102),
                new XAttribute("y", 51),
                new XAttribute("fill", "#8b949e"),
                new XAttribute("font-family", "sans-serif"),
                new XAttribute("font-size", 12),
                Truncate(track.Artist, 34)
            ),

            // Duration
            new XElement(
                Svg + "text",
                new XAttribute("x", 102),
                new XAttribute("y", 70),
                new XAttribute("fill", "#8b949e"),
                new XAttribute("font-family", "sans-serif"),
                new XAttribute("font-size", 11),
                FormatDuration(track.DurationSec)
            )
        );
    }

    private static async Task<EmbeddedImage> GetEmbeddedImageAsync(string imageUrl)
    {
        var normalizedUrl = NormalizeArtworkUrl(imageUrl);
        var lazy = ImageCache.GetOrAdd(normalizedUrl,
            url => new Lazy<Task<EmbeddedImage>>(() => DownloadImageAsync(url)));

        try
        {
            return await lazy.Value;
        }
        catch
        {
            ImageCache.TryRemove(new KeyValuePair<string, Lazy<Task<EmbeddedImage>>>(normalizedUrl, lazy));

            return await DownloadImageAsync(
                "https://raw.githubusercontent.com/lumi2021-silly-workflows/"
                + "markdown-plus/refs/heads/main/MarkdownPlus.LastFm/assets/song-no-cover.png");
        }
    }

    private static async Task<EmbeddedImage> DownloadImageAsync(string url)
    {
        using var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        var mediaType = response.Content.Headers.ContentType?.MediaType ?? "image/png";

        if (!mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The response is not an image.");

        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        var dataUri = $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}";

        return new EmbeddedImage(dataUri, hash);
    }

    private static string NormalizeArtworkUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Host.EndsWith("mzstatic.com", StringComparison.OrdinalIgnoreCase))
            return Regex.Replace(url, @"\d+x\d+bb(?=\.)", "600x600bb", RegexOptions.IgnoreCase);

        return url;
    }

    private static string FormatDuration(int seconds) =>
        seconds <= 0 ? "--:--" : $"{seconds / 60}:{seconds % 60:D2}";

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
}
