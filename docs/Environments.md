# Environments

Environments are defined in the same files as used by VSCode REST Client or Visual Studio.

## `http-client.env.json` or `.vscode/settings.json`

PSRest looks for `http-client.env.json` or `.vscode/settings.json` files in the current location and then in parent directories.
`http-client.env.json` or `.vscode/settings.json` have the same environment entries structure but use different root paths:

- `http-client.env.json` uses `$`, the JSON root object.
- `.vscode/settings.json` uses `$.rest-client.environmentVariables`, the JSON root object property `rest-client.environmentVariables`.

Examples: [http-client.env.json](../tests/http/http-client.env.json), [.vscode/settings.json](../tests/http/.vscode/settings.json)

# `.env`

HTTP file variables like  `{{$dotEnv varName}}` are loaded from `.env` in the same directory as the HTTP file.

Example: [.env](../tests/http/.env)

## See also

- [README](README.md)
- [VSCode REST Client](https://github.com/Huachao/vscode-restclient)
- [Use .http files in Visual Studio](https://learn.microsoft.com/en-us/aspnet/core/test/http-files)
