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

- [x] Environments and variables `{{var}}`, `{{$shared var}}`, `{{$dotenv var}}`, `{{$processEnv var}}`
- [ ] Parse and invoke REST Client files `.http`, `.rest`
    - [x] Add `Invoke-RestHttp`
    - [x] Support file variables
    - [x] Support GraphQL variables
    - [x] Support GraphQL and content files
    - [ ] Support GraphQL selected operations

## See also

- [PSRest Release Notes](https://github.com/nightroman/PSRest/blob/main/Release-Notes.md)
- [VSCode REST Client](https://github.com/Huachao/vscode-restclient)
