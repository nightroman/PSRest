<#
.Synopsis
	Assorted tests.
#>

Set-StrictMode -Version 3
Import-Module PSRest

task export {
	$data = Import-PowerShellDataFile ..\src\Content\PSRest.psd1
	$cmdlets = (Get-Command -Module PSRest -CommandType Cmdlet).ForEach('Name')
	foreach($$ in $data.CmdletsToExport) {
		assert ($$ -in $cmdlets) "Exported cmdlet '$$' is missing."
	}
}

task help {
	foreach($cmd in Get-Command -Module PSRest) {
		$r = Get-Help $cmd
		if (!$r.Synopsis.EndsWith('.')) {
			Write-Warning "$($cmd.CommandType) '$cmd': missing synopsis or its period."
		}
	}
}
