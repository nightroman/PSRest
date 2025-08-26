# PSRest Release Notes

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
