using MarkdownPlus.Core;
using MarkdownPlus.Github;
using MarkdownPlus.LastFm;
using MarkdownPlus.Steam;
using MarkdownPlus.Wakatime;

namespace MarkdownPlus;

public static class ModulesHandler
{
    private static ModuleDescriptor[] _modules = [
        new WakatimeModule(),
        new GithubModule(),
        new LastFmModule(),
        new SteamModule(),
    ];

    public static IReadOnlyDictionary<string, ModuleDescriptor.ProcessNodeDelegate> Delegates { get; private set; } = null!;
    public static IReadOnlyDictionary<string, string> LoadedEnvironmentVariables { get; private set; } = null!;

    public static void InitModules()
    {
        Dictionary<string, ModuleDescriptor.ProcessNodeDelegate> delegates = new();
        Dictionary<string, string> resolvedEnvVars = new();

        HashSet<string> foundVars = [];
        HashSet<string> missingRequiredVars = [];

        foreach (var module in _modules)
        {
            foreach (var (envVar, optional) in module.EnvironmentVariables)
            {
                var value = Environment.GetEnvironmentVariable(envVar);

                if (!string.IsNullOrEmpty(value))
                {
                    resolvedEnvVars[envVar] = value;
                    foundVars.Add(envVar);
                }
                else if (!optional)
                {
                    missingRequiredVars.Add(envVar);
                }
            }
            
            foreach (var (tagName, tagDelegate) in module.Tags)
            {
                delegates.Add(tagName, tagDelegate);
            }
        }
        
        if (missingRequiredVars.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✔ Found environment variables:");
            if (foundVars.Count > 0)
            {
                foreach (var varName in foundVars)
                    Console.WriteLine($"  - {varName}");
            }
            else
            {
                Console.WriteLine("  (none)");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✖ Missing required environment variables:");
            foreach (var varName in missingRequiredVars)
            {
                Console.WriteLine($"  - {varName}");
            }

            Console.ResetColor();

            throw new InvalidOperationException("Module initialization failed due to missing environment variables.");
        }

        Delegates = delegates;
        LoadedEnvironmentVariables = resolvedEnvVars;
    }
}
