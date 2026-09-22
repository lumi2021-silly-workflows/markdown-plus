namespace MarkdownPlus.Core.Exceptions;

public class AuthException(LacksEnvVarException[] innerExceptions) : Exception
{
    public LacksEnvVarException[] InnerExceptions = innerExceptions;
}
