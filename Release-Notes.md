# PSRest Release Notes

## v0.5.0

New cmdlet `Import-RestDotEnv`.

## v0.4.2

Add system variables:
- `{{$guid}}`
- `{{$randomInt min max}}`
- `{{$timestamp [offset option]}}`
- `{{$datetime rfc1123|iso8601 [offset option]}}`
- `{{$localDatetime rfc1123|iso8601 [offset option]}}`

Fix `$dotenv` duplicates: take last.

## v0.4.1

Format some XML responses.

## v0.4.0

Support GraphQL operation name header `X-GraphQL-Operation: {operationName}`.\
REST Client feature request: https://github.com/Huachao/vscode-restclient/issues/1393

## v0.3.0

Support GraphQL and content files: `< {file}` (literal), `<@ {file}` (expanded variables).

## v0.2.0

`Set-RestEnvironment`
- Support GraphQL variables.
- Fix variables and output issues.

## v0.1.3

`Set-RestEnvironment`: new parameters `DotEnvFile`, `SettingsFile`.

Use case insensitive request headers.

## v0.1.2

Support and use HTTP version in request line.

## v0.1.1

Support file variables.

Fix a few issues and REST Client differences.

`Invoke-RestHttp` does not require `Set-RestEnvironment`.

## v0.1.0

New cmdlet `Invoke-RestHttp` (.http, .rest) invokes the first HTTP request with
JSON or GraphQL body with no file variables and no GraphQL variables, for now.

`Set-RestEnvironment` parameter `Name` default is `$env:REST_ENV`.

## v0.0.3

Commands help and other tweaks.

## v0.0.2

Require `Set-RestEnvironment` before other commands.

Avoid .env imported to process environment.

## v0.0.1

Commands:

- `Set-RestEnvironment`
- `Get-RestVariable`
- `Resolve-RestVariable`
