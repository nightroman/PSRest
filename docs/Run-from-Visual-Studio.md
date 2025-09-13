# Run from Visual Studio

## External tool

- Menu `Tools / External Tools...`
- Click `Add`
- Fill the form

| Field             | Text                                               | Notes                          |
| ----------------- | -------------------------------------------------- | ------------------------------ |
| Title             | `Invoke-RestHttp`                                  | you may assign a hotkey by `&` |
| Command           | `pwsh.exe`                                         | assuming in the system path    |
| Arguments         | `-c Invoke-RestHttp "$(ItemPath)" -Tag $(CurLine)` |                                |
| Use Output window | `ON`                                               |                                |

## NuGet Package Manager console

- Menu `Tools / NuGet Package Manager / Package Manager console`
- Type this command

```powershell
pwsh -c Invoke-RestHttp $dte.ActiveDocument.FullName -Tag $dte.ActiveDocument.Selection.CurrentLine
```
