namespace PSRest;

static class Const
{
    public const string MyName = "PSRest";

    public const string DotEnvFile = ".env";
    public const string SettingsFile1 = "http-client.env.json";
    public const string SettingsFile2 = ".vscode/settings.json";
    public const string SettingsRoot = "rest-client.environmentVariables";

    public const string DatetimeRfc1123 = "rfc1123";
    public const string DatetimeIso8601 = "iso8601";

    public const string EnvNameShared = "$shared";
    public const string EnvVarDefault = "REST_ENV";
    public const string EnvVarTag = "REST_TAG";
    public const string VarRestEnvironment = "RestEnvironment";

    public const string MediaContentType = "Content-Type";
    public const string MediaTypeJson = "application/json";
    public const string MediaTypeXml = "application/xml";

    public const string SystemGuid = "$guid";
    public const string SystemRandomInt = "$randomInt";
    public const string SystemTimestamp = "$timestamp";
    public const string SystemDatetime = "$datetime";
    public const string SystemLocalDatetime = "$localDatetime";

    public const string VarTypeShared = "$shared";
    public const string VarTypeDotEnv = "$dotenv";
    public const string VarTypeProcessEnv = "$processEnv";

    public const string XGraphQLOperation = "X-GraphQL-Operation";
    public const string XRequestType = "X-Request-Type";
}
