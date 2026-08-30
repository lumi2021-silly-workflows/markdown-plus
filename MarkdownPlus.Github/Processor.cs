using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MarkdownPlus.Core;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

public static class Processor
{
    private static readonly ModuleLogger logger = new ModuleLogger("Github Service");

    public static async Task<AstNode[]> GithubTagProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var token = envVars[Constants.API_TOKEN_VAR];
        var username = envVars[Constants.USERNAME_VAR];
        
        logger.Info("Requesting github's contribution data...");
        var contributions = await GetContributionsAsync(token, username);
        var data = contributions.Take(10).ToList();
        logger.Info("Processing github's contribution data...");

        var content = new List<ListItemNode>();

        foreach (var e in data)
        {
            switch (e.Type)
            {
                case "commit":
                    content.Add(new ListItemNode
                    {
                        Children = [
                            new ParagraphNode($"✏️ Made {e.CommitCount} {(e.CommitCount == 1 ? "commit" : "commits")}")
                        ],
                    });
                    break;

                case "pull_request":
                    switch (e.State)
                    {
                        case "OPEN":
                            content.Add(new ListItemNode
                            {
                                Children = [
                                    new ParagraphNode { Inlines = [
                                        new TextNode($"↗️ Opened pull request "),
                                        new LinkNode($"#{e.Number}", e.Url),
                                        new TextNode(" in "),
                                        new LinkNode(e.RepoNameWithOwner, e.RepoUrl),
                                    ]},
                                ],
                            });
                            break;
                        case "CLOSED":
                            content.Add(new ListItemNode
                            {
                                Children = [
                                    new ParagraphNode { Inlines = [
                                        new TextNode($"❌ Closed pull request "),
                                        new LinkNode($"#{e.Number}", e.Url),
                                        new TextNode(" in "),
                                        new LinkNode(e.RepoNameWithOwner, e.RepoUrl),
                                    ]},
                                ],
                            });
                            break;
                        case "MERGED":
                            content.Add(new ListItemNode
                            {
                                Children = [
                                    new ParagraphNode { Inlines = [
                                        new TextNode($"🎉 Merged pull request "),
                                        new LinkNode($"#{e.Number}", e.Url),
                                        new TextNode(" in "),
                                        new LinkNode(e.RepoNameWithOwner, e.RepoUrl),
                                    ]},
                                ],
                            });
                            break;
                        default:
                            logger.Error($"Unknown pr state \"{e.State}\"");
                            break;
                    }
                    break;

                case "issue":
                    switch (e.State)
                    {
                        case "OPEN":
                            content.Add(new ListItemNode
                            {
                                Children = [
                                    new ParagraphNode { Inlines = [
                                        new TextNode($"️ Opened issue "),
                                        new LinkNode($"#{e.Number}", e.Url),
                                        new TextNode(" in "),
                                        new LinkNode(e.RepoNameWithOwner, e.RepoUrl),
                                    ]},
                                ],
                            });
                            break;
                        case "CLOSED":
                            content.Add(new ListItemNode
                            {
                                Children = [
                                    new ParagraphNode { Inlines = [
                                        new TextNode($"️✅ Closed issue "),
                                        new LinkNode($"#{e.Number}", e.Url),
                                        new TextNode(" in "),
                                        new LinkNode(e.RepoNameWithOwner, e.RepoUrl),
                                    ]},
                                ],
                            });
                            break;
                        default:
                            logger.Error($"Unknown issue state \"{e.State}\"");
                            break;
                    }
                    break;

                default:
                    logger.Error($"Unknown github contribution type \"{e.Type}\"");
                    break;
            }
        }

        var list = new ListNode { Items = content };

        return [list];
    }
    
    private class ContributionItem
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

    private static async Task<List<ContributionItem>> GetContributionsAsync(string token, string username)
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

        var jsonString = await GraphqlFetchAsync(query, token);
        using var doc = JsonDocument.Parse(jsonString);

        if (doc.RootElement.TryGetProperty("errors", out var errors))
            throw new Exception(errors.GetRawText());

        var cc = doc.RootElement.GetProperty("data").GetProperty("user").GetProperty("contributionsCollection");
        var list = new List<ContributionItem>();
        
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
            list.Add(new ContributionItem
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
            list.Add(new ContributionItem
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
            list.Add(new ContributionItem
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
            list.Add(new ContributionItem
            {
                Type = "review",
                Date = node.GetProperty("occurredAt").GetDateTime(),
                Url = review.GetProperty("url").GetString() ?? string.Empty,
                State = review.GetProperty("state").GetString() ?? string.Empty
            });
        }

        return list.OrderByDescending(x => x.Date).ToList();
    }

    private static async Task<string> GraphqlFetchAsync(string query, string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MarkdownPlus-App");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = JsonSerializer.Serialize(new { query });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("https://api.github.com/graphql", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static string GetWeekKey(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = date.AddDays(-1 * diff).Date;
        return monday.ToString("yyyy-MM-dd");
    }
}
