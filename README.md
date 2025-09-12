# PSRest

> VSCode REST Client features in PowerShell.

## Install

Get [PSRest](https://www.powershellgallery.com/packages/PSRest) module from PSGallery:

```powershell
Install-Module PSRest
```

## Commands

- `Invoke-RestHttp`
- `Set-RestEnvironment`
- `Get-RestVariable`
- `Resolve-RestVariable`
- `Import-RestDotEnv`

## Variables

Environment and file (user defined)

- `{{variableName}}` and `{{$shared variableName}}`
- `{{$processEnv [%]envVarName}}`
- `{{$dotenv [%]variableName}}`

System (dynamic)

- `{{$guid}}`
- `{{$randomInt min max}}`
- `{{$timestamp [offset option]}}`
- `{{$datetime rfc1123|iso8601 [offset option]}}`
- `{{$localDatetime rfc1123|iso8601 [offset option]}}`

Prompt (input)

- `# @prompt var [prompt]`
- `// @prompt var [prompt]`

Request

- `# @name requestName`
- `// @name requestName`
- `{{requestName.(response|request).(body|headers).(*|XPath|Header Name)}}`

## Roadmap

- [x] Environments and file, environment, system variables
- [ ] Parse and invoke REST Client files `.http`, `.rest`
    - [x] Add `Invoke-RestHttp`
    - [x] Support file variables
    - [x] Support GraphQL variables
    - [x] Support GraphQL and content files
    - [x] Support GraphQL selected operations
    - [x] Format some XML responses
    - [x] Support prompt variables
    - [ ] Support request variables

## Differences with REST Client

The main difference: PSRest is CLI, REST Client is GUI. PSRest is designed for
interactive and non-interactive calls. The latter implies some stricter rules.

PSRest fails on undefined variables instead of leaving them unresolved by
REST Client like `{{var}}`. Unresolved variables may cause cryptic invalid
input errors or later processing issues and some troubleshooting.

PSRest GraphQL operations support header `X-GraphQL-Operation: operationName`
for the exact operation in multi-operation GraphQL. REST Client invokes the
first operation.

Request variables XPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.
- Element with children inner XML: PSRest trims, REST Client gets as is.

## See also

- [PSRest Release Notes](https://github.com/nightroman/PSRest/blob/main/Release-Notes.md)
- [VSCode REST Client](https://github.com/Huachao/vscode-restclient)
