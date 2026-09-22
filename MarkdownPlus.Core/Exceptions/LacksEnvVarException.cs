namespace MarkdownPlus.Core.Exceptions;

public class LacksEnvVarException(string envVar, string format) : Exception(
    $"Expected environment variable '{envVar}' not found. Please define '{envVar}={format}'")
{
    public readonly string EnvVar = envVar;
    public readonly string Format = format;
}
