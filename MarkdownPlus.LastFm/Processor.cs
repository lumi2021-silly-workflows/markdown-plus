using System.Text.Json.Nodes;
using MarkdownPlus.Core;
using MarkdownPlus.Core.Exceptions;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.LastFm;

public static class Processor
{
    private static readonly ModuleLogger logger = new("Last.Fm Service");
    private static readonly HttpClient Client = new();
    private static Task<JsonArray?>? _lastFmTask;
    
    public static async Task<AstNode[]> LastfmTagProcessor(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var username = envVars.GetValueOrDefault(Constants.USERNAME_VAR);
        var apiKey = envVars.GetValueOrDefault(Constants.API_KEY_VAR);

        if (username == null || apiKey == null)
        {
            List<LacksEnvVarException> exceptions = [];
            if (username == null) exceptions.Add(new LacksEnvVarException(Constants.USERNAME_VAR, "<your last.fm username>"));
            if (apiKey == null) exceptions.Add(new LacksEnvVarException(Constants.API_KEY_VAR, "<your last.fm API key>"));
            throw new AuthException([..exceptions]);
        }

        logger.Info("Loading Last.fm top tracks...");
        var tracks = await FetchLastFmTopTracksAsync(username, apiKey);

        if (tracks == null || tracks.Count == 0)
            return [new ParagraphNode("No tracks found")];

        logger.Info("Loading tracks' cover images...");
        var container = new HtmlElementNode
        {
            TagName = "p",
            SelfClosing = false,
            TrailingLineBreak = true,
        };
        
        foreach (var track in tracks)
        {
            var trackObj = track?.AsObject();
            if (trackObj == null) continue;

            var artistObj = trackObj["artist"]?.AsObject();
            var artistName = artistObj?["name"]?.ToString() ?? "Unknown Artist";
            var trackName = trackObj["name"]?.ToString() ?? "Unknown Track";
            var artistUrl = artistObj?["url"]?.ToString() ?? string.Empty;
            var trackUrl = trackObj["url"]?.ToString() ?? string.Empty;

            var (coverUrl, durationSec) = await FetchItunesMetadataAsync(artistName, trackName);

            if (string.IsNullOrEmpty(coverUrl))
            {
                coverUrl = "https://raw.githubusercontent.com/lumi2021-silly-workflows/markdown-plus/"
                    + "refs/heads/main/MarkdownPlus.LastFm/assets/song-no-cover.png";
            }

            var duration = "—-:--";
            if (durationSec > 0)
            {
                var minutes = durationSec / 60;
                var seconds = durationSec % 60;
                duration = $"{minutes}:{seconds:D2}";
            }

            var trackHLink = HtmlElementNode.CreateA(trackName, trackUrl);
            var artistHLink = HtmlElementNode.CreateA(artistName, artistUrl);

            var div = new HtmlElementNode
            {
                TagName           = "div",
                TrailingLineBreak = false,
                SelfClosing       = false,
                Attributes        = { { "style", "clear: both; padding: 10px 0;" } },
                Children =
                {
                    new HtmlElementNode
                    {
                        TagName           = "img",
                        TrailingLineBreak = false,
                        SelfClosing       = true,
                        Attributes =
                        {
                            { "src", coverUrl },
                            { "width", "60" },
                            { "align", "left" },
                        },
                    },
                    new HtmlElementNode
                    {
                        TagName           = "p",
                        TrailingLineBreak = false,
                        SelfClosing       = false,
                        Children =
                        {
                            new HtmlElementNode
                            {
                                TagName           = "strong",
                                TrailingLineBreak = false,
                                SelfClosing       = false,
                                Children          = { trackHLink }
                            },
                            new HtmlTextNode($" • "),
                            artistHLink,
                        },
                    },
                    new HtmlElementNode
                    {
                        TagName           = "strong",
                        TrailingLineBreak = false,
                        SelfClosing       = false,
                        Attributes = { {"clear", "left"} },
                        Children = { new HtmlTextNode(duration) },
                    },
                },
            };
            container.Children.Add(div);
        }

        return [container];
    }
    
    private static Task<JsonArray?> FetchLastFmTopTracksAsync(string username, string apiKey)
    {
        _lastFmTask ??= Task.Run(async () =>
        {
            var url = new UrlBuilder(
                "https://ws.audioscrobbler.com/2.0/",
                new Dictionary<string, string>
                {
                    { "method", $"user.gettoptracks" },
                    { "user", $"{Uri.EscapeDataString(username)}" },
                    { "api_key", $"{Uri.EscapeDataString(apiKey)}" },
                    { "format", "json" },
                    { "limit", "5" }
                }
            ).ToString();

            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    var response = await Client.GetAsync(url);
                    var statusCode = (int)response.StatusCode;

                    if (statusCode is 500 or 502 or 503 or 504)
                    {
                        await Task.Delay(1000 * (int)Math.Pow(2, attempt));
                        continue;
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    var node = JsonNode.Parse(jsonString);

                    if (node?["error"] != null)
                    {
                        throw new Exception($"Last.fm error {node["error"]}: {node["message"]}");
                    }

                    return node?["toptracks"]?["track"]?.AsArray();
                }
                catch when (attempt < 4)
                {
                    // Continua a tentar nas primeiras 4 falhas
                }
            }

            return null;
        });

        return _lastFmTask;
    }

    private static async Task<(string CoverUrl, int DurationSec)> FetchItunesMetadataAsync(string artistName, string trackName)
    {
        try
        {
            var searchTerm = Uri.EscapeDataString($"{artistName} {trackName}");
            var searchUrl = $"https://itunes.apple.com/search?term={searchTerm}&entity=song&limit=1";

            var response = await Client.GetStringAsync(searchUrl);
            var node = JsonNode.Parse(response);
            var firstResult = node?["results"]?[0];

            if (firstResult != null)
            {
                var coverUrl = firstResult["artworkUrl60"]?.ToString() ?? string.Empty;
                var trackTimeMillis = firstResult["trackTimeMillis"]?.GetValue<long>() ?? 0;
                var durationSec = (int)(trackTimeMillis / 1000);

                return (coverUrl, durationSec);
            }
        }
        catch
        {
            // ignored
        }

        return (string.Empty, 0);
    }
}
