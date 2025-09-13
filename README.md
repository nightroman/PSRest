# PSRest

> VSCode REST Client features in PowerShell.

## Install

Get [PSRest](https://www.powershellgallery.com/packages/PSRest) module from PSGallery:

```powershell
Install-Module PSRest
```

## Features

- Invoking requests from REST Client files `.http`, `.rest`.
- Environment, file, prompt, request, system variables.
- GraphQL with variables and selected operations.
- External HTTP content or GraphQL source files.
- Automatic calls of referenced named requests.

## Commands

- `Invoke-RestHttp`
- `Set-RestEnvironment`
- `Get-RestVariable`
- `Resolve-RestVariable`
- `Import-RestDotEnv`
- `Reset-Rest`

## Variables

> See [VSCode REST Client] for details.

**Environment and file (user defined)**

- `{{variableName}}` and `{{$shared variableName}}`
- `{{$processEnv [%]envVarName}}`
- `{{$dotenv [%]variableName}}`

**System (dynamic)**

- `{{$guid}}`
- `{{$randomInt min max}}`
- `{{$timestamp [offset option]}}`
- `{{$datetime rfc1123|iso8601 [offset option]}}`
- `{{$localDatetime rfc1123|iso8601 [offset option]}}`

**Prompt (input)**

- `# @prompt var [prompt]`
- `// @prompt var [prompt]`

**Request**

- `# @name requestName`
- `// @name requestName`
- `{{requestName.(response|request).(body|headers).(*|JSONPath|XPath|Header Name)}}`

## Differences with REST Client

The main difference: PSRest is CLI, REST Client is GUI. PSRest is designed for
getting request results not just for viewing. This implies some stricter rules.

PSRest fails on undefined variables instead of leaving them unresolved by REST
Client like `{{var}}`. Unresolved variables cause cryptic invalid input errors
in some cases and various processing issues in others.

PSRest GraphQL operations support header `X-GraphQL-Operation: operationName`
for the exact operation in multi-operation GraphQL. REST Client invokes the
first operation.

Request variables JSONPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.

Request variables XPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.
- Element with children inner XML: PSRest trims spaces, REST Client does not.

PSRest automatically calls required named requests, REST Client does not.

## See also

- [PSRest Release Notes](https://github.com/nightroman/PSRest/blob/main/Release-Notes.md)
- [VSCode REST Client]

[VSCode REST Client]: https://github.com/Huachao/vscode-restclient#readme
