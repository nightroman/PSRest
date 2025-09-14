# Environments

Environments are defined in files as used by Visual Studio or VSCode REST Client.

## `http-client.env.json` or `.vscode/settings.json`

PSRest looks for `http-client.env.json` or `.vscode/settings.json` in the
current location, then in parent directories, and uses the first found.
Files have the same environment objects but use different root entries:

- `http-client.env.json` uses `$`, JSON root.
- `.vscode/settings.json` uses `$.rest-client.environmentVariables`, JSON property.

Examples: [http-client.env.json](../tests/http/http-client.env.json), [.vscode/settings.json](../tests/http/.vscode/settings.json)

# `.env`

HTTP file variables `{{$dotEnv varName}}` are loaded from `.env` in the same directory as the HTTP file.

Example: [.env](../tests/http/.env)

## See also

- [VSCode REST Client](https://github.com/Huachao/vscode-restclient)
- [Use .http files in Visual Studio](https://learn.microsoft.com/en-us/aspnet/core/test/http-files)
