
function Start-Server {
	try { $null = Invoke-RestMethod http://127.0.0.1:55001 }
	catch {
		Start-Process pwsh "$PSScriptRoot/server.ps1" -WindowStyle Minimized
		Start-Sleep 1
	}
}
