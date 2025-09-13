
function Start-Server {
	try { $null = Invoke-RestMethod http://[::1]:55001 }
	catch {
		Start-Process pwsh "$PSScriptRoot/server.ps1"
		Start-Sleep 1
	}
}
