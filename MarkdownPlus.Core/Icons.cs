using MarkdownPlus.Markdown.Ast;

namespace MarkdownPlus.Core;

public static class Icons
{
    public static AstNode Fork => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/repo-forked-24.svg");
    public static AstNode ForkInverted => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/feed-forked-24.svg");
    public static AstNode Tag => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/tag-24.svg");
    public static AstNode TagFill => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/feed-tag-24.svg");
    public static AstNode Star => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/star-24.svg");
    public static AstNode StarFill => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/star-fill-24.svg");
    public static AstNode StarInverted => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/feed-star-24.svg");
    public static AstNode Heart => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/heart-24.svg");
    public static AstNode HeartFill => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/heart-fill-24.svg");
    public static AstNode People => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/people-24.svg");
    public static AstNode Repo => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/repo-24.svg");
    public static AstNode RepoInverted => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/feed-repo-24.svg");
    public static AstNode Stack => CreateIconNode("https://raw.githubusercontent.com/primer/octicons/main/icons/stack-24.svg");

    private static HtmlElementNode CreateIconNode(string url) => new HtmlElementNode()
        {
            TagName = "img",
            Attributes =
            {
                ["src"] = url,
                ["width"] = "24px",
            },
            TrailingLineBreak = false,
        };
}
