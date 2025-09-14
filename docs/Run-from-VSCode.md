# Run from VSCode

> Open an HTTP file in the editor and invoke it as described.

Ensure VSCode PowerShell extension is installed.

Use `PowerShell: Show Extension Terminal` from the command palette `View / Command Palette...` or `Ctrl+Shift+P`.
The extension terminal is needed for typing commands and viewing output of registered commands.

> Ensure using PowerShell extension terminal, not PowerShell console terminals (not suitable for this).

## Invoke in extension terminal

Type this command in the extension terminal:

```powershell
$e = $psEditor.GetEditorContext(); Invoke-RestHttp $e.CurrentFile.Path -Tag $e.CursorPosition.Line
```

## Invoke registered command

Create or open your VSCode profile (see `$profile` in the extension terminal) and add this code:

```powershell
Register-EditorCommand -Name HTTP -DisplayName 'Invoke-RestHttp' -ScriptBlock {
    $e = $psEditor.GetEditorContext()
    Invoke-RestHttp $e.CurrentFile.Path -Tag $e.CursorPosition.Line
}
```

Use `PowerShell: Show Additional Commands` from the command palette.
Select the registered command and view its terminal output.

To make this easier, add some key to VSCode `keybindings.json`:

```json
{ "key": "ctrl+k i", "command": "PowerShell.ShowAdditionalCommands" },
```

## See also

- [Run-from-Visual-Studio](Run-from-Visual-Studio.md)
