using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Wakatime;

public static class Processor
{
    public static async Task<AstNode[]> WakatimeTagProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var apiKey = envVars[Constants.API_KEY_VAR];
        
        using var client = new HttpClient();
        var authTokenBytes = Encoding.UTF8.GetBytes(apiKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authTokenBytes));

        var response = await client.GetAsync("https://wakatime.com/api/v1/users/current/stats/last_7_days");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonString);
        var data = doc.RootElement.GetProperty("data");

        var content = new StringBuilder();
        
        content.AppendLine($"Total Time: {data.GetProperty("human_readable_total").GetString()}");
        content.AppendLine();

        var languages = data.GetProperty("languages");
        var levels = "⣿⣷⣶⣦⣤⣄⣀";

        var limit = Math.Min(5, languages.GetArrayLength());
        for (var i = 0; i < limit; i++)
        {
            var item = languages[i];
            var name = item.GetProperty("name").GetString();
            var percent = item.GetProperty("percent").GetDouble() / 100.0;
            var text = item.GetProperty("text").GetString();

            var line = "- ";
            line += ('"' + Truncate(name, 13) + '"').PadRight(16);
            line += ProgressBar(percent, 30, levels) + " ";
            line += text;

            content.AppendLine(line);
        }

        var codeBlock = new CodeBlockNode
        {
            Language = "rust",
            Code     = content.ToString().TrimEnd(),
        };

        return [codeBlock];
    }
    
    private static string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;

        return str.Substring(0, maxLength - 3) + "...";
    }

    private static string ProgressBar(double percent, int width, string levels = "# ")
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
