<#
.Synopsis
	Common tests about commands.
#>

Set-StrictMode -Version 3
Import-Module PSRest

task exported_command_exists {
	$commands = (Get-Command -Module PSRest).ForEach('Name')
	$exported = @(
		$data = Import-PowerShellDataFile ..\src\Content\PSRest.psd1
		$data.AliasesToExport
		$data.CmdletsToExport
		$data.FunctionsToExport
	)
	foreach($_ in $exported) {
		assert ($_ -in $commands) "Missing exported command: '$_'."
	}
}

task command_help_synopsis {
	$commands = Get-Command -Module PSRest
	foreach($_ in $commands) {
		if (!(Get-Help $_).Synopsis.EndsWith('.')) {
			Write-Warning "$($_.CommandType) '$_': Missing synopsis or its period."
		}
	}
}
