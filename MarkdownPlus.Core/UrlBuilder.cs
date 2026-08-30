using System.Net;
using System.Text;

namespace MarkdownPlus.Core;

public readonly record struct UrlBuilder(string BaseUrl)
{
    public Dictionary<string, string> Query { get; init; } = new();
    public UrlBuilder(string BaseUrl, Dictionary<string, string> Query) : this(BaseUrl) => this.Query = Query;

    public override string ToString()
    {
        if (Query.Count == 0)
            return BaseUrl;

        var sb = new StringBuilder(BaseUrl);

        sb.Append(BaseUrl.Contains('?') ? '&' : '?');

        var first = true;

        foreach (var (key, value) in Query)
        {
            if (!first) sb.Append('&');
            first = false;

            sb.Append(WebUtility.UrlEncode(key));
            sb.Append('=');
            sb.Append(WebUtility.UrlEncode(value));
        }

        return sb.ToString();
    }
    public UrlBuilder Copy() => new (BaseUrl) {Query = new Dictionary<string, string>(Query)};
}
