using System.Text.Json.Serialization;

namespace MarkdownPlus.Github;

internal enum GithubAccountType
{
    User,
    Organization
}

sealed internal record GithubUserStats(
    string Username, string AvatarUrl, GithubAccountType Type, DateTime JoinedAt,
    int Followers, int Following, int Repositories, long StorageKb, int Releases,
    long Stars, long Forks);

sealed internal record GithubRepositoryStats(
    string Owner, string Name, string? Description, string? Language,
    long Stars, long Forks, string HtmlUrl);

sealed internal class ActivityItem
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

sealed internal class GithubAccount
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
sealed internal class GithubRepository
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
    
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("size")]
    public long Size { get; init; }

    [JsonPropertyName("stargazers_count")]
    public long StargazersCount { get; init; }

    [JsonPropertyName("forks_count")]
    public long ForksCount { get; init; }
    
    [JsonPropertyName("html_url")]
    public required string HtmlUrl { get; init; }

    [JsonPropertyName("private")]
    public bool IsPrivate { get; init; }

    [JsonPropertyName("owner")]
    public GithubRepositoryOwner Owner { get; init; } = null!;
}
sealed internal class GithubRepositoryOwner
{
    [JsonPropertyName("login")]
    public string Login { get; init; } = null!;
}
sealed internal class GithubRelease
{
    [JsonPropertyName("id")]
    public long Id { get; init; }
}
