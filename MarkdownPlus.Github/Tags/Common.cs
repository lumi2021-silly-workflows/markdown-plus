using MarkdownPlus.Core;
using MarkdownPlus.Core.Exceptions;

namespace MarkdownPlus.Github;

public partial class GithubTags
{
    private static readonly ModuleLogger logger = new("Github Service");
    private static (string token, string username)? AuthInfo = null;
    
    private static (string token, string username) Auth(IReadOnlyDictionary<string, string> envVars)
    {
        if (AuthInfo.HasValue) return AuthInfo.Value;
        
        var token = envVars.GetValueOrDefault(Constants.API_TOKEN_VAR);
        var username = envVars.GetValueOrDefault(Constants.USERNAME_VAR);
        
        if (token == null || username == null)
        {
            List<LacksEnvVarException> exceptions = [];
            if (token == null) exceptions.Add(new LacksEnvVarException(Constants.API_TOKEN_VAR, "<your github API token>"));
            if (username == null) exceptions.Add(new LacksEnvVarException(Constants.USERNAME_VAR, "<your github username>"));
            throw new AuthException([..exceptions]);
        }

        AuthInfo = (token, username);
        return AuthInfo.Value;
    }
}
