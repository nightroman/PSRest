<#
.Synopsis
	Help script, https://github.com/nightroman/Helps
#>

Set-StrictMode -Version 3
Import-Module PSRest

### Set-RestEnvironment
@{
	command = 'Set-RestEnvironment'
	synopsis = 'Sets the current environment.'
	description = @'
	...
'@
	parameters = @{
		Name = '...'
		Path = '...'
	}
}

### Resolve-RestVariable
@{
	command = 'Get-RestVariable'
	synopsis = 'Gets the specified variable.'
	description = @'
	...
'@
	parameters = @{
		Name = '...'
		Type = '...'
		Environment = '...'
	}
}

### Resolve-RestVariable
@{
	command = 'Resolve-RestVariable'
	synopsis = 'Expands variables in input strings.'
	description = @'
'@
	parameters = @{
		Value = '...'
		Environment = '...'
	}
}
