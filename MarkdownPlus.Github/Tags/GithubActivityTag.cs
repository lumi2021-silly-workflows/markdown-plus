using MarkdownPlus.Core.Exceptions;
using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Github;

public partial class GithubTags
{
    public static async Task<AstNode> GithubTagProcess(HtmlElementNode node, IReadOnlyDictionary<string, string> envVars)
    {
        var (token, username) = Auth(envVars);
        
        logger.Info("Requesting github's activity data...");
        var activity = await API.GetActivityAsync(token, username);
        var data = activity.Take(10).ToList();
        logger.Info("Processing github's activity data...");

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

        return list;
    }
}
