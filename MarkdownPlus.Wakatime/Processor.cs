using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Wakatime;

public static class Processor
{
    public static async Task<AstNode> WakatimeTagProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var apiKey = envVars[Constants.API_KEY_VAR];

        var displayStyle = DisplayStyle.Block;
        var levels = node.Attributes.GetValueOrDefault("levels", "# ")!;

        if (node.Attributes.TryGetValue("style", out var styleString))
        {
            var style = StyleParser.Parse(styleString!);
            if (style.TryGetValue("display", out var displayString)) displayStyle = displayString switch
            {
                "code" => DisplayStyle.Code,
                _ => DisplayStyle.Block,
            };
        }
        
        using var client = new HttpClient();
        var authTokenBytes = Encoding.UTF8.GetBytes(apiKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authTokenBytes));

        var response = await client.GetAsync("https://wakatime.com/api/v1/users/current/stats/last_7_days");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonString);
        var data = doc.RootElement.GetProperty("data");

        return displayStyle switch
        {
            DisplayStyle.Block => await CardGenerator.GenerateDisplayBlock(data),
            DisplayStyle.Code => CardGenerator.GenerateDisplayCode(data, levels),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    
}
