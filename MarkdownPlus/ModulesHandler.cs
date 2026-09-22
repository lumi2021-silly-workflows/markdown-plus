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

        foreach (var module in _modules)
        {
            foreach (var envVar in module.EnvironmentVariables)
            {
                var value = Environment.GetEnvironmentVariable(envVar);

                if (!string.IsNullOrEmpty(value))
                {
                    resolvedEnvVars[envVar] = value;
                    foundVars.Add(envVar);
                }
            }
            
            foreach (var (tagName, tagDelegate) in module.Tags)
            {
                delegates.Add(tagName, tagDelegate);
            }
        }
        
        Delegates = delegates;
        LoadedEnvironmentVariables = resolvedEnvVars;
    }
}

