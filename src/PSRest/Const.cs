
namespace PSRest;

static class Const
{
    public const string DotEnvFile = ".env";
    public const string SettingsFile = "settings.json";
    public const string SettingsKeyRoot = "rest-client.environmentVariables";
    public const string VarRestEnvironment = "RestEnvironment";
    public const string VSCodeDir = ".vscode";

    public const string EnvNameShared = "$shared";
    public const string EnvVarDefault = "REST_ENV";

    public const string MediaTypeJson = "application/json";

    public const string VarTypeShared = "$shared";
    public const string VarTypeDotEnv = "$dotenv";
    public const string VarTypeProcessEnv = "$processEnv";

    public const string XGraphQLOperation = "X-GraphQL-Operation";
    public const string XRequestType = "X-Request-Type";
}
