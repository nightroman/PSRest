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
- [ ] Parse and invoke REST Client files `.http` / `.rest` (GraphQL and HTTP requests)
    - [x] Add `Invoke-RestHttp`
    - [x] Support file variables
    - [ ] Support GraphQL variables
    - [ ] Other content types in addition to JSON
    - [ ] Support request body or GraphQL in files

## See also

- [PSRest Release Notes](https://github.com/nightroman/PSRest/blob/main/Release-Notes.md)
- [VSCode REST Client](https://github.com/Huachao/vscode-restclient)
