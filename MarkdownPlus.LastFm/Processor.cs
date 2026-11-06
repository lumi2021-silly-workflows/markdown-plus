using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Xml;
using MarkdownPlus.Core;
using MarkdownPlus.Core.Caching;
using MarkdownPlus.Core.Exceptions;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.LastFm;

public static class Processor
{
    private static readonly ModuleLogger logger = new("Last.Fm Service");
    private static readonly HttpClient Client = new();

    private static readonly ICacheManager cache = Cache.GetServiceCache("lastfm");

    private static readonly ConcurrentDictionary<string, Lazy<Task<JsonArray?>>> TrackTasks = new();

    private const string FallbackCover =
        "https://raw.githubusercontent.com/lumi2021-silly-workflows/markdown-plus/"
        + "refs/heads/main/MarkdownPlus.LastFm/assets/song-no-cover.png";

    public static async Task<AstNode> LastfmTagProcessor(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var username = envVars.GetValueOrDefault(Constants.USERNAME_VAR);
        var apiKey = envVars.GetValueOrDefault(Constants.API_KEY_VAR);

        if (username == null || apiKey == null)
        {
            List<LacksEnvVarException> exceptions = [];

            if (username == null)
                exceptions.Add(new LacksEnvVarException(
                    Constants.USERNAME_VAR,
                    "<your last.fm username>"));

            if (apiKey == null)
                exceptions.Add(new LacksEnvVarException(
                    Constants.API_KEY_VAR,
                    "<your last.fm API key>"));

            throw new AuthException([.. exceptions]);
        }

        var max = int.TryParse(node.Attributes.GetValueOrDefault("max", "5"), out var value) ? value : 5;
        
        logger.Info("Loading Last.fm top tracks...");
        
        var taskKey = $"{username}:{apiKey}";

        var tracks = await TrackTasks
            .GetOrAdd(taskKey, _ => new Lazy<Task<JsonArray?>>(() => FetchLastFmTopTracksAsync(username, apiKey, max)))
            .Value;

        if (tracks == null || tracks.Count == 0) return new ParagraphNode("No tracks found");

        logger.Info("Loading tracks' cover images...");

        List<(string url, string path, string alt)> entries = [];

        foreach (var (i, track) in tracks.Index())
        {
            var trackObj = track?.AsObject();
            if (trackObj == null) continue;
            
            var artistObj = trackObj["artist"]?.AsObject();
            var artistName = artistObj?["name"]?.ToString() ?? "Unknown Artist";
            var trackName = trackObj["name"]?.ToString() ?? "Unknown Track";
            var artistUrl = artistObj?["url"]?.ToString() ?? string.Empty;
            var trackUrl = trackObj["url"]?.ToString() ?? string.Empty;
            var mbid = trackObj["mbid"]?.ToString() ?? string.Empty;
            
            var (coverUrl, durationSec) = await FetchItunesMetadataAsync(artistName, trackName);

            if (string.IsNullOrWhiteSpace(coverUrl)) coverUrl = FallbackCover;

            var entry = new CardGenerator.TrackEntry(trackName, artistName, trackUrl, artistUrl, coverUrl, durationSec);
            var svg = await CardGenerator.CreateTrackSvgAsync(entry, i + 1);
            
            var resourceKey = $"lastfm-top-tracks-{username}-{mbid}-{i}";
            var alt = $"{trackName} - {artistName}";
            
            cache.TouchResource(resourceKey, "svg", DateTimeOffset.UtcNow.AddHours(23));
            await cache.SetContentAsync(resourceKey, svg.DumpString());
            entries.Add((trackUrl, cache.GetPath(resourceKey), alt));
        }

        var body = HtmlElementNode.Div();
        foreach (var (url, path, alt) in entries)
        {
            body.Children.Add(HtmlElementNode.A(url, HtmlElementNode.Img(path, alt)));
            body.Children.Add(HtmlElementNode.Br());
        }
        return body;
    }

    private static async Task<JsonArray?> FetchLastFmTopTracksAsync(string username, string apiKey, int count)
    {
        var url = new UrlBuilder(
            "https://ws.audioscrobbler.com/2.0/",
            new Dictionary<string, string>
            {
                ["method"] = "user.gettoptracks",
                ["user"] = Uri.EscapeDataString(username),
                ["api_key"] = Uri.EscapeDataString(apiKey),
                ["format"] = "json",
                ["limit"] = $"{count}",
            }).ToString();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                using var response = await Client.GetAsync(url);
                var statusCode = (int)response.StatusCode;

                if (statusCode is 500 or 502 or 503 or 504)
                {
                    await Task.Delay(1000 * (int)Math.Pow(2, attempt));
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonNode.Parse(jsonString);

                return result?["error"] != null
                    ? throw new InvalidOperationException($"Last.fm error {result["error"]}: {result["message"]}")
                    : result?["toptracks"]?["track"]?.AsArray();
            }
            catch (HttpRequestException) when (attempt < 4)
            {
                await Task.Delay(1000 * (int)Math.Pow(2, attempt));
            }
            catch (TaskCanceledException) when (attempt < 4)
            {
                await Task.Delay(1000 * (int)Math.Pow(2, attempt));
            }
        }

        return null;
    }

    private static async Task<(string CoverUrl, int DurationSec)> FetchItunesMetadataAsync(string artistName, string trackName)
    {
        try
        {
            var searchTerm = Uri.EscapeDataString($"{artistName} {trackName}");
            var searchUrl = $"https://itunes.apple.com/search?term={searchTerm}&entity=song&limit=1";

            using var response = await Client.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();
 
            var jsonStr = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(jsonStr) ?? throw new InvalidOperationException("Not a valid json");
            var count = json["resultCount"]!.GetValue<int>();
            var results = json["results"]!;

            if (count > 0 && results[0] != null)
            {
                var result = results[0];
                var coverUrl = result["artworkUrl30"]?.ToString() ?? string.Empty;
                var durationMs = result["trackTimeMillis"]?.GetValue<long>() ?? 0;
                return (coverUrl, (int)(durationMs / 1000));
            }
        }
        catch (HttpRequestException)
        {
            // Complementary source
        }
        catch (System.Text.Json.JsonException)
        {
            // Ignore invalid json
        }

        return (string.Empty, 0);
    }
}
