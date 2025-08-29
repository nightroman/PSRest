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

## Roadmap

- [x] Environments and file, environment, system variables
    - `{{variableName}}` and `{{$shared variableName}}`
    - `{{$processEnv [%]envVarName}}`
    - `{{$dotenv [%]variableName}}`
- [ ] Parse and invoke REST Client files `.http`, `.rest`
    - [x] Add `Invoke-RestHttp`
    - [x] Support file variables
    - [x] Support GraphQL variables
    - [x] Support GraphQL and content files
    - [x] Support GraphQL selected operations
    - [x] Format some XML responses
    - [ ] Add more system variables
    - [ ] Support prompt variables

## See also

- [PSRest Release Notes](https://github.com/nightroman/PSRest/blob/main/Release-Notes.md)
- [VSCode REST Client](https://github.com/Huachao/vscode-restclient)
