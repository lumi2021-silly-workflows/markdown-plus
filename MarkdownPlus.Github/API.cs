using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarkdownPlus.Github;

public class API
{
    public sealed record GithubUserStats(
        string Username, string AvatarUrl, GithubAccountType Type, DateTime JoinedAt,
        int Followers, int Following, int Repositories, long StorageKb, int Releases,
        long Stars, long Forks);
    
    public class ActivityItem
    {
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int CommitCount { get; set; }
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string RepoNameWithOwner { get; set; } = string.Empty;
        public string RepoUrl { get; set; } = string.Empty;
    }
    public enum GithubAccountType
    {
        User,
        Organization
    }
    
    sealed private class GithubAccount
    {
        [JsonPropertyName("login")]
        public string Login { get; init; } = null!;

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; init; } = null!;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; init; }

        [JsonPropertyName("followers")]
        public int Followers { get; init; }

        [JsonPropertyName("following")]
        public int Following { get; init; }

        [JsonPropertyName("public_repos")]
        public int PublicRepositories { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; } = null!;
    }
    sealed private class GithubRepository
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("size")]
        public long Size { get; init; }

        [JsonPropertyName("stargazers_count")]
        public long StargazersCount { get; init; }

        [JsonPropertyName("forks_count")]
        public long ForksCount { get; init; }

        [JsonPropertyName("owner")]
        public GithubRepositoryOwner Owner { get; init; } = null!;
    }
    sealed private class GithubRepositoryOwner
    {
        [JsonPropertyName("login")]
        public string Login { get; init; } = null!;
    }
    sealed private class GithubRelease
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }
    }
    
    public static async Task<List<ActivityItem>> GetActivityAsync(string token, string username, CancellationToken cancellationToken = default)
    {
        var query = $$"""
                      query {
                        user(login: "{{username}}") {
                          contributionsCollection {
                            commitContributionsByRepository {
                              repository {
                                nameWithOwner
                                url
                              }
                              contributions(first: 25) {
                                nodes {
                                  commitCount
                                  occurredAt
                                }
                              }
                            }
                            pullRequestContributions(first: 10) {
                              nodes {
                                occurredAt
                                pullRequest {
                                  number
                                  title
                                  url
                                  state
                                  repository {
                                    name
                                    nameWithOwner
                                    url
                                  }
                                }
                              }
                            }
                            issueContributions(first: 10) {
                              nodes {
                                occurredAt
                                issue {
                                  number
                                  title
                                  url
                                  state
                                  repository {
                                    name
                                    nameWithOwner
                                    url
                                  }
                                }
                              }
                            }
                            pullRequestReviewContributions(first: 10) {
                              nodes {
                                pullRequestReview {
                                  state
                                  url
                                }
                                occurredAt
                              }
                            }
                          }
                        }
                      }
                      """;

        var response = await GraphqlFetchAsync(query, token);
        if (response.RootElement.TryGetProperty("errors", out var errors))
            throw new Exception(errors.GetRawText());

        var cc = response.RootElement.GetProperty("data").GetProperty("user").GetProperty("contributionsCollection");
        var list = new List<ActivityItem>();
        
        var weeklyCommits = new Dictionary<string, (DateTime Date, int Count)>();

        foreach (var repo in cc.GetProperty("commitContributionsByRepository").EnumerateArray())
        {
            foreach (var node in repo.GetProperty("contributions").GetProperty("nodes").EnumerateArray())
            {
                var date = node.GetProperty("occurredAt").GetDateTime();
                var count = node.GetProperty("commitCount").GetInt32();
                var weekKey = GetWeekKey(date);

                if (!weeklyCommits.TryGetValue(weekKey, out var value))
                {
                    weeklyCommits[weekKey] = (date, count);
                }
                else
                {
                    var (dateTime, i) = value;
                    weeklyCommits[weekKey] = (Date: dateTime, i + count);
                }
            }
        }

        foreach (var entry in weeklyCommits.Values)
        {
            list.Add(new ActivityItem
            {
                Type = "commit",
                Date = entry.Date,
                CommitCount = entry.Count
            });
        }

        // Pull Requests
        foreach (var node in cc.GetProperty("pullRequestContributions").GetProperty("nodes").EnumerateArray())
        {
            var pr = node.GetProperty("pullRequest");
            var repo = pr.GetProperty("repository");
            list.Add(new ActivityItem
            {
                Type = "pull_request",
                Date = node.GetProperty("occurredAt").GetDateTime(),
                Number = pr.GetProperty("number").GetInt32(),
                Title = pr.GetProperty("title").GetString() ?? string.Empty,
                Url = pr.GetProperty("url").GetString() ?? string.Empty,
                State = pr.GetProperty("state").GetString() ?? string.Empty,
                RepoNameWithOwner = repo.GetProperty("nameWithOwner").GetString() ?? string.Empty,
                RepoUrl = repo.GetProperty("url").GetString() ?? string.Empty
            });
        }

        // Issues
        foreach (var node in cc.GetProperty("issueContributions").GetProperty("nodes").EnumerateArray())
        {
            var issue = node.GetProperty("issue");
            var repo = issue.GetProperty("repository");
            list.Add(new ActivityItem
            {
                Type = "issue",
                Date = node.GetProperty("occurredAt").GetDateTime(),
                Number = issue.GetProperty("number").GetInt32(),
                Title = issue.GetProperty("title").GetString() ?? string.Empty,
                Url = issue.GetProperty("url").GetString() ?? string.Empty,
                State = issue.GetProperty("state").GetString() ?? string.Empty,
                RepoNameWithOwner = repo.GetProperty("nameWithOwner").GetString() ?? string.Empty,
                RepoUrl = repo.GetProperty("url").GetString() ?? string.Empty
            });
        }

        // Reviews
        foreach (var node in cc.GetProperty("pullRequestReviewContributions").GetProperty("nodes").EnumerateArray())
        {
            var review = node.GetProperty("pullRequestReview");
            list.Add(new ActivityItem
            {
                Type = "review",
                Date = node.GetProperty("occurredAt").GetDateTime(),
                Url = review.GetProperty("url").GetString() ?? string.Empty,
                State = review.GetProperty("state").GetString() ?? string.Empty
            });
        }

        return list.OrderByDescending(x => x.Date).ToList();
    }

    public static async Task<GithubUserStats> GetGithubUserStatsAsync(string username, string token, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.github.com");

        client.DefaultRequestHeaders.UserAgent.ParseAdd("MarkdownPlus-App");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var account = await GetAsync<GithubAccount>(client, $"/users/{Uri.EscapeDataString(username)}", cancellationToken);
        var accountType = account.Type switch
        {
            "User" => GithubAccountType.User,
            "Organization" => GithubAccountType.Organization,
            _ => throw new InvalidOperationException($"Tipo de conta GitHub desconhecido: {account.Type}"),
        };
        
        var repositories = new List<GithubRepository>();
        const int perPage = 100;

        for (var page = 1; ; page++)
        {
            var endpoint = accountType switch
            {
                GithubAccountType.User =>
                    $"/users/{Uri.EscapeDataString(username)}/repos" +
                    $"?visibility=public&per_page={perPage}&page={page}",

                GithubAccountType.Organization =>
                    $"/orgs/{Uri.EscapeDataString(username)}/repos" +
                    $"?type=public&per_page={perPage}&page={page}",

                _ => throw new UnreachableException()
            };

            var pageRepositories = await GetAsync<List<GithubRepository>>(client, endpoint, cancellationToken);

            if (pageRepositories.Count == 0) break;

            repositories.AddRange(pageRepositories);

            if (pageRepositories.Count < perPage)
                break;
        }
        
        long stars = 0;
        long forks = 0;
        long storageKb = 0;

        foreach (var repository in repositories)
        {
            stars += repository.StargazersCount;
            forks += repository.ForksCount;
            storageKb += repository.Size;
        }
        
        var releases = 0;

        foreach (var repository in repositories)
        {
            releases += await GetReleaseCountAsync(
                client,
                repository.Owner.Login,
                repository.Name,
                cancellationToken);
        }
        
        return new GithubUserStats(
            username,
            account.AvatarUrl,
            accountType,
            account.CreatedAt,
            account.Followers,
            account.Following,
            repositories.Count,
            storageKb,
            releases,
            stars,
            forks
        );
        
        async Task<int> GetReleaseCountAsync(HttpClient client, string owner, string repository, CancellationToken cancellationToken)
        {
            var endpoint =
                $"/repos/{Uri.EscapeDataString(owner)}/" +
                $"{Uri.EscapeDataString(repository)}/releases" +
                "?per_page=1";

            using var response = await client.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var releases = await response.Content.ReadFromJsonAsync<List<GithubRelease>>(cancellationToken);

            if (releases is null || releases.Count == 0)
                return 0;
            
            if (!response.Headers.TryGetValues("Link", out var linkValues))
                return releases.Count;

            var linkHeader = string.Join(",", linkValues);
            
            var lastPage = GetLastPageFromLinkHeader(linkHeader);
            return lastPage ?? releases.Count;
        }
        
        int? GetLastPageFromLinkHeader(string linkHeader)
        {
            const string relation = "rel=\"last\"";

            foreach (var part in linkHeader.Split(','))
            {
                if (!part.Contains(relation, StringComparison.Ordinal)) continue;

                var start = part.IndexOf('<');
                var end = part.IndexOf('>');

                if (start < 0 || end <= start) continue;
                var url = part[(start + 1)..end];
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) continue;

                var query = uri.Query.TrimStart('?');

                foreach (var parameter in query.Split('&'))
                {
                    var pair = parameter.Split('=', 2);
                    if (pair.Length != 2) continue;
                    if (!pair[0].Equals("page", StringComparison.OrdinalIgnoreCase)) continue;
                    if (int.TryParse(pair[1], out var page)) return page;
                }
            }

            return null;
        }
    }
    
    private static async Task<JsonDocument> GraphqlFetchAsync(string query, string token, object? variables = null)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MarkdownPlus-App");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = JsonSerializer.Serialize(new { query, variables });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("https://api.github.com/graphql", content);
        response.EnsureSuccessStatusCode();

        var text = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(text);
    }
    private static async Task<T> GetAsync<T>(HttpClient client, string endpoint, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        return result ?? throw new InvalidOperationException($"GitHub returned an empty response: {endpoint}");
    }
    
    private static string GetWeekKey(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = date.AddDays(-1 * diff).Date;
        return monday.ToString("yyyy-MM-dd");
    }
}
