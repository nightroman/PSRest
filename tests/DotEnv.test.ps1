<#
.Synopsis
	Tests Import-RestDotEnv.
#>

Set-StrictMode -Version 3
Import-Module PSRest

Enter-Build {
	$env_user = $env:user
	$env:user = $null
}

Exit-Build {
	$env:user = $env_user
}

task import_AsDictionary_exact {
	$r = Import-RestDotEnv http/.env -AsDictionary

	equals $env:user $null

	#! TakeLast
	equals $r.user admin

	equals $r.continentsRegex A
}

task import_AsDictionary_traverse {
	Set-Location http/.vscode
	$r = Import-RestDotEnv -AsDictionary

	equals $env:user $null
	equals $r.user admin
}

task import_AsKeyValue_exact {
	$1, $2, $3 = Import-RestDotEnv http/.env -AsKeyValue

	equals $env:user $null

	equals $1.Key user
	equals $1.Value ignored

	equals $2.Key user
	equals $2.Value admin

	equals $3.Key continentsRegex
	equals $3.Value A
}

task import_AsKeyValue_traverse {
	Set-Location http/.vscode
	$1, $2, $3 = Import-RestDotEnv -AsKeyValue

	equals $env:user $null
	equals $1.Key user
	equals $1.Value ignored
}

task fail_on_specified_missing_file {
	try { throw Import-RestDotEnv .env }
	catch { equals "$_" "Missing file: '$(Join-Path $PSScriptRoot .env)'." }
}
