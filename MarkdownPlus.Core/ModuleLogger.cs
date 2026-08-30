namespace MarkdownPlus.Core;

public class ModuleLogger(string serviceName)
{
    private readonly string _prefix = $"[{serviceName}]";

    public void Info(string message) => Log(message, ConsoleColor.Gray);
    public void Success(string message) => Log(message, ConsoleColor.Green);
    public void Warn(string message) => Log(message, ConsoleColor.Yellow);
    public void Error(string message) => Log(message, ConsoleColor.Red);

    private void Log(string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"{_prefix} {message}");
        Console.ForegroundColor = originalColor;
    }
}
