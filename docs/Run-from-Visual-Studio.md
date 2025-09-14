# Run from Visual Studio

> Two ways to invoke the current request from the editor

Open an HTTP file in the editor and invoke it as external tool or Package Manager console command.

## External tool

- Menu `Tools / External Tools`
- Click `Add`
- Set:

| Field             | Text                                               | Notes                          |
| ----------------- | -------------------------------------------------- | ------------------------------ |
| Title             | `Invoke-RestHttp`                                  | you may assign a hotkey by `&` |
| Command           | `pwsh.exe`                                         | assuming in the path           |
| Arguments         | `-c Invoke-RestHttp "$(ItemPath)" -Tag $(CurLine)` |                                |
| Use Output window | `Yes`                                              |                                |

## NuGet Package Manager console

- Menu `Tools / NuGet Package Manager / Package Manager console`
- Type this command

```powershell
pwsh -c Invoke-RestHttp $dte.ActiveDocument.FullName -Tag $dte.ActiveDocument.Selection.CurrentLine
```

## See also

- [Run-from-VSCode](Run-from-VSCode.md)
